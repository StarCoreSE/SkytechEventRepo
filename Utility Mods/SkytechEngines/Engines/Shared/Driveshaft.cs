using System;
using ModularAssemblies.Utils;

namespace Skytech.Engines.Shared
{
    internal class Driveshaft : AssemblyBase
    {
        // TODO
        public float AvailablePower = 100;
        public float UsedPower = 0;
        public float LastUsedPower = 0;
        public float AvailablePowerPct { get; private set; } = 1;

        public override void UpdateTick()
        {
            base.UpdateTick();
            LastUsedPower = UsedPower;
            UsedPower = 0;
            AvailablePowerPct = Math.Min(LastUsedPower / AvailablePower, 1);
        }
    }
}
