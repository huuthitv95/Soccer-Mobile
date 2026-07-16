using System;
using System.Collections.Generic;
using System.Linq;

namespace SoccerMobilePro.Catalog
{
    public enum CatalogValidationCode
    {
        InvalidSchema,
        InvalidVersion,
        MissingId,
        DuplicateId,
        MissingReference,
        InvalidValue,
        InvalidDeltaBase,
        InvalidDeltaTarget,
        InvalidDeltaHash,
        FallbackCycle
    }

    public sealed class CatalogValidationError
    {
        public CatalogValidationError(CatalogValidationCode code, string field, string entityId, string message)
        {
            Code = code;
            Field = field ?? string.Empty;
            EntityId = entityId ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public CatalogValidationCode Code { get; }
        public string Field { get; }
        public string EntityId { get; }
        public string Message { get; }
    }

    public sealed class CatalogValidationResult
    {
        public CatalogValidationResult(IReadOnlyList<CatalogValidationError> errors)
        {
            Errors = errors ?? Array.Empty<CatalogValidationError>();
        }

        public IReadOnlyList<CatalogValidationError> Errors { get; }
        public bool Succeeded => Errors.Count == 0;
    }

    public interface ICatalogValidator
    {
        CatalogValidationResult ValidateSnapshot(CatalogSnapshot snapshot);
        CatalogValidationResult ValidateDelta(CatalogDelta delta, string activeVersion, ICatalogCodec codec);
    }

    public sealed class DefaultCatalogValidator : ICatalogValidator
    {
        public CatalogValidationResult ValidateSnapshot(CatalogSnapshot snapshot)
        {
            var errors = new List<CatalogValidationError>();
            if (snapshot == null)
            {
                errors.Add(Error(CatalogValidationCode.InvalidSchema, "snapshot", string.Empty, "Snapshot is required."));
                return new CatalogValidationResult(errors);
            }

            if (snapshot.SchemaVersion < 1) errors.Add(Error(CatalogValidationCode.InvalidSchema, "schemaVersion", string.Empty, "Schema version must be positive."));
            if (!CatalogVersion.IsCanonical(snapshot.CatalogVersion)) errors.Add(Error(CatalogValidationCode.InvalidVersion, "catalogVersion", string.Empty, "Catalog version must contain 12 digits."));

            HashSet<string> leagues = CollectIds(snapshot.Leagues, item => item.LeagueId, "leagueId", errors);
            HashSet<string> competitions = CollectIds(snapshot.Competitions, item => item.CompetitionId, "competitionId", errors);
            HashSet<string> clubs = CollectIds(snapshot.Clubs, item => item.ClubId, "clubId", errors);
            HashSet<string> players = CollectIds(snapshot.Players, item => item.PlayerId, "playerId", errors);
            HashSet<string> ratings = CollectIds(snapshot.Ratings, item => item.RatingId, "ratingId", errors);
            HashSet<string> items = CollectIds(snapshot.Items, item => item.ItemDefinitionId, "itemDefinitionId", errors);
            HashSet<string> registrations = CollectIds(snapshot.Registrations, item => item.RegistrationId, "registrationId", errors);
            HashSet<string> models = CollectIds(snapshot.Models, item => item.ModelAssetId, "modelAssetId", errors);

            foreach (CompetitionDefinition competition in snapshot.Competitions ?? Enumerable.Empty<CompetitionDefinition>()) RequireReference(leagues, competition.LeagueId, "competition.leagueId", competition.CompetitionId, errors);
            foreach (ClubDefinition club in snapshot.Clubs ?? Enumerable.Empty<ClubDefinition>()) RequireReference(leagues, club.LeagueId, "club.leagueId", club.ClubId, errors);
            foreach (PlayerSeasonRating rating in snapshot.Ratings ?? Enumerable.Empty<PlayerSeasonRating>())
            {
                RequireReference(players, rating.PlayerId, "rating.playerId", rating.RatingId, errors);
                if (rating.Overall < 1 || rating.Overall > 100) errors.Add(Error(CatalogValidationCode.InvalidValue, "rating.overall", rating.RatingId, "Overall must be between 1 and 100."));
            }
            foreach (PlayerItemDefinition item in snapshot.Items ?? Enumerable.Empty<PlayerItemDefinition>())
            {
                RequireReference(players, item.PlayerId, "item.playerId", item.ItemDefinitionId, errors);
                RequireReference(models, item.ModelAssetId, "item.modelAssetId", item.ItemDefinitionId, errors);
            }
            foreach (PlayerClubRegistration registration in snapshot.Registrations ?? Enumerable.Empty<PlayerClubRegistration>())
            {
                RequireReference(players, registration.PlayerId, "registration.playerId", registration.RegistrationId, errors);
                RequireReference(clubs, registration.ClubId, "registration.clubId", registration.RegistrationId, errors);
                if (registration.ShirtNumber < 1 || registration.ShirtNumber > 99) errors.Add(Error(CatalogValidationCode.InvalidValue, "registration.shirtNumber", registration.RegistrationId, "Shirt number must be between 1 and 99."));
            }
            foreach (ModelAssetManifest model in snapshot.Models ?? Enumerable.Empty<ModelAssetManifest>())
            {
                if (string.IsNullOrWhiteSpace(model.Address)) errors.Add(Error(CatalogValidationCode.InvalidValue, "model.address", model.ModelAssetId, "Model address is required."));
                if (!string.IsNullOrEmpty(model.FallbackModelAssetId)) RequireReference(models, model.FallbackModelAssetId, "model.fallbackModelAssetId", model.ModelAssetId, errors);
            }

            ValidateFallbackCycles(snapshot.Models, errors);
            return new CatalogValidationResult(errors);
        }

