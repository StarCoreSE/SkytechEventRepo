using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;
using VRage.Input;

namespace Skytech.Engines
{
    internal class FuelEngine : AssemblyBase
    {
        public float MaxRpmLimit { get; private set; } = 1;
        public float Rpm { get; private set; } = 1; // TODO

        public float Power { get; private set; } = 0;
        public float MaxPower { get; private set; } = 0;
        public float FuelUse { get; private set; } = 0;
        public bool AnyCylindersOverheated { get; private set; } = false;
        public float AverageCylinderTemp { get; private set; } = 0;

        /// <summary>
        /// Total cooling from radiators.
        /// </summary>
        public float RadiatorCooling { get; private set; } = 0; // TODO

        public HashSet<FuelEngineCylinder> Cylinders = new HashSet<FuelEngineCylinder>();

        public override void OnPartAdd(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartAdd(block, isBasePart);

            FuelEngineCylinder cyl;
            if (AssemblyManager<FuelEngineCylinder>.TryGet(block, out cyl))
            {
                cyl.Engine = this;
                Cylinders.Add(cyl);
            }
            else switch (block.BlockDefinition.SubtypeName)
            {
                case "ST_T_Radiator": // born to hardcode
                    RadiatorCooling += FuelEngineCylinder.RadiatorCooling * 1;
                    break;
                case "ST_T_LargeRadiator":
                    RadiatorCooling += FuelEngineCylinder.RadiatorCooling * 9;
                    break;
            }
        }

        public override void OnPartRemove(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartRemove(block, isBasePart);

            FuelEngineCylinder cyl;
            if (AssemblyManager<FuelEngineCylinder>.TryGet(block, out cyl))
            {
                cyl.Engine = this;
                Cylinders.Add(cyl);
            }
            else switch (block.BlockDefinition.SubtypeName)
            {
                case "ST_T_Radiator":
                    RadiatorCooling -= FuelEngineCylinder.RadiatorCooling * 1;
                    break;
                case "ST_T_LargeRadiator":
                    RadiatorCooling -= FuelEngineCylinder.RadiatorCooling * 9;
                    break;
            }
        }

        protected override void BlockInfoCallback(IMyCubeBlock block, StringBuilder sb)
        {
            base.BlockInfoCallback(block, sb);

            if (block.BlockDefinition.SubtypeName == "ST_T_Cylinder")
                return;

            sb.AppendLine($"RPM: {Rpm*100:F0}%"); // TODO unique info for each block type would be cool
            sb.AppendLine($"Power: {Power:F}/{MaxPower:F}");
            sb.AppendLine($"Fuel Use: {FuelUse:F}");
            sb.AppendLine($"Power per Fuel: {Power/FuelUse:F}");
            sb.AppendLine($"Avg. Cylinder Temp: {AverageCylinderTemp*100:N0}% {(AnyCylindersOverheated ? " !OVERHEAT!" : "")}");
        }

        public override void UpdateTick()
        {
            UpdatePower();

            if (MyAPIGateway.Input.IsNewKeyPressed(MyKeys.PageUp))
            {
                Rpm += 0.1f;
            }
            if (MyAPIGateway.Input.IsNewKeyPressed(MyKeys.PageDown))
            {
                Rpm -= 0.1f;
            }
        }

        private void UpdatePower()
        {
            AverageCylinderTemp = 0;
            FuelUse = 0;
            Power = 0;
            MaxPower = 0;

		    // TODO priority, maybe?

            if (Rpm > 0)
            {
                foreach (var cyl in Cylinders)
                {
                    if (cyl.Overheated)
                        continue;

                    // cylinder already updated in its own logic
                    Power += cyl.PowerWithNoHeat(Rpm) * (1 - cyl.HeatLevel * FuelEngineCylinder.MaxHeatPowerPenalty);
                    MaxPower += cyl.MaxPowerNoHeat(MaxRpmLimit);
                    FuelUse += cyl.GetFuelRate(Rpm, false);
                    AverageCylinderTemp += cyl.HeatLevel;
                    if (cyl.Overheated)
                        AnyCylindersOverheated = true;
                }
            }

            AverageCylinderTemp /= Cylinders.Count;
        }
    }
}
