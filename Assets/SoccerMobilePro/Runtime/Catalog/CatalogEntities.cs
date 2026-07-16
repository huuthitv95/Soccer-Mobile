using System;
using System.Collections.Generic;

namespace SoccerMobilePro.Catalog
{
    public static class CatalogVersion
    {
        public const int Length = 12;

        public static bool IsCanonical(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length != Length) return false;
            for (int index = 0; index < value.Length; index++)
            {
                if (value[index] < '0' || value[index] > '9') return false;
            }

            return true;
        }

        public static int Compare(string left, string right)
        {
            if (!IsCanonical(left)) throw new ArgumentException("Catalog version must contain 12 digits.", nameof(left));
            if (!IsCanonical(right)) throw new ArgumentException("Catalog version must contain 12 digits.", nameof(right));
            return string.CompareOrdinal(left, right);
        }
    }

    public sealed class LeagueDefinition
    {
        public string LeagueId { get; set; } = string.Empty;
        public string NameKey { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string RightsVersion { get; set; } = string.Empty;
        public string ProvenanceId { get; set; } = string.Empty;
    }

    public sealed class CompetitionDefinition
    {
        public string CompetitionId { get; set; } = string.Empty;
        public string LeagueId { get; set; } = string.Empty;
        public string NameKey { get; set; } = string.Empty;
        public string SeasonId { get; set; } = string.Empty;
    }

    public sealed class ClubDefinition
    {
        public string ClubId { get; set; } = string.Empty;
        public string LeagueId { get; set; } = string.Empty;
        public string NameKey { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string CrestAddress { get; set; } = string.Empty;
        public string RightsVersion { get; set; } = string.Empty;
        public string ProvenanceId { get; set; } = string.Empty;
    }

    public sealed class PlayerIdentity
    {
        public string PlayerId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int BirthYear { get; set; }
        public List<string> NationalityIds { get; set; } = new List<string>();
        public string PreferredFoot { get; set; } = string.Empty;
        public string RightsVersion { get; set; } = string.Empty;
        public string ProvenanceId { get; set; } = string.Empty;
    }

    public sealed class PlayerAttributeValue
    {
        public string AttributeId { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public sealed class PlayerSeasonRating
    {
        public string RatingId { get; set; } = string.Empty;
        public string PlayerId { get; set; } = string.Empty;
        public string SeasonId { get; set; } = string.Empty;
        public string PrimaryPosition { get; set; } = string.Empty;
        public int Overall { get; set; }
        public List<PlayerAttributeValue> Attributes { get; set; } = new List<PlayerAttributeValue>();
    }

    public sealed class PlayerItemDefinition
    {
        public string ItemDefinitionId { get; set; } = string.Empty;
        public string PlayerId { get; set; } = string.Empty;
        public string ProgramId { get; set; } = string.Empty;
        public int BaseOverall { get; set; }
        public string RulesVersion { get; set; } = string.Empty;
        public string ModelAssetId { get; set; } = string.Empty;
    }

    public sealed class PlayerClubRegistration
    {
        public string RegistrationId { get; set; } = string.Empty;
        public string PlayerId { get; set; } = string.Empty;
        public string ClubId { get; set; } = string.Empty;
        public string SeasonId { get; set; } = string.Empty;
        public int ShirtNumber { get; set; }
    }

    public sealed class ModelAssetManifest
    {
        public string ModelAssetId { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string FallbackModelAssetId { get; set; } = string.Empty;
        public string RigVersion { get; set; } = string.Empty;
        public List<string> LodAddresses { get; set; } = new List<string>();
        public List<string> Dependencies { get; set; } = new List<string>();
        public long SizeBytes { get; set; }
        public string Checksum { get; set; } = string.Empty;
    }

    public sealed class CatalogSnapshot
    {
        public int SchemaVersion { get; set; } = 1;
        public string CatalogVersion { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public List<LeagueDefinition> Leagues { get; set; } = new List<LeagueDefinition>();
        public List<CompetitionDefinition> Competitions { get; set; } = new List<CompetitionDefinition>();
        public List<ClubDefinition> Clubs { get; set; } = new List<ClubDefinition>();
        public List<PlayerIdentity> Players { get; set; } = new List<PlayerIdentity>();
        public List<PlayerSeasonRating> Ratings { get; set; } = new List<PlayerSeasonRating>();
        public List<PlayerItemDefinition> Items { get; set; } = new List<PlayerItemDefinition>();
        public List<PlayerClubRegistration> Registrations { get; set; } = new List<PlayerClubRegistration>();
        public List<ModelAssetManifest> Models { get; set; } = new List<ModelAssetManifest>();
    }

    public sealed class CatalogRemovalSet
    {
        public List<string> LeagueIds { get; set; } = new List<string>();
        public List<string> CompetitionIds { get; set; } = new List<string>();
        public List<string> ClubIds { get; set; } = new List<string>();
        public List<string> PlayerIds { get; set; } = new List<string>();
        public List<string> RatingIds { get; set; } = new List<string>();
        public List<string> ItemDefinitionIds { get; set; } = new List<string>();
        public List<string> RegistrationIds { get; set; } = new List<string>();
        public List<string> ModelAssetIds { get; set; } = new List<string>();
    }

    public sealed class CatalogDelta
    {
        public string BaseVersion { get; set; } = string.Empty;
        public string TargetVersion { get; set; } = string.Empty;
        public string PayloadHash { get; set; } = string.Empty;
        public CatalogSnapshot Upserts { get; set; } = new CatalogSnapshot();
        public CatalogRemovalSet Removals { get; set; } = new CatalogRemovalSet();
    }
}
