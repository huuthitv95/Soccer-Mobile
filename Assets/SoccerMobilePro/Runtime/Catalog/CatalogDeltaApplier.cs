using System;
using System.Collections.Generic;

namespace SoccerMobilePro.Catalog
{
    public sealed class CatalogDeltaApplier
    {
        private readonly ICatalogCodec codec;
        private readonly ICatalogValidator validator;

        public CatalogDeltaApplier(ICatalogCodec codec, ICatalogValidator validator)
        {
            this.codec = codec ?? throw new ArgumentNullException(nameof(codec));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public CatalogSnapshot Apply(CatalogSnapshot active, CatalogDelta delta)
        {
            if (active == null) throw new ArgumentNullException(nameof(active));
            CatalogValidationResult deltaResult = validator.ValidateDelta(delta, active.CatalogVersion, codec);
            if (!deltaResult.Succeeded) throw new CatalogDeltaException("Delta contract is invalid.", deltaResult.Errors);

            CatalogSnapshot output = codec.DeserializeSnapshot(codec.SerializeSnapshot(active));
            CatalogSnapshot upserts = delta.Upserts ?? new CatalogSnapshot();
            CatalogRemovalSet removals = delta.Removals ?? new CatalogRemovalSet();
            Merge(output.Leagues, upserts.Leagues, removals.LeagueIds, item => item.LeagueId);
            Merge(output.Competitions, upserts.Competitions, removals.CompetitionIds, item => item.CompetitionId);
            Merge(output.Clubs, upserts.Clubs, removals.ClubIds, item => item.ClubId);
            Merge(output.Players, upserts.Players, removals.PlayerIds, item => item.PlayerId);
            Merge(output.Ratings, upserts.Ratings, removals.RatingIds, item => item.RatingId);
            Merge(output.Items, upserts.Items, removals.ItemDefinitionIds, item => item.ItemDefinitionId);
            Merge(output.Registrations, upserts.Registrations, removals.RegistrationIds, item => item.RegistrationId);
            Merge(output.Models, upserts.Models, removals.ModelAssetIds, item => item.ModelAssetId);
            output.CatalogVersion = delta.TargetVersion;
            if (upserts.SchemaVersion > 0) output.SchemaVersion = upserts.SchemaVersion;
            if (!string.IsNullOrWhiteSpace(upserts.Region)) output.Region = upserts.Region;
            if (!string.IsNullOrWhiteSpace(upserts.Season)) output.Season = upserts.Season;

            CatalogValidationResult snapshotResult = validator.ValidateSnapshot(output);
            if (!snapshotResult.Succeeded) throw new CatalogDeltaException("Delta creates an invalid snapshot.", snapshotResult.Errors);
            return output;
        }

        private static void Merge<T>(List<T> current, IEnumerable<T> upserts, IEnumerable<string> removals, Func<T, string> idSelector)
        {
            var byId = new Dictionary<string, T>(StringComparer.Ordinal);
            foreach (T item in current ?? new List<T>()) byId[idSelector(item)] = item;
            foreach (string id in removals ?? Array.Empty<string>()) byId.Remove(id);
            foreach (T item in upserts ?? Array.Empty<T>()) byId[idSelector(item)] = item;
            current.Clear();
            var keys = new List<string>(byId.Keys);
            keys.Sort(StringComparer.Ordinal);
            foreach (string key in keys) current.Add(byId[key]);
        }
    }

    public sealed class CatalogDeltaException : Exception
    {
        public CatalogDeltaException(string message, IReadOnlyList<CatalogValidationError> errors) : base(message)
        {
            Errors = errors ?? Array.Empty<CatalogValidationError>();
        }

        public IReadOnlyList<CatalogValidationError> Errors { get; }
    }
}
