using System;
using System.Collections;
using System.Collections.Generic;
using SoccerMobilePro.Catalog;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SoccerMobilePro.Catalog.Unity
{
    public enum ModelAssetLoadState
    {
        NotRequested,
        Downloading,
        Ready,
        Failed,
        Evicted
    }

    public sealed class ModelAssetResolution
    {
        public ModelAssetResolution(string requestedModelAssetId, string resolvedModelAssetId, ModelAssetLoadState state, GameObject instance, bool usedFallback, string failureReason)
        {
            RequestedModelAssetId = requestedModelAssetId ?? string.Empty;
            ResolvedModelAssetId = resolvedModelAssetId ?? string.Empty;
            State = state;
            Instance = instance;
            UsedFallback = usedFallback;
            FailureReason = failureReason ?? string.Empty;
        }

        public string RequestedModelAssetId { get; }
        public string ResolvedModelAssetId { get; }
        public ModelAssetLoadState State { get; }
        public GameObject Instance { get; }
        public bool UsedFallback { get; }
        public string FailureReason { get; }
    }

    public interface IModelManifestVerifier
    {
        bool Verify(ModelAssetManifest manifest);
    }

    public sealed class RequiredChecksumModelManifestVerifier : IModelManifestVerifier
    {
        public bool Verify(ModelAssetManifest manifest) => manifest != null && !string.IsNullOrWhiteSpace(manifest.Checksum);
    }

    public interface IModelAssetResolver
    {
        ModelAssetLoadState GetState(string modelAssetId);
        IEnumerator Resolve(ModelAssetManifest requested, IReadOnlyDictionary<string, ModelAssetManifest> manifests, Action<ModelAssetResolution> completed);
        void Release(ModelAssetResolution resolution);
        void Evict(string modelAssetId);
    }

    public sealed class AddressableModelAssetResolver : IModelAssetResolver
    {
        private readonly IModelManifestVerifier verifier;
        private readonly Dictionary<string, ModelAssetLoadState> states = new Dictionary<string, ModelAssetLoadState>(StringComparer.Ordinal);

        public AddressableModelAssetResolver(IModelManifestVerifier verifier = null)
        {
            this.verifier = verifier ?? new RequiredChecksumModelManifestVerifier();
        }

        public ModelAssetLoadState GetState(string modelAssetId)
        {
            return !string.IsNullOrEmpty(modelAssetId) && states.TryGetValue(modelAssetId, out ModelAssetLoadState state)
                ? state
                : ModelAssetLoadState.NotRequested;
        }

        public IEnumerator Resolve(ModelAssetManifest requested, IReadOnlyDictionary<string, ModelAssetManifest> manifests, Action<ModelAssetResolution> completed)
        {
            if (requested == null) throw new ArgumentNullException(nameof(requested));
            if (manifests == null) throw new ArgumentNullException(nameof(manifests));
            if (completed == null) throw new ArgumentNullException(nameof(completed));
            var visited = new HashSet<string>(StringComparer.Ordinal);
            ModelAssetResolution result = null;
            yield return ResolveInternal(requested, requested.ModelAssetId, manifests, visited, value => result = value);
            completed(result ?? new ModelAssetResolution(requested.ModelAssetId, string.Empty, ModelAssetLoadState.Failed, null, false, "resolver-no-result"));
        }

        public void Release(ModelAssetResolution resolution)
        {
            if (resolution?.Instance != null) Addressables.ReleaseInstance(resolution.Instance);
        }

        public void Evict(string modelAssetId)
        {
            if (!string.IsNullOrEmpty(modelAssetId)) states[modelAssetId] = ModelAssetLoadState.Evicted;
        }

        private IEnumerator ResolveInternal(ModelAssetManifest current, string requestedId, IReadOnlyDictionary<string, ModelAssetManifest> manifests, HashSet<string> visited, Action<ModelAssetResolution> completed)
        {
            if (!visited.Add(current.ModelAssetId))
            {
                states[current.ModelAssetId] = ModelAssetLoadState.Failed;
                completed(new ModelAssetResolution(requestedId, current.ModelAssetId, ModelAssetLoadState.Failed, null, current.ModelAssetId != requestedId, "fallback-cycle"));
                yield break;
            }

            states[current.ModelAssetId] = ModelAssetLoadState.Downloading;
            if (verifier.Verify(current))
            {
                AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(current.Address);
                yield return handle;
                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    states[current.ModelAssetId] = ModelAssetLoadState.Ready;
                    completed(new ModelAssetResolution(requestedId, current.ModelAssetId, ModelAssetLoadState.Ready, handle.Result, current.ModelAssetId != requestedId, string.Empty));
                    yield break;
                }
                if (handle.IsValid()) Addressables.Release(handle);
            }

            states[current.ModelAssetId] = ModelAssetLoadState.Failed;
            if (!string.IsNullOrEmpty(current.FallbackModelAssetId) && manifests.TryGetValue(current.FallbackModelAssetId, out ModelAssetManifest fallback))
            {
                yield return ResolveInternal(fallback, requestedId, manifests, visited, completed);
                yield break;
            }

            completed(new ModelAssetResolution(requestedId, current.ModelAssetId, ModelAssetLoadState.Failed, null, current.ModelAssetId != requestedId, verifier.Verify(current) ? "addressable-load-failed" : "manifest-verification-failed"));
        }
    }
}
