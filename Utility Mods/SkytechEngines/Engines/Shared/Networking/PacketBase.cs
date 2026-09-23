using ProtoBuf;
using System;
using AriUtils;
using Sandbox.ModAPI;

namespace Skytech.Engines.Shared.Networking
{
    [ProtoInclude(GlobalData.ServerNetworkId + 1, typeof(SimpleSyncManager.InternalSimpleSyncBothWays))]
    [ProtoInclude(GlobalData.ServerNetworkId + 2, typeof(NetworkProfiler.NetworkProfilePacket))]
    [ProtoContract(UseProtoMembersOnly = true)]
    public abstract class PacketBase
    {
        /// <summary>
        /// Called whenever your packet is received.
        /// </summary>
        public abstract void Received(ulong senderSteamId, bool fromServer);

        /// <summary>
        /// Gets profiling info for this packet
        /// </summary>
        /// <returns></returns>
        public abstract PacketInfo GetInfo();

        public struct PacketInfo
        {
            public string Name => PacketType?.Name ?? PacketTypeName;

            public long Timestamp;
            public Type PacketType;
            /// <summary>
            /// optional if PacketType is not available
            /// </summary>
            public string PacketTypeName;
            public int PacketSize;
            public PacketInfo[] SubPackets;

            public static PacketInfo FromPacket(PacketBase packet)
            {
                return new PacketInfo
                {
                    PacketType = packet.GetType(),
                    PacketSize = MyAPIGateway.Utilities.SerializeToBinary(packet).Length
                };
            }

            public static PacketInfo FromPacket(PacketBase packet, params PacketInfo[] subPackets)
            {
                return new PacketInfo
                {
                    PacketType = packet.GetType(),
                    PacketSize = MyAPIGateway.Utilities.SerializeToBinary(packet).Length,
                    SubPackets = subPackets
                };
            }
        }
    }
}
