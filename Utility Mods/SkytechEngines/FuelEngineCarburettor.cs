using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using System.Text;
using Skytech.Engines.Shared.Exhaust;
using VRage.Game.ModAPI;
using VRageMath;

namespace Skytech.Engines
{
    internal class FuelEngineCarburettor : AssemblyBase
    {
        public FuelEngineCylinder Cylinder;
        public List<Turbo> Turbos = new List<Turbo>();

        public int SuperchargerCount = 0;
        public float TurboBonus = 0;

        private float _superchargerBonus = 0;

        public override void OnPartAdd(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartAdd(block, isBasePart);

            if (isBasePart)
            {
                MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                {
                    FuelEngineCylinder cyl;
                    if (AssemblyManager<FuelEngineCylinder>.TryGet(block, out cyl))
                    {
                        cyl.Carburettors.Add(this);
                        Cylinder = cyl;
                    }
                });
            }
            else
            {
                MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                {
                    Turbo turbo;
                    if (TurboManager.I.TryGetTurbo(block, out turbo))
                    {
                        Turbos.Add(turbo);
                    }
                });

                if (block.BlockDefinition.SubtypeName == "ST_T_Supercharger")
                    SuperchargerCount++;
            }
        }

        public override void OnPartRemove(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartRemove(block, isBasePart);
            
            Turbo turbo;
            if (TurboManager.I.TryGetTurbo(block, out turbo))
            {
                Turbos.Remove(turbo);
            }

            if (block.BlockDefinition.SubtypeName == "ST_T_Supercharger")
                SuperchargerCount--;
        }

        public override void UpdateTick()
        {
            TurboBonus = 0;
            foreach (var turbo in Turbos)
            {
                TurboBonus += turbo.TurboBonus;
            }
        }

        protected override void BlockInfoCallback(IMyCubeBlock block, StringBuilder sb)
        {
            base.BlockInfoCallback(block, sb);
            // TODO
            sb.AppendLine($"Turbos: {Turbos.Count} (+{TurboBonus*100:N0}%)");
            sb.AppendLine($"Superchargers: {SuperchargerCount} (+{_superchargerBonus*100:N0}%)");
        }

        public override void Unload()
        {
            base.Unload();
            Cylinder?.Carburettors.Remove(this);
        }

        public float PowerPerFuel(float rpm)
        {
            _superchargerBonus = SuperchargerCount * (1f - MathHelper.Clamp(rpm, 0, 1)) * FuelEngineCylinder.SuperchargerBonusMult;
            return FuelEngineCylinder.BasePowerPerFuel * (1 + TurboBonus + _superchargerBonus);
        }
    }
}
