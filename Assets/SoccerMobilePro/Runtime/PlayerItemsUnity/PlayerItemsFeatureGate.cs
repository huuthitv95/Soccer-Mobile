using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SoccerMobilePro.PlayerItems.Unity
{
    public sealed class PlayerItemsFeatureGate : MonoBehaviour
    {
        [SerializeField] private bool featureEnabled;
        [SerializeField] private bool readOnly = true;

        public bool FeatureEnabled => featureEnabled;
        public bool IsReadOnly => !featureEnabled || readOnly;
        public bool CanMutate => featureEnabled && !readOnly;

        public void ConfigureForDiagnostics(bool enabled, bool forceReadOnly)
        {
            featureEnabled = enabled;
            readOnly = forceReadOnly;
        }
    }

    public sealed class InventoryProjection
    {
        public InventoryProjection(string ownerId, long revision, IReadOnlyList<string> itemIds, bool readOnly)
        {
            OwnerId = ownerId ?? string.Empty;
            Revision = revision;
            ItemIds = itemIds ?? Array.Empty<string>();
            IsReadOnly = readOnly;
        }

        public string OwnerId { get; }
        public long Revision { get; }
        public IReadOnlyList<string> ItemIds { get; }
        public bool IsReadOnly { get; }
    }

    public static class InventoryProjectionFactory
    {
        public static InventoryProjection Create(InventorySnapshot snapshot, PlayerItemsFeatureGate gate)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (gate == null) throw new ArgumentNullException(nameof(gate));
            string[] ids = (snapshot.Items ?? new List<OwnedPlayerItem>()).Select(item => item.ItemId).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            return new InventoryProjection(snapshot.OwnerId, snapshot.Revision, ids, gate.IsReadOnly);
        }
    }
}
