using ProtoBuf;
using Sandbox.ModAPI;
using System.Collections.Generic;

namespace AriUtils.Components.Networking.SimpleSync
{
    internal class SimpleSyncManager : SingletonBase<SimpleSyncManager>
    {
        private readonly Dictionary<long, ISimpleSync> _syncIdMap = new Dictionary<long, ISimpleSync>();

        public override void Init()
        {
            Log.Info("SimpleSyncManager", "Ready.");
        }

        public override void Update()
        {
            // do nothing
        }

        public void RegisterSync(ISimpleSync sync, long entityId)
        {
            long id = entityId;
            while (_syncIdMap.ContainsKey(id))
                id++;
            sync.SyncId = id;

            _syncIdMap.Add(sync.SyncId, sync);
            //Log.Info("SimpleSyncManager", $"Registered SimpleSync {sync.SyncId} on {sync.Component.GetType().Name}");
        }

        public void UnregisterSync(ISimpleSync sync)
        {
            _syncIdMap.Remove(sync.SyncId);
            //Log.Info("SimpleSyncManager", $"Unregistered SimpleSync {sync.SyncId}");
        }
        
        public override void Unload()
        {
            base.Unload();
            Log.Info("SimpleSyncManager", "Closed.");
        }

        [ProtoContract]
        public class InternalSimpleSyncBothWays : PacketBase
        {
            [ProtoMember(1)] public long SyncId;
            [ProtoMember(2)] public byte[] Contents;

            public override void Received(ulong senderSteamId, bool fromServer)
            {
                if (fromServer && MyAPIGateway.Session.IsServer)
                    return;

                ISimpleSync theSync;
                if (!SimpleSyncManager.I._syncIdMap.TryGetValue(SyncId, out theSync))
                    return;
                theSync.UpdateFromNetwork(Contents);
            }

            public override PacketInfo GetInfo()
            {
                return PacketInfo.FromPacket(this,
                    new PacketInfo
                    {
                        PacketTypeName = nameof(SyncId),
                        PacketSize = sizeof(long)
                    },
                    new PacketInfo
                    {
                        PacketTypeName = nameof(Contents),
                        PacketSize = Contents == null ? 0 : Contents.Length * sizeof(byte)
                    }
                );
            }
        }
    }
}
