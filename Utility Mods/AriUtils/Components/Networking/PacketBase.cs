using ProtoBuf;
using System;
using AriUtils.Components.Networking.SimpleSync;
using Sandbox.ModAPI;

namespace AriUtils.Components.Networking
{
    /// <summary>
    /// Base type all packets must inherit from. Create a partial class in your mod assembly for ProtoIncludes on all added subtypes.
    /// </summary>
    [ProtoInclude(GlobalData.ServerNetworkId + 1, typeof(SimpleSyncManager.InternalSimpleSyncBothWays))]
    [ProtoInclude(GlobalData.ServerNetworkId + 2, typeof(NetworkProfiler.NetworkProfilePacket))]
    [ProtoContract(UseProtoMembersOnly = true)]
    public abstract partial class PacketBase
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
