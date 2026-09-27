using System.Collections.Generic;

namespace Skytech.Thrusters.Shared
{
    internal static class ThrusterConstants
    {
        public static readonly Dictionary<string, ThrusterInfo> ThrusterInfos = new Dictionary<string, ThrusterInfo>
        {
            ["LargeBlockSmallAtmosphericThrust"] = new ThrusterInfo
            {
                MaxPowerConsumption = 4800,
            },
            ["LargeBlockLargeAtmosphericThrust"] = new ThrusterInfo
            {
                MaxPowerConsumption = 33600,
            },
        };

        public struct ThrusterInfo
        {
            /// <summary>
            /// Maximum shaft power consumption in kW at full thrust.
            /// </summary>
            public float MaxPowerConsumption;
        }
    }
}
