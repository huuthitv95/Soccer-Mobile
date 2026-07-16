using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SoccerMobilePro.Catalog.Unity;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.TestTools;

namespace SoccerMobilePro.Catalog.PlayModeTests
{
    public sealed class CatalogAddressablesPlayModeTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return Addressables.InitializeAsync();
        }

        [UnityTest]
        public IEnumerator FixtureCatalog_LoadsFromAddressablesAndValidates()
        {
            AddressableCatalogLoadResult result = null;
            var loader = new AddressableCatalogLoader(new NewtonsoftCatalogCodec(), new DefaultCatalogValidator());

            yield return loader.Load("football/catalog/fixture", value => result = value);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Succeeded, Is.True, result.FailureReason);
            Assert.That(result.Snapshot.Leagues, Has.Count.EqualTo(2));
            Assert.That(result.Snapshot.Clubs, Has.Count.EqualTo(4));
            Assert.That(result.Snapshot.Players, Has.Count.EqualTo(44));
        }

        [UnityTest]
        public IEnumerator GenericModel_LoadsAfterCleanCacheRequest()
        {
            yield return Addressables.ClearDependencyCacheAsync(CatalogFixtureFactory.GenericModelAddress, true);
            CatalogSnapshot fixture = CatalogFixtureFactory.Create();
            IReadOnlyDictionary<string, ModelAssetManifest> manifests = fixture.Models.ToDictionary(model => model.ModelAssetId, StringComparer.Ordinal);
            var resolver = new AddressableModelAssetResolver();
            ModelAssetResolution result = null;

            yield return resolver.Resolve(manifests[CatalogFixtureFactory.GenericModelId], manifests, value => result = value);

            Assert.That(result.State, Is.EqualTo(ModelAssetLoadState.Ready));
            Assert.That(result.Instance, Is.Not.Null);
            Assert.That(result.UsedFallback, Is.False);
            resolver.Release(result);
        }

        [UnityTest]
        public IEnumerator RejectedManifest_UsesGenericFallbackWithoutBlocking()
        {
            CatalogSnapshot fixture = CatalogFixtureFactory.Create();
            IReadOnlyDictionary<string, ModelAssetManifest> manifests = fixture.Models.ToDictionary(model => model.ModelAssetId, StringComparer.Ordinal);
            var resolver = new AddressableModelAssetResolver(new RejectMissingFixtureManifestVerifier());
            ModelAssetResolution result = null;

            yield return resolver.Resolve(manifests[CatalogFixtureFactory.MissingModelId], manifests, value => result = value);

            Assert.That(result.State, Is.EqualTo(ModelAssetLoadState.Ready));
            Assert.That(result.ResolvedModelAssetId, Is.EqualTo(CatalogFixtureFactory.GenericModelId));
            Assert.That(result.UsedFallback, Is.True);
            Assert.That(result.Instance, Is.Not.Null);
            resolver.Release(result);
        }

        [UnityTest]
        public IEnumerator MissingAddress_UsesGenericFallbackAndCanBeEvicted()
        {
            CatalogSnapshot fixture = CatalogFixtureFactory.Create();
            IReadOnlyDictionary<string, ModelAssetManifest> manifests = fixture.Models.ToDictionary(model => model.ModelAssetId, StringComparer.Ordinal);
            var resolver = new AddressableModelAssetResolver();
            ModelAssetResolution result = null;
            Action<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle, Exception> previousHandler = ResourceManager.ExceptionHandler;
            ResourceManager.ExceptionHandler = (_, __) => { };
            try
            {
                yield return resolver.Resolve(manifests[CatalogFixtureFactory.MissingModelId], manifests, value => result = value);
            }
            finally
            {
                ResourceManager.ExceptionHandler = previousHandler;
            }

            Assert.That(result.State, Is.EqualTo(ModelAssetLoadState.Ready));
            Assert.That(result.ResolvedModelAssetId, Is.EqualTo(CatalogFixtureFactory.GenericModelId));
            Assert.That(result.UsedFallback, Is.True);
            resolver.Release(result);
            resolver.Evict(CatalogFixtureFactory.MissingModelId);
            Assert.That(resolver.GetState(CatalogFixtureFactory.MissingModelId), Is.EqualTo(ModelAssetLoadState.Evicted));
        }

        private sealed class RejectMissingFixtureManifestVerifier : IModelManifestVerifier
        {
            public bool Verify(ModelAssetManifest manifest) => manifest != null && manifest.ModelAssetId != CatalogFixtureFactory.MissingModelId;
        }
    }
}
