using System;
using System.Collections.Generic;
using System.IO;
using SoccerMobilePro.Platform;

namespace SoccerMobilePro.Catalog
{
    public interface IActiveCatalogStore
    {
        string ActiveVersion { get; }
        bool HasVersion(string catalogVersion);
        bool TryRead(string catalogVersion, out CatalogSnapshot snapshot);
        bool TryReadActive(out CatalogSnapshot snapshot);
        void Stage(CatalogSnapshot snapshot);
        void Activate(string catalogVersion);
        bool Rollback(string catalogVersion);
    }

    public sealed class FileCatalogStore : IActiveCatalogStore
    {
        private readonly string rootPath;
        private readonly ICatalogCodec codec;
        private readonly ICatalogValidator validator;

        public FileCatalogStore(string rootPath, ICatalogCodec codec, ICatalogValidator validator)
        {
            if (string.IsNullOrWhiteSpace(rootPath)) throw new ArgumentException("Catalog root path is required.", nameof(rootPath));
            this.rootPath = Path.GetFullPath(rootPath);
            this.codec = codec ?? throw new ArgumentNullException(nameof(codec));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            Directory.CreateDirectory(this.rootPath);
        }

        public string ActiveVersion
        {
            get
            {
                string pointer = PointerPath;
                return File.Exists(pointer) ? File.ReadAllText(pointer).Trim() : string.Empty;
            }
        }

        public bool HasVersion(string catalogVersion) => CatalogVersion.IsCanonical(catalogVersion) && File.Exists(SnapshotPath(catalogVersion));

        public bool TryRead(string catalogVersion, out CatalogSnapshot snapshot)
        {
            snapshot = null;
            if (!HasVersion(catalogVersion)) return false;
            try
            {
                CatalogSnapshot decoded = codec.DeserializeSnapshot(File.ReadAllText(SnapshotPath(catalogVersion)));
                if (!validator.ValidateSnapshot(decoded).Succeeded || decoded.CatalogVersion != catalogVersion) return false;
                snapshot = decoded;
                return true;
            }
            catch (IOException) { return false; }
            catch (CatalogFormatException) { return false; }
        }

        public bool TryReadActive(out CatalogSnapshot snapshot) => TryRead(ActiveVersion, out snapshot);

        public void Stage(CatalogSnapshot snapshot)
        {
            CatalogValidationResult result = validator.ValidateSnapshot(snapshot);
            if (!result.Succeeded) throw new CatalogStoreException("Cannot stage an invalid catalog.");
            string stagePath = StagePath(snapshot.CatalogVersion);
            WriteAtomically(stagePath, codec.SerializeSnapshot(snapshot));
        }

        public void Activate(string catalogVersion)
        {
            string stagePath = StagePath(catalogVersion);
            string snapshotPath = SnapshotPath(catalogVersion);
            if (File.Exists(stagePath))
            {
                if (File.Exists(snapshotPath)) File.Delete(snapshotPath);
                File.Move(stagePath, snapshotPath);
            }
            if (!TryRead(catalogVersion, out _)) throw new CatalogStoreException("Catalog version is missing or corrupt.");
            WriteAtomically(PointerPath, catalogVersion);
        }

        public bool Rollback(string catalogVersion)
        {
            if (!TryRead(catalogVersion, out _)) return false;
            WriteAtomically(PointerPath, catalogVersion);
            return true;
        }

        private string PointerPath => Path.Combine(rootPath, "active.version");
        private string SnapshotPath(string version) => Path.Combine(rootPath, version + ".catalog.json");
        private string StagePath(string version) => Path.Combine(rootPath, version + ".stage.json");

        private static void WriteAtomically(string path, string content)
        {
            string temporary = path + ".tmp";
            File.WriteAllText(temporary, content ?? string.Empty);
            if (File.Exists(path))
            {
                string backup = path + ".bak";
                if (File.Exists(backup)) File.Delete(backup);
                File.Replace(temporary, path, backup);
            }
            else File.Move(temporary, path);
        }
    }

    public enum CatalogInstallStatus
    {
        Installed,
        RolledBack,
        InvalidManifest,
        InvalidSignature,
        InvalidHash,
        InvalidPayload,
        InvalidDelta,
        MissingBase,
        MissingRollback,
        StoreFailure
    }

    public sealed class CatalogInstallResult
    {
        public CatalogInstallResult(CatalogInstallStatus status, string activeVersion, IReadOnlyList<CatalogValidationError> errors = null)
        {
            Status = status;
            ActiveVersion = activeVersion ?? string.Empty;
            Errors = errors ?? Array.Empty<CatalogValidationError>();
        }

        public CatalogInstallStatus Status { get; }
        public string ActiveVersion { get; }
        public IReadOnlyList<CatalogValidationError> Errors { get; }
        public bool Succeeded => Status == CatalogInstallStatus.Installed || Status == CatalogInstallStatus.RolledBack;
    }

