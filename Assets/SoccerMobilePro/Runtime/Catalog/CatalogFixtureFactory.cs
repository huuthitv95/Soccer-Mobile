using System.Collections.Generic;

namespace SoccerMobilePro.Catalog
{
    public static class CatalogFixtureFactory
    {
        public const string InitialVersion = "202607160001";
        public const string GenericModelId = "model-generic-player";
        public const string MissingModelId = "model-missing-player";
        public const string GenericModelAddress = "football/model/generic-player";

        public static CatalogSnapshot Create()
        {
            var snapshot = new CatalogSnapshot
            {
                SchemaVersion = 1,
                CatalogVersion = InitialVersion,
                Region = "fixture",
                Season = "fixture-2026"
            };

            snapshot.Models.Add(new ModelAssetManifest
            {
                ModelAssetId = GenericModelId,
                Address = GenericModelAddress,
                RigVersion = "legacy-humanoid-v1",
                SizeBytes = 1,
                Checksum = "fixture-generic"
            });
            snapshot.Models.Add(new ModelAssetManifest
            {
                ModelAssetId = MissingModelId,
                Address = "football/model/intentionally-missing",
                FallbackModelAssetId = GenericModelId,
                RigVersion = "legacy-humanoid-v1",
                SizeBytes = 1,
                Checksum = "fixture-missing"
            });

            for (int leagueIndex = 1; leagueIndex <= 2; leagueIndex++)
            {
                string leagueId = "fixture-league-" + leagueIndex;
                snapshot.Leagues.Add(new LeagueDefinition
                {
                    LeagueId = leagueId,
                    NameKey = "fixture.league." + leagueIndex,
                    CountryCode = leagueIndex == 1 ? "VN" : "GB",
                    RightsVersion = "licensed-full-2026",
                    ProvenanceId = "fixture-provenance"
                });
                snapshot.Competitions.Add(new CompetitionDefinition
                {
                    CompetitionId = "fixture-competition-" + leagueIndex,
                    LeagueId = leagueId,
                    NameKey = "fixture.competition." + leagueIndex,
                    SeasonId = snapshot.Season
                });

                for (int clubOffset = 1; clubOffset <= 2; clubOffset++)
                {
                    int clubNumber = ((leagueIndex - 1) * 2) + clubOffset;
                    string clubId = "fixture-club-" + clubNumber;
                    snapshot.Clubs.Add(new ClubDefinition
                    {
                        ClubId = clubId,
                        LeagueId = leagueId,
                        NameKey = "fixture.club." + clubNumber,
                        ShortName = "F" + clubNumber,
                        CrestAddress = "football/crest/fixture-" + clubNumber,
                        RightsVersion = "licensed-full-2026",
                        ProvenanceId = "fixture-provenance"
                    });

                    for (int squadIndex = 1; squadIndex <= 11; squadIndex++)
                    {
                        int playerNumber = ((clubNumber - 1) * 11) + squadIndex;
                        string playerId = "fixture-player-" + playerNumber.ToString("00");
                        string modelId = playerNumber == 44 ? MissingModelId : GenericModelId;
                        snapshot.Players.Add(new PlayerIdentity
                        {
                            PlayerId = playerId,
                            DisplayName = "Fixture Player " + playerNumber.ToString("00"),
                            BirthYear = 1988 + (playerNumber % 16),
                            NationalityIds = new List<string> { leagueIndex == 1 ? "VN" : "GB" },
                            PreferredFoot = playerNumber % 4 == 0 ? "left" : "right",
                            RightsVersion = "licensed-full-2026",
                            ProvenanceId = "fixture-provenance"
                        });
                        snapshot.Ratings.Add(new PlayerSeasonRating
                        {
                            RatingId = "fixture-rating-" + playerNumber.ToString("00"),
                            PlayerId = playerId,
                            SeasonId = snapshot.Season,
                            PrimaryPosition = squadIndex == 1 ? "GK" : squadIndex <= 5 ? "DEF" : squadIndex <= 8 ? "MID" : "FWD",
                            Overall = 60 + (playerNumber % 20),
                            Attributes = new List<PlayerAttributeValue>
                            {
                                new PlayerAttributeValue { AttributeId = "pace", Value = 55 + (playerNumber % 30) },
                                new PlayerAttributeValue { AttributeId = "passing", Value = 50 + (playerNumber % 30) }
                            }
                        });
                        snapshot.Items.Add(new PlayerItemDefinition
                        {
                            ItemDefinitionId = "fixture-item-" + playerNumber.ToString("00"),
                            PlayerId = playerId,
                            ProgramId = "fixture-base",
                            BaseOverall = 60 + (playerNumber % 20),
                            RulesVersion = "fixture-rules-1",
                            ModelAssetId = modelId
                        });
                        snapshot.Registrations.Add(new PlayerClubRegistration
                        {
                            RegistrationId = "fixture-registration-" + playerNumber.ToString("00"),
                            PlayerId = playerId,
                            ClubId = clubId,
                            SeasonId = snapshot.Season,
                            ShirtNumber = squadIndex
                        });
                    }
                }
            }

            return snapshot;
        }
    }
}
