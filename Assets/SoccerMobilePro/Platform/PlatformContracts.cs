using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;

namespace SoccerMobilePro.Platform
{
    public enum AccountAuthLevel
    {
        Guest = 0,
        Linked = 1
    }

    public enum SessionFailureReason
    {
        None = 0,
        Missing = 1,
        Expired = 2,
        Revoked = 3,
        AccountLinkConflict = 4,
        RateLimited = 5,
        InvalidRequest = 6
    }

    public sealed class AccountSession
    {
        public AccountSession(
            string accountId,
            string sessionId,
            AccountAuthLevel authLevel,
            string provider,
            DateTimeOffset issuedAtUtc,
            DateTimeOffset expiresAtUtc,
            string consentVersion,
            long serverRevision)
        {
            if (string.IsNullOrWhiteSpace(accountId)) throw new ArgumentException("Account ID is required.", nameof(accountId));
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentException("Session ID is required.", nameof(sessionId));
            if (expiresAtUtc <= issuedAtUtc) throw new ArgumentOutOfRangeException(nameof(expiresAtUtc));

            AccountId = accountId;
            SessionId = sessionId;
            AuthLevel = authLevel;
            Provider = provider ?? string.Empty;
            IssuedAtUtc = issuedAtUtc;
            ExpiresAtUtc = expiresAtUtc;
            ConsentVersion = consentVersion ?? string.Empty;
            ServerRevision = serverRevision;
        }

        public string AccountId { get; }
        public string SessionId { get; }
        public AccountAuthLevel AuthLevel { get; }
        public string Provider { get; }
        public DateTimeOffset IssuedAtUtc { get; }
        public DateTimeOffset ExpiresAtUtc { get; }
        public string ConsentVersion { get; }
        public long ServerRevision { get; }
        public bool CanUseOnlineEconomy => AuthLevel == AccountAuthLevel.Linked;
        public bool IsExpired(DateTimeOffset nowUtc) => nowUtc >= ExpiresAtUtc;
    }

    public sealed class LocalePreference
    {
        public LocalePreference(string locale, string voiceLocale, string source, DateTimeOffset changedAtUtc, int version)
        {
            Locale = string.IsNullOrWhiteSpace(locale) ? "vi-VN" : locale;
            VoiceLocale = string.IsNullOrWhiteSpace(voiceLocale) ? Locale : voiceLocale;
            Source = source ?? "fallback";
            ChangedAtUtc = changedAtUtc;
            Version = Math.Max(1, version);
        }

        public string Locale { get; }
        public string VoiceLocale { get; }
        public string Source { get; }
        public DateTimeOffset ChangedAtUtc { get; }
        public int Version { get; }
    }

    public sealed class SettingsSnapshot
    {
        private readonly IReadOnlyDictionary<string, string> values;
        private readonly IReadOnlyDictionary<string, string> deviceOverrides;

        public SettingsSnapshot(
            string accountId,
            int schemaVersion,
            long revision,
            IReadOnlyDictionary<string, string> values,
            IReadOnlyDictionary<string, string> deviceOverrides,
            DateTimeOffset updatedAtUtc)
        {
            AccountId = accountId ?? string.Empty;
            SchemaVersion = Math.Max(1, schemaVersion);
            Revision = Math.Max(0, revision);
            this.values = Copy(values);
            this.deviceOverrides = Copy(deviceOverrides);
            UpdatedAtUtc = updatedAtUtc;
        }

        public string AccountId { get; }
        public int SchemaVersion { get; }
        public long Revision { get; }
        public IReadOnlyDictionary<string, string> Values => values;
        public IReadOnlyDictionary<string, string> DeviceOverrides => deviceOverrides;
        public DateTimeOffset UpdatedAtUtc { get; }

        private static IReadOnlyDictionary<string, string> Copy(IReadOnlyDictionary<string, string> source)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            if (source != null)
            {
                foreach (KeyValuePair<string, string> item in source) result[item.Key] = item.Value;
            }