    public sealed class CatalogInstaller
    {
        private const int BackwardSchemaWindow = 1;
        private readonly IActiveCatalogStore store;
        private readonly ICatalogCodec codec;
        private readonly ICatalogValidator validator;
        private readonly ICatalogSignatureVerifier signatureVerifier;
        private readonly CatalogDeltaApplier deltaApplier;

        public CatalogInstaller(IActiveCatalogStore store, ICatalogCodec codec, ICatalogValidator validator, ICatalogSignatureVerifier signatureVerifier)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.codec = codec ?? throw new ArgumentNullException(nameof(codec));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.signatureVerifier = signatureVerifier ?? throw new ArgumentNullException(nameof(signatureVerifier));
            deltaApplier = new CatalogDeltaApplier(codec, validator);
        }

        public CatalogInstallResult InstallSnapshot(CatalogManifest manifest, string payload, int clientSchemaVersion, DateTimeOffset nowUtc)
        {
            CatalogInstallStatus manifestStatus = ValidateManifest(manifest, payload, clientSchemaVersion, nowUtc);
            if (manifestStatus != CatalogInstallStatus.Installed) return Result(manifestStatus);
            try
            {
                CatalogSnapshot snapshot = codec.DeserializeSnapshot(payload);
                if (snapshot.CatalogVersion != manifest.CatalogVersion) return Result(CatalogInstallStatus.InvalidPayload);
                CatalogValidationResult validation = validator.ValidateSnapshot(snapshot);
                if (!validation.Succeeded) return new CatalogInstallResult(CatalogInstallStatus.InvalidPayload, store.ActiveVersion, validation.Errors);
                if (!string.IsNullOrEmpty(manifest.RollbackVersion) && !store.HasVersion(manifest.RollbackVersion)) return Result(CatalogInstallStatus.MissingRollback);
                store.Stage(snapshot);
                store.Activate(snapshot.CatalogVersion);
                return Result(CatalogInstallStatus.Installed);
            }
            catch (CatalogFormatException) { return Result(CatalogInstallStatus.InvalidPayload); }
            catch (IOException) { return Result(CatalogInstallStatus.StoreFailure); }
            catch (CatalogStoreException) { return Result(CatalogInstallStatus.StoreFailure); }
        }

        public CatalogInstallResult InstallDelta(CatalogManifest targetManifest, string deltaPayload, int clientSchemaVersion, DateTimeOffset nowUtc)
        {
            CatalogInstallStatus manifestStatus = ValidateManifest(targetManifest, deltaPayload, clientSchemaVersion, nowUtc);
            if (manifestStatus != CatalogInstallStatus.Installed) return Result(manifestStatus);
            try
            {
                CatalogDelta delta = codec.DeserializeDelta(deltaPayload);
                if (delta.TargetVersion != targetManifest.CatalogVersion) return Result(CatalogInstallStatus.InvalidDelta);
                if (!store.TryReadActive(out CatalogSnapshot active)) return Result(CatalogInstallStatus.MissingBase);
                CatalogSnapshot target = deltaApplier.Apply(active, delta);
                if (!string.IsNullOrEmpty(targetManifest.RollbackVersion) && !store.HasVersion(targetManifest.RollbackVersion)) return Result(CatalogInstallStatus.MissingRollback);
                store.Stage(target);
                store.Activate(target.CatalogVersion);
                return Result(CatalogInstallStatus.Installed);
            }
            catch (CatalogFormatException) { return Result(CatalogInstallStatus.InvalidDelta); }
            catch (CatalogDeltaException exception) { return new CatalogInstallResult(CatalogInstallStatus.InvalidDelta, store.ActiveVersion, exception.Errors); }
            catch (IOException) { return Result(CatalogInstallStatus.StoreFailure); }
            catch (CatalogStoreException) { return Result(CatalogInstallStatus.StoreFailure); }
        }

        public CatalogInstallResult Rollback(string catalogVersion)
        {
            return store.Rollback(catalogVersion)
                ? Result(CatalogInstallStatus.RolledBack)
                : Result(CatalogInstallStatus.MissingRollback);
        }

        private CatalogInstallStatus ValidateManifest(CatalogManifest manifest, string payload, int clientSchemaVersion, DateTimeOffset nowUtc)
        {
            if (manifest == null || !CatalogVersion.IsCanonical(manifest.CatalogVersion) || !manifest.IsEffective(nowUtc) || manifest.SchemaVersion > clientSchemaVersion || manifest.SchemaVersion < clientSchemaVersion - BackwardSchemaWindow)
                return CatalogInstallStatus.InvalidManifest;
            if (!string.Equals(CatalogIntegrity.ComputeSha256(payload), manifest.PayloadHash, StringComparison.Ordinal)) return CatalogInstallStatus.InvalidHash;
            if (!signatureVerifier.Verify(manifest)) return CatalogInstallStatus.InvalidSignature;
            return CatalogInstallStatus.Installed;
        }

        private CatalogInstallResult Result(CatalogInstallStatus status) => new CatalogInstallResult(status, store.ActiveVersion);
    }

    public sealed class CatalogStoreException : Exception
    {
        public CatalogStoreException(string message) : base(message) { }
    }
}
