using ModularAssemblies.Utils;
using Sandbox.ModAPI;
using Skytech.Engines.Shared;
using System;
using AriUtils;
using Sandbox.Game.Entities;
using VRage.Game.ModAPI;
using VRageMath;

namespace Skytech.Thrusters.Shared
{
    internal class ShaftedThruster : AssemblyBase
    {
        private readonly string[] _driveshaftParts = AssemblyManager<Driveshaft>.Definition.AllowedBlockSubtypes;

        private IMyThrust Thruster;
        private ThrusterConstants.ThrusterInfo ThrustInfo;
        private IMyCubeBlock ShaftBlock;
        private Driveshaft Shaft;
        private IMyCubeBlock GimbalBlock;
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
                ThrustInfo = ThrusterConstants.ThrusterInfos[Thruster.BlockDefinition.SubtypeName];

                foreach (var component in Thruster.Components)
                {
                    Log.Info("Thruster Components", component.GetType().Name);
                }
            }
            else if (block.BlockDefinition.SubtypeName == "Gimbal3x3Center")
            {
                GimbalBlock = block;
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

            if (block == GimbalBlock)
            {
                GimbalBlock = null;
                Gimbal = null;
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

            if (GimbalBlock != null && Gimbal == null)
            {
                AssemblyManager<Gimbal3x3>.TryGet(GimbalBlock, out Gimbal);
            }

            if (Shaft == null)
            {
                if (AssemblyManager<Driveshaft>.TryGet(ShaftBlock, out Shaft))
                {
                    Shaft.OnPartRemoved += UpdateDriveshaft;
                }
                else
                {
                    if (Gimbal != null)
                    {
                        Gimbal.ThrustMultiplier = 0;
                    }
                    return;
                }
            }

            // allows directly connected thrusters
            if (Gimbal == null)
            {
                Shaft.UsedPower += Thruster.CurrentThrustPercentage/100 * ThrustInfo.MaxPowerConsumption; // TODO this doesn't take thruster usage into account
                Thruster.ThrustMultiplier = Shaft.AvailablePowerPct;
            }
            else
            {
                Shaft.UsedPower += Gimbal.DesiredThrusterPowerPct * ThrustInfo.MaxPowerConsumption;
                Gimbal.ThrustMultiplier = Shaft.AvailablePowerPct;
            }
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
