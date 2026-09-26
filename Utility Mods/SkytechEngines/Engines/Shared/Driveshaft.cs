using ModularAssemblies.Utils;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;
using VRageMath;

namespace Skytech.Engines.Shared
{
    internal class Driveshaft : AssemblyBase
    {
        // TODO
        public float AvailablePower = 0;
        public float UsedPower = 0;
        public float LastUsedPower = 0;
        public float AvailablePowerPct { get; private set; } = 1;

        private readonly Dictionary<IMyCubeBlock, FuelEngine> _engines = new Dictionary<IMyCubeBlock, FuelEngine>();
        private readonly HashSet<IMyCubeBlock> _partsNeedingUpdate = new HashSet<IMyCubeBlock>();

        public override void OnPartAdd(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartAdd(block, isBasePart);
            if (block.BlockDefinition.SubtypeName == "ST_T_CrankShaft")
            {
                FuelEngine engine;
                AssemblyManager<FuelEngine>.TryGet(block, out engine);
                _engines.Add(block, engine);
                engine.OnPartRemoved += UpdateEngine;
            }
        }

        public override void OnPartRemove(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartRemove(block, isBasePart);
            if (block.BlockDefinition.SubtypeName == "ST_T_CrankShaft")
            {
                FuelEngine engine;
                if (_engines.TryGetValue(block, out engine))
                {
                    engine.OnPartRemoved -= UpdateEngine;
                    _engines.Remove(block);
                }
            }
        }

        public override void UpdateTick()
        {
            if (_partsNeedingUpdate.Count > 0)
            {
                foreach (var enginePart in _partsNeedingUpdate)
                {
                    FuelEngine e;
                    if (!AssemblyManager<FuelEngine>.TryGet(enginePart, out e))
                        continue;
                    _engines.Add(enginePart, e);
                    e.OnPartRemoved += UpdateEngine;
                }

                foreach (var enginePart in _engines.Keys)
                {
                    _partsNeedingUpdate.Remove(enginePart);
                }
            }

            AvailablePower = 0;
            foreach (var engine in _engines.Values)
            {
                AvailablePower += engine.Power;
            }
            LastUsedPower = UsedPower;
            UsedPower = 0;
            AvailablePowerPct = MathHelper.Clamp(AvailablePower / LastUsedPower, 0, 1);
            if (float.IsNaN(AvailablePowerPct))
                AvailablePowerPct = 0;

            MyAPIGateway.Utilities.ShowNotification($"Driveshaft Power Usage: {LastUsedPower:N0}kW/{AvailablePower:N0}kW ({AvailablePowerPct*100:F1}%)", 1000/60);
        }

        protected override void BlockInfoCallback(IMyCubeBlock block, StringBuilder sb)
        {
            base.BlockInfoCallback(block, sb);
            sb.AppendLine($"Power Used: {LastUsedPower:F}/{AvailablePower:F} ({AvailablePowerPct*100:F1}%)");
        }

        private void UpdateEngine(IMyCubeBlock engineBlock, bool isBaseBlock)
        {
            FuelEngine engine;
            if (_engines.TryGetValue(engineBlock, out engine))
            {
                engine.OnPartRemoved -= UpdateEngine;
                _engines.Remove(engineBlock);
            }

            if (!engineBlock.MarkedForClose)
            {
                _partsNeedingUpdate.Add(engineBlock);
            }
        }
    }
}
