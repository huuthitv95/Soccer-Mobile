using System;
using Newtonsoft.Json;
using SoccerMobilePro.Platform;

namespace SoccerMobilePro.Catalog
{
    public interface ICatalogCodec
    {
        string SerializeSnapshot(CatalogSnapshot snapshot);
        CatalogSnapshot DeserializeSnapshot(string payload);
        string SerializeDelta(CatalogDelta delta);
        CatalogDelta DeserializeDelta(string payload);
    }

    public sealed class NewtonsoftCatalogCodec : ICatalogCodec
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Include,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            Formatting = Formatting.None
        };

        public string SerializeSnapshot(CatalogSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            return JsonConvert.SerializeObject(snapshot, Settings);
        }

        public CatalogSnapshot DeserializeSnapshot(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload)) throw new CatalogFormatException("Catalog snapshot payload is empty.");
            try
            {
                return JsonConvert.DeserializeObject<CatalogSnapshot>(payload, Settings)
                    ?? throw new CatalogFormatException("Catalog snapshot decoded to null.");
            }
            catch (JsonException exception)
            {
                throw new CatalogFormatException("Catalog snapshot JSON is invalid.", exception);
            }
        }

        public string SerializeDelta(CatalogDelta delta)
        {
            if (delta == null) throw new ArgumentNullException(nameof(delta));
            return JsonConvert.SerializeObject(delta, Settings);
        }

        public CatalogDelta DeserializeDelta(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload)) throw new CatalogFormatException("Catalog delta payload is empty.");
            try
            {
                return JsonConvert.DeserializeObject<CatalogDelta>(payload, Settings)
                    ?? throw new CatalogFormatException("Catalog delta decoded to null.");
            }
            catch (JsonException exception)
            {
                throw new CatalogFormatException("Catalog delta JSON is invalid.", exception);
            }
        }
    }

    public sealed class CatalogFormatException : Exception
    {
        public CatalogFormatException(string message) : base(message) { }
        public CatalogFormatException(string message, Exception innerException) : base(message, innerException) { }
    }

    public static class CatalogDeltaIntegrity
    {
        public static string ComputePayloadHash(CatalogDelta delta, ICatalogCodec codec)
        {
            if (delta == null) throw new ArgumentNullException(nameof(delta));
            if (codec == null) throw new ArgumentNullException(nameof(codec));
            string original = delta.PayloadHash;
            try
            {
                delta.PayloadHash = string.Empty;
                return CatalogIntegrity.ComputeSha256(codec.SerializeDelta(delta));
            }
            finally
            {
                delta.PayloadHash = original;
            }
        }
    }
}
