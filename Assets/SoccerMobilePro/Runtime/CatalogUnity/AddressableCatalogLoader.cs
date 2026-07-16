using System;
using System.Collections;
using SoccerMobilePro.Catalog;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SoccerMobilePro.Catalog.Unity
{
    public sealed class AddressableCatalogLoadResult
    {
        public AddressableCatalogLoadResult(bool succeeded, CatalogSnapshot snapshot, string failureReason)
        {
            Succeeded = succeeded;
            Snapshot = snapshot;
            FailureReason = failureReason ?? string.Empty;
        }

        public bool Succeeded { get; }
        public CatalogSnapshot Snapshot { get; }
        public string FailureReason { get; }
    }

    public sealed class AddressableCatalogLoader
    {
        private readonly ICatalogCodec codec;
        private readonly ICatalogValidator validator;

        public AddressableCatalogLoader(ICatalogCodec codec, ICatalogValidator validator)
        {
            this.codec = codec ?? throw new ArgumentNullException(nameof(codec));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public IEnumerator Load(string address, Action<AddressableCatalogLoadResult> completed)
        {
            if (completed == null) throw new ArgumentNullException(nameof(completed));
            AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(address);
            yield return handle;
            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                if (handle.IsValid()) Addressables.Release(handle);
                completed(new AddressableCatalogLoadResult(false, null, "addressable-load-failed"));
                yield break;
            }

            try
            {
                CatalogSnapshot snapshot = codec.DeserializeSnapshot(handle.Result.text);
                CatalogValidationResult validation = validator.ValidateSnapshot(snapshot);
                completed(validation.Succeeded
                    ? new AddressableCatalogLoadResult(true, snapshot, string.Empty)
                    : new AddressableCatalogLoadResult(false, null, "catalog-validation-failed"));
            }
            catch (CatalogFormatException)
            {
                completed(new AddressableCatalogLoadResult(false, null, "catalog-format-failed"));
            }
            finally
            {
                Addressables.Release(handle);
            }
        }
    }
}
