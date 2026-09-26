using ModularAssemblies.Utils;
using Sandbox.ModAPI;
using Skytech.Engines.Shared;
using System;
using VRage.Game.ModAPI;

namespace Skytech.Thrusters.Shared
{
    internal class ShaftedThruster : AssemblyBase
    {
        private readonly string[] _driveshaftParts = AssemblyManager<Driveshaft>.Definition.AllowedBlockSubtypes;

        private IMyThrust Thruster;
        private IMyCubeBlock ShaftBlock;
        private Driveshaft Shaft;
        private Gimbal3x3 Gimbal;

        public override void OnPartAdd(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartAdd(block, isBasePart);

            if (_driveshaftParts.Contains(block.BlockDefinition.SubtypeName))
            {
                ShaftBlock = block;
                UpdateDriveshaft(block, isBasePart);
            }
            else if (block is IMyThrust)
            {
                Thruster = (IMyThrust) block;
            }
            else if (isBasePart)
            {
                AssemblyManager<Gimbal3x3>.TryGet(block, out Gimbal);
            }
        }

        public override void OnPartRemove(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartRemove(block, isBasePart);
            if (block == ShaftBlock)
            {
                ShaftBlock = null;
            }

            if (block == Thruster)
            {
                Thruster = null;
            }
        }

        public override void Unload()
        {
            base.Unload();
        }

        public override void UpdateTick()
        {
            if (Thruster == null || ShaftBlock == null)
                return;
            if (Shaft == null)
            {
                if (AssemblyManager<Driveshaft>.TryGet(ShaftBlock, out Shaft))
                {
                    Shaft.OnPartRemoved += UpdateDriveshaft;
                }
                else
                {
                    Gimbal.ThrustMultiplier = 0;
                    return;
                }
            }

            Shaft.UsedPower += Gimbal.DesiredThrusterPower * 1000f;
            Gimbal.ThrustMultiplier = Shaft.AvailablePowerPct;
        }

        private void UpdateDriveshaft(IMyCubeBlock shaftBlock, bool isBaseBlock)
        {
            if (shaftBlock != ShaftBlock)
                return;

            if (Shaft != null)
            {
                Shaft.OnPartRemoved -= UpdateDriveshaft;
            }

            Shaft = null;
        }
    }
}
