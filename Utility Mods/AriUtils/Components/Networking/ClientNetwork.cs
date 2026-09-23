using AriUtils.Components.Networking.SimpleSync;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AriUtils.Components.Networking
{
    public class ClientNetwork : SingletonBase<ClientNetwork>
    {
        // We only need one because it's only being sent to the server.
        private readonly HashSet<PacketBase> _packetQueue = new HashSet<PacketBase>();
        public NetworkProfiler Profiler = new NetworkProfiler(false);
        private SimpleSyncManager _syncManager;

        public override void Init()
        {
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(GlobalData.ClientNetworkId, ReceivedPacket);
            if (SimpleSyncManager.I == null)
            {
                SimpleSyncManager.CreateWithOwner<SharedMain>();
            }
            _syncManager = SimpleSyncManager.I;

            Log.Info("ClientNetwork", "Ready.");
        }

        public override void Unload()
        {
            base.Unload();
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(GlobalData.ClientNetworkId, ReceivedPacket);
            Log.Info("ClientNetwork", "Closed.");
        }

        public override void Update()
        {
            if (_packetQueue.Count > 0)
            {
                if (MyAPIGateway.Session.IsServer)
                {
                    foreach (var packet in _packetQueue.ToArray())
                    {
                        try
                        {
                            packet.Received(0, false);
                        }
                        catch (Exception ex)
                        {
                            Log.Exception("ClientNetwork", ex);
                        }
                    }
                }
                else
                {
                    try
                    {
                        byte[] serialized = MyAPIGateway.Utilities.SerializeToBinary(_packetQueue.ToArray());
                        MyAPIGateway.Multiplayer.SendMessageToServer(GlobalData.ServerNetworkId, serialized);
                        if (Profiler.Active)
                            Profiler.LogUpPackets(_packetQueue, serialized.Length);
                    }
                    catch (Exception ex)
                    {
                        Log.Exception("ClientNetwork", new Exception($"Failed to serialize packet containing {string.Join(", ", _packetQueue.Select(p => p.GetType().Name).Distinct())}.", ex));
                    }
                }

                _packetQueue.Clear();
            }

            Profiler.Update();
        }

        void ReceivedPacket(ushort channelId, byte[] serialized, ulong senderSteamId, bool isSenderServer)
        {
            try
            {
                PacketBase[] packets = MyAPIGateway.Utilities.SerializeFromBinary<PacketBase[]>(serialized);
                foreach (var packet in packets)
                    packet.Received(senderSteamId, true);
                if (Profiler.Active)
                    Profiler.LogDownPackets(packets, serialized.Length);
            }
            catch (Exception ex)
            {
                Log.Exception("ClientNetwork", ex);
            }
        }


        public static void SendToServer(PacketBase packet) =>
            I?.SendToServerInternal(packet);

        private void SendToServerInternal(PacketBase packet)
        {
            if (packet == null)
                return;

            if (Environment.CurrentManagedThreadId != GlobalData.MainThreadId)
            {
                // avoid thread contention
                MyAPIGateway.Utilities.InvokeOnGameThread(() => SendToServerInternal(packet));
                return;
            }

            _packetQueue.Add(packet);
        }
    }
}
