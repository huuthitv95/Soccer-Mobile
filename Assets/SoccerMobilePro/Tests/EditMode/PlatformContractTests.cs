using System;
using System.Collections.Generic;
using NUnit.Framework;
using SoccerMobilePro.Platform;

namespace SoccerMobilePro.MatchCore.Tests
{
    public sealed class PlatformContractTests
    {
        private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 7, 15, 0, 0, 0, TimeSpan.Zero);

        [Test]
        public void GuestSession_CannotUseOnlineEconomy()
        {
            var repository = new FakeSessionRepository();
            SessionOperationResult result = repository.CreateGuest("device-account", Now, TimeSpan.FromHours(1));

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Session.AuthLevel, Is.EqualTo(AccountAuthLevel.Guest));
            Assert.That(result.Session.CanUseOnlineEconomy, Is.False);
        }

        [Test]
        public void Session_ExpiresAndRevocationIsSticky()
        {
            var repository = new FakeSessionRepository();
            AccountSession expiring = repository.CreateGuest("expiry", Now, TimeSpan.FromMinutes(1)).Session;
            Assert.That(repository.GetCurrent(Now.AddMinutes(1)).FailureReason, Is.EqualTo(SessionFailureReason.Expired));

            AccountSession active = repository.CreateGuest("revoke", Now, TimeSpan.FromHours(1)).Session;
            Assert.That(repository.Revoke(active.SessionId, Now).FailureReason, Is.EqualTo(SessionFailureReason.Revoked));
            Assert.That(repository.GetCurrent(Now).FailureReason, Is.EqualTo(SessionFailureReason.Revoked));
        }

        [Test]
        public void GuestLink_RejectsExistingTargetWithoutMerge()
        {
            var repository = new FakeSessionRepository();
            repository.SeedLinkedAccount("linked-account");
            AccountSession guest = repository.CreateGuest("guest-account", Now, TimeSpan.FromHours(1)).Session;

            SessionOperationResult result = repository.LinkGuest("linked-account", "test-provider", Now, TimeSpan.FromHours(1));

            Assert.That(result.FailureReason, Is.EqualTo(SessionFailureReason.AccountLinkConflict));
            Assert.That(repository.GetCurrent(Now).Session.AccountId, Is.EqualTo(guest.AccountId));
        }

        [Test]
        public void Refresh_RateLimitsRetryStorm()
        {
            var repository = new FakeSessionRepository();
            AccountSession session = repository.CreateGuest("retry", Now, TimeSpan.FromHours(1)).Session;

            for (int index = 0; index < 3; index++)
            {
                SessionOperationResult refresh = repository.Refresh(session.SessionId, Now.AddSeconds(index), TimeSpan.FromHours(1));
                Assert.That(refresh.Succeeded, Is.True);
                session = refresh.Session;
            }

            SessionOperationResult limited = repository.Refresh(session.SessionId, Now.AddSeconds(3), TimeSpan.FromHours(1));
            Assert.That(limited.FailureReason, Is.EqualTo(SessionFailureReason.RateLimited));
            Assert.That(limited.RetryAfterSeconds, Is.GreaterThan(0));
        }

        [Test]
        public void InMemoryCredentialVault_DeletesCredential()
        {
            var vault = new InMemoryCredentialVault();
            vault.Store("account", "opaque-test-credential");
            Assert.That(vault.TryRead("account", out string value), Is.True);
            Assert.That(value, Is.EqualTo("opaque-test-credential"));

            vault.Delete("account");
            Assert.That(vault.TryRead("account", out _), Is.False);
        }

        [Test]
        public void OfflineCatalog_FallsBackWhenNewestCacheIsCorrupt()
        {
            var repository = new InMemoryCatalogRepository(new FakeCatalogSignatureVerifier());
            repository.SeedCache(CreateManifest(2, "002", "valid-v2", Now.AddDays(1)), "corrupt-payload");
            repository.SeedCache(CreateManifest(1, "001", "valid-v1", Now.AddDays(1)), "valid-v1");

            CatalogReadResult result = repository.GetActive(2, Now, false);

            Assert.That(result.Status, Is.EqualTo(CatalogReadStatus.OfflineFallback));
            Assert.That(result.Manifest.CatalogVersion, Is.EqualTo("001"));
            Assert.That(result.Payload, Is.EqualTo("valid-v1"));
        }

        [TestCase(2, true)]
        [TestCase(1, true)]
        [TestCase(0, false)]
        public void Catalog_AcceptsOnlySchemaNAndNMinusOne(int schemaVersion, bool expectedSuccess)
        {
            const string payload = "catalog";
            var repository = new InMemoryCatalogRepository(new FakeCatalogSignatureVerifier());
            repository.SeedCache(CreateManifest(schemaVersion, "001", payload, Now.AddDays(1)), payload);

            Assert.That(repository.GetActive(2, Now, true).Succeeded, Is.EqualTo(expectedSuccess));
        }

        [Test]
        public void Catalog_RejectsExpiredOrInvalidSignature()
        {
            const string payload = "catalog";
            var expiredRepository = new InMemoryCatalogRepository(new FakeCatalogSignatureVerifier());
            expiredRepository.SeedCache(CreateManifest(2, "001", payload, Now.AddSeconds(-1)), payload);
            Assert.That(expiredRepository.GetActive(2, Now, true).Status, Is.EqualTo(CatalogReadStatus.Expired));

            var unsignedRepository = new InMemoryCatalogRepository(new FakeCatalogSignatureVerifier());
            CatalogManifest unsigned = CreateManifest(2, "001", payload, Now.AddDays(1), "not-signed");
            unsignedRepository.SeedCache(unsigned, payload);
            Assert.That(unsignedRepository.GetActive(2, Now, true).Status, Is.EqualTo(CatalogReadStatus.InvalidSignature));
        }

        [Test]
        public void SettingsSnapshot_CopiesSourceMaps()
        {
            var source = new Dictionary<string, string> { ["audio.music"] = "on" };
            var snapshot = new SettingsSnapshot("account", 2, 3, source, null, Now);
            source["audio.music"] = "off";

            Assert.That(snapshot.Values["audio.music"], Is.EqualTo("on"));
        }

        private static CatalogManifest CreateManifest(
            int schemaVersion,
            string catalogVersion,
            string payload,
            DateTimeOffset expiresAtUtc,
            string signature = null)
        {
            string hash = CatalogIntegrity.ComputeSha256(payload);
            return new CatalogManifest(
                schemaVersion,
                catalogVersion,
                "VN",
                "2026",
                Now.AddDays(-1),
                expiresAtUtc,
                "0.1.0",
                hash,
                signature ?? "test-signed:" + hash,
                string.Empty);
        }
    }
}