            return new ReadOnlyDictionary<string, string>(result);
        }
    }

    public readonly struct SessionOperationResult
    {
        private SessionOperationResult(bool succeeded, AccountSession session, SessionFailureReason failureReason, int retryAfterSeconds)
        {
            Succeeded = succeeded;
            Session = session;
            FailureReason = failureReason;
            RetryAfterSeconds = retryAfterSeconds;
        }

        public bool Succeeded { get; }
        public AccountSession Session { get; }
        public SessionFailureReason FailureReason { get; }
        public int RetryAfterSeconds { get; }

        public static SessionOperationResult Success(AccountSession session) => new SessionOperationResult(true, session, SessionFailureReason.None, 0);
        public static SessionOperationResult Failure(SessionFailureReason reason, int retryAfterSeconds = 0) => new SessionOperationResult(false, null, reason, Math.Max(0, retryAfterSeconds));
    }

    public interface ISessionRepository
    {
        SessionOperationResult CreateGuest(string deviceScopedAccountId, DateTimeOffset nowUtc, TimeSpan lifetime);
        SessionOperationResult GetCurrent(DateTimeOffset nowUtc);
        SessionOperationResult Refresh(string sessionId, DateTimeOffset nowUtc, TimeSpan lifetime);
        SessionOperationResult LinkGuest(string targetAccountId, string provider, DateTimeOffset nowUtc, TimeSpan lifetime);
        SessionOperationResult Revoke(string sessionId, DateTimeOffset nowUtc);
    }

    public interface ISecureCredentialVault
    {
        void Store(string accountId, string credential);
        bool TryRead(string accountId, out string credential);
        void Delete(string accountId);
    }

    public sealed class InMemoryCredentialVault : ISecureCredentialVault
    {
        private readonly Dictionary<string, string> credentials = new Dictionary<string, string>(StringComparer.Ordinal);

        public void Store(string accountId, string credential)
        {
            if (string.IsNullOrWhiteSpace(accountId) || string.IsNullOrEmpty(credential)) throw new ArgumentException("Credential key and value are required.");
            credentials[accountId] = credential;
        }

        public bool TryRead(string accountId, out string credential) => credentials.TryGetValue(accountId, out credential);
        public void Delete(string accountId) => credentials.Remove(accountId);
    }

    public sealed class FakeSessionRepository : ISessionRepository
    {
        private const int RefreshLimit = 3;
        private static readonly TimeSpan RefreshWindow = TimeSpan.FromMinutes(1);
        private readonly HashSet<string> linkedAccounts = new HashSet<string>(StringComparer.Ordinal);
        private readonly Queue<DateTimeOffset> refreshAttempts = new Queue<DateTimeOffset>();
        private readonly HashSet<string> revokedSessionIds = new HashSet<string>(StringComparer.Ordinal);
        private AccountSession current;
        private long revision;

        public void SeedLinkedAccount(string accountId) => linkedAccounts.Add(accountId);

        public SessionOperationResult CreateGuest(string deviceScopedAccountId, DateTimeOffset nowUtc, TimeSpan lifetime)
        {
            if (string.IsNullOrWhiteSpace(deviceScopedAccountId) || lifetime <= TimeSpan.Zero) return SessionOperationResult.Failure(SessionFailureReason.InvalidRequest);
            current = CreateSession(deviceScopedAccountId, AccountAuthLevel.Guest, "guest", nowUtc, lifetime);
            return SessionOperationResult.Success(current);
        }

        public SessionOperationResult GetCurrent(DateTimeOffset nowUtc)
        {
            if (current == null) return SessionOperationResult.Failure(SessionFailureReason.Missing);
            if (revokedSessionIds.Contains(current.SessionId)) return SessionOperationResult.Failure(SessionFailureReason.Revoked);
            if (current.IsExpired(nowUtc)) return SessionOperationResult.Failure(SessionFailureReason.Expired);
            return SessionOperationResult.Success(current);
        }

        public SessionOperationResult Refresh(string sessionId, DateTimeOffset nowUtc, TimeSpan lifetime)
        {
            while (refreshAttempts.Count > 0 && nowUtc - refreshAttempts.Peek() >= RefreshWindow) refreshAttempts.Dequeue();
            if (refreshAttempts.Count >= RefreshLimit) return SessionOperationResult.Failure(SessionFailureReason.RateLimited, 60);
            refreshAttempts.Enqueue(nowUtc);

            SessionOperationResult active = GetCurrent(nowUtc);
            if (!active.Succeeded || active.Session.SessionId != sessionId || lifetime <= TimeSpan.Zero) return active.Succeeded ? SessionOperationResult.Failure(SessionFailureReason.InvalidRequest) : active;
            current = CreateSession(current.AccountId, current.AuthLevel, current.Provider, nowUtc, lifetime);
            return SessionOperationResult.Success(current);
        }

        public SessionOperationResult LinkGuest(string targetAccountId, string provider, DateTimeOffset nowUtc, TimeSpan lifetime)
        {
            SessionOperationResult active = GetCurrent(nowUtc);
            if (!active.Succeeded) return active;
            if (active.Session.AuthLevel != AccountAuthLevel.Guest || string.IsNullOrWhiteSpace(targetAccountId) || string.IsNullOrWhiteSpace(provider) || lifetime <= TimeSpan.Zero)
                return SessionOperationResult.Failure(SessionFailureReason.InvalidRequest);
            if (linkedAccounts.Contains(targetAccountId)) return SessionOperationResult.Failure(SessionFailureReason.AccountLinkConflict);

            linkedAccounts.Add(targetAccountId);
            current = CreateSession(targetAccountId, AccountAuthLevel.Linked, provider, nowUtc, lifetime);
            return SessionOperationResult.Success(current);
        }

        public SessionOperationResult Revoke(string sessionId, DateTimeOffset nowUtc)
        {
            if (current == null || current.SessionId != sessionId) return SessionOperationResult.Failure(SessionFailureReason.Missing);
            revokedSessionIds.Add(sessionId);
            return SessionOperationResult.Failure(SessionFailureReason.Revoked);
        }

        private AccountSession CreateSession(string accountId, AccountAuthLevel authLevel, string provider, DateTimeOffset nowUtc, TimeSpan lifetime)
        {
            revision++;
            return new AccountSession(accountId, "fake-session-" + revision, authLevel, provider, nowUtc, nowUtc.Add(lifetime), "test-consent", revision);
        }
    }

    public enum CatalogReadStatus
    {
        Ready = 0,
        OfflineFallback = 1,
        Missing = 2,
        Expired = 3,
        Corrupt = 4,
        Incompatible = 5,
        InvalidSignature = 6
    }

    public sealed class CatalogManifest
    {
        public CatalogManifest(
            int schemaVersion,
            string catalogVersion,
            string region,
            string season,
            DateTimeOffset effectiveAtUtc,
            DateTimeOffset expiresAtUtc,
            string minClientVersion,
            string payloadHash,
            string signature,
            string rollbackVersion)
        {
            SchemaVersion = schemaVersion;
            CatalogVersion = catalogVersion ?? string.Empty;
            Region = region ?? string.Empty;
            Season = season ?? string.Empty;
            EffectiveAtUtc = effectiveAtUtc;
            ExpiresAtUtc = expiresAtUtc;
            MinClientVersion = minClientVersion ?? string.Empty;
            PayloadHash = payloadHash ?? string.Empty;
            Signature = signature ?? string.Empty;
            RollbackVersion = rollbackVersion ?? string.Empty;
        }

        public int SchemaVersion { get; }
        public string CatalogVersion { get; }
        public string Region { get; }
        public string Season { get; }
        public DateTimeOffset EffectiveAtUtc { get; }
        public DateTimeOffset ExpiresAtUtc { get; }
        public string MinClientVersion { get; }
        public string PayloadHash { get; }
        public string Signature { get; }
        public string RollbackVersion { get; }
        public bool IsEffective(DateTimeOffset nowUtc) => nowUtc >= EffectiveAtUtc && nowUtc < ExpiresAtUtc;
    }

    public readonly struct CatalogReadResult
    {
        public CatalogReadResult(CatalogReadStatus status, CatalogManifest manifest, string payload)
        {
            Status = status;
            Manifest = manifest;
            Payload = payload ?? string.Empty;
        }

        public CatalogReadStatus Status { get; }
        public CatalogManifest Manifest { get; }
        public string Payload { get; }
        public bool Succeeded => Status == CatalogReadStatus.Ready || Status == CatalogReadStatus.OfflineFallback;
    }

    public interface ICatalogRepository
    {
        CatalogReadResult GetActive(int clientSchemaVersion, DateTimeOffset nowUtc, bool isOnline);
    }

    public interface ICatalogSignatureVerifier
    {
        bool Verify(CatalogManifest manifest);
    }

    public sealed class FakeCatalogSignatureVerifier : ICatalogSignatureVerifier
    {
        public bool Verify(CatalogManifest manifest) => manifest != null && manifest.Signature == "test-signed:" + manifest.PayloadHash;
    }

    public static class CatalogIntegrity
    {
        public static string ComputeSha256(string payload)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (byte value in bytes) builder.Append(value.ToString("x2"));
                return builder.ToString();
            }
        }
    }

    public sealed class InMemoryCatalogRepository : ICatalogRepository
    {
        private const int BackwardSchemaWindow = 1;
        private readonly ICatalogSignatureVerifier signatureVerifier;
        private readonly List<CachedCatalog> cache = new List<CachedCatalog>();

        public InMemoryCatalogRepository(ICatalogSignatureVerifier signatureVerifier)
        {
            this.signatureVerifier = signatureVerifier ?? throw new ArgumentNullException(nameof(signatureVerifier));
        }

        public void SeedCache(CatalogManifest manifest, string payload)
        {
            if (manifest == null) throw new ArgumentNullException(nameof(manifest));
            cache.Add(new CachedCatalog(manifest, payload));
            cache.Sort((left, right) => string.CompareOrdinal(right.Manifest.CatalogVersion, left.Manifest.CatalogVersion));
        }

        public CatalogReadResult GetActive(int clientSchemaVersion, DateTimeOffset nowUtc, bool isOnline)
        {
            CatalogReadStatus lastFailure = CatalogReadStatus.Missing;
            foreach (CachedCatalog candidate in cache)
            {
                if (candidate.Manifest.SchemaVersion > clientSchemaVersion || candidate.Manifest.SchemaVersion < clientSchemaVersion - BackwardSchemaWindow)
                {
                    lastFailure = CatalogReadStatus.Incompatible;
                    continue;
                }

                if (!candidate.Manifest.IsEffective(nowUtc))
                {
                    lastFailure = CatalogReadStatus.Expired;
                    continue;
                }

                if (CatalogIntegrity.ComputeSha256(candidate.Payload) != candidate.Manifest.PayloadHash)
                {
                    lastFailure = CatalogReadStatus.Corrupt;
                    continue;
                }

                if (!signatureVerifier.Verify(candidate.Manifest))
                {
                    lastFailure = CatalogReadStatus.InvalidSignature;
                    continue;
                }

                return new CatalogReadResult(isOnline ? CatalogReadStatus.Ready : CatalogReadStatus.OfflineFallback, candidate.Manifest, candidate.Payload);
            }

            return new CatalogReadResult(lastFailure, null, string.Empty);
        }

        private sealed class CachedCatalog
        {
            public CachedCatalog(CatalogManifest manifest, string payload)
            {
                Manifest = manifest;
                Payload = payload ?? string.Empty;
            }

            public CatalogManifest Manifest { get; }
            public string Payload { get; }
        }
    }
}
