using System;
using System.Text;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Skytech.Engines
{
    internal class FuelEngine : AssemblyBase
    {
        public float Rpm { get; private set; } = 1; // TODO

        public override void OnPartAdd(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartAdd(block, isBasePart);

            // delay a tick to let everything init right
            FuelEngineCylinder cyl;
            if (AssemblyManager<FuelEngineCylinder>.TryGet(block, out cyl))
            {
                cyl.Engine = this;
            }
        }

        public override void OnPartRemove(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartRemove(block, isBasePart);
        }

        protected override void BlockInfoCallback(IMyCubeBlock block, StringBuilder sb)
        {
            base.BlockInfoCallback(block, sb);
            sb.AppendLine($"Engine RPM: {Rpm*100:F0}%");
        }
    }
}