        public CatalogValidationResult ValidateDelta(CatalogDelta delta, string activeVersion, ICatalogCodec codec)
        {
            var errors = new List<CatalogValidationError>();
            if (delta == null)
            {
                errors.Add(Error(CatalogValidationCode.InvalidDeltaBase, "delta", string.Empty, "Delta is required."));
                return new CatalogValidationResult(errors);
            }

            if (!CatalogVersion.IsCanonical(delta.BaseVersion) || delta.BaseVersion != activeVersion)
                errors.Add(Error(CatalogValidationCode.InvalidDeltaBase, "baseVersion", delta.BaseVersion, "Delta base must match the active canonical version."));
            if (!CatalogVersion.IsCanonical(delta.TargetVersion) || (CatalogVersion.IsCanonical(delta.BaseVersion) && CatalogVersion.IsCanonical(delta.TargetVersion) && CatalogVersion.Compare(delta.TargetVersion, delta.BaseVersion) <= 0))
                errors.Add(Error(CatalogValidationCode.InvalidDeltaTarget, "targetVersion", delta.TargetVersion, "Delta target must be newer than base."));
            if (string.IsNullOrWhiteSpace(delta.PayloadHash) || !string.Equals(delta.PayloadHash, CatalogDeltaIntegrity.ComputePayloadHash(delta, codec), StringComparison.Ordinal))
                errors.Add(Error(CatalogValidationCode.InvalidDeltaHash, "payloadHash", delta.TargetVersion, "Delta payload hash is invalid."));
            return new CatalogValidationResult(errors);
        }

        private static HashSet<string> CollectIds<T>(IEnumerable<T> source, Func<T, string> selector, string field, List<CatalogValidationError> errors)
        {
            var result = new HashSet<string>(StringComparer.Ordinal);
            foreach (T item in source ?? Enumerable.Empty<T>())
            {
                string id = selector(item);
                if (string.IsNullOrWhiteSpace(id)) errors.Add(Error(CatalogValidationCode.MissingId, field, string.Empty, "Entity ID is required."));
                else if (!result.Add(id)) errors.Add(Error(CatalogValidationCode.DuplicateId, field, id, "Entity ID must be unique."));
            }
            return result;
        }

        private static void RequireReference(HashSet<string> ids, string reference, string field, string entityId, List<CatalogValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(reference) || !ids.Contains(reference)) errors.Add(Error(CatalogValidationCode.MissingReference, field, entityId, "Referenced entity does not exist."));
        }

        private static void ValidateFallbackCycles(IEnumerable<ModelAssetManifest> source, List<CatalogValidationError> errors)
        {
            Dictionary<string, string> fallback = (source ?? Enumerable.Empty<ModelAssetManifest>()).ToDictionary(model => model.ModelAssetId, model => model.FallbackModelAssetId ?? string.Empty, StringComparer.Ordinal);
            foreach (string origin in fallback.Keys)
            {
                var visited = new HashSet<string>(StringComparer.Ordinal);
                string current = origin;
                while (fallback.TryGetValue(current, out string next) && !string.IsNullOrEmpty(next))
                {
                    if (!visited.Add(current))
                    {
                        errors.Add(Error(CatalogValidationCode.FallbackCycle, "model.fallbackModelAssetId", origin, "Model fallback graph contains a cycle."));
                        break;
                    }
                    current = next;
                }
            }
        }

        private static CatalogValidationError Error(CatalogValidationCode code, string field, string entityId, string message) => new CatalogValidationError(code, field, entityId, message);
    }
}
