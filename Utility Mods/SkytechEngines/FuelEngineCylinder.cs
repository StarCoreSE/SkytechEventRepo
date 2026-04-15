using Collections;
using Sandbox.ModAPI;
using Skytech.Engines.Shared.Exhaust;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;

namespace Skytech.Engines
{
    internal class FuelEngineCylinder : AssemblyBase, IExhaustProducer
    {
        public const float ExhaustPerFuel = 1;
        public const float BaseFuelRate = 0.3f;
        public const float CarbFuelRate = 2.5f;
        public const float InjectorFuelRate = 8f;
        public const float BasePowerPerFuel = 54f;
        public const float SuperchargerBonusMult = 0.4f;
        public const float HeatPerFuel = 2f;
        public const float CoolingPerExhaust = 4f;
        public const float BaseCooling = 0.1f;
        public const float CoolingRange = 8f;
        public const float OverheatLevel = 0.95f;
        public const float EndOverheatLevel = 0.2f;
        public const float RadiatorCooling = 3f;
        public const float MaxHeatPowerPenalty = 0.5f;

        public FuelEngine Engine = null; // Set by owning engine
        public HashSet<FuelEngineCarburettor> Carburettors = new HashSet<FuelEngineCarburettor>();
        public HashSet<IMyCubeBlock> Injectors = new HashSet<IMyCubeBlock>();

        public bool Overheated = false;
        public float BaseFuelBurnRate = 0;
        /// <summary>
        /// Heat fraction, 0-1
        /// </summary>
        public float HeatLevel { get; private set; } = 0;

        public ICollection<FuelEngineExhaust> OutletAssembly => OutletExhaust;
        public CleanedList<FuelEngineExhaust> OutletExhaust { get; set; } = new CleanedList<FuelEngineExhaust>();
        public CleanedSet<Turbo> OutletTurbos { get; set; } = new CleanedSet<Turbo>();
        public FuelEngineExhaust.Exhaust ExhaustProduced { get; private set; } = FuelEngineExhaust.Exhaust.Zero;
        public IMyCubeBlock Block { get; private set; }

        public override void OnPartAdd(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartAdd(block, isBasePart);

            // delay a tick to let everything init right
            MyAPIGateway.Utilities.InvokeOnGameThread(() =>
            {
                FuelEngineCarburettor carb;
                if (AssemblyManager<FuelEngineCarburettor>.TryGet(block, out carb))
                {
                    Carburettors.Add(carb);
                    carb.Cylinder = this;
                }
            });

            if (block.BlockDefinition.SubtypeName == "ST_T_Injector")
            {
                Injectors.Add(block);
            }

            if (isBasePart)
            {
                Block = block;
                Grid.OnBlockAdded += OnBlockAdded;

                FuelEngine eng;
                if (AssemblyManager<FuelEngine>.TryGet(block, out eng))
                {
                    Engine = eng;
                    Engine.Cylinders.Add(this);
                }

                // Modular Assemblies is the worst framework ever and update order is kinda wonky. This connects cylinders and adjacent consumers.
                List<IMySlimBlock> neighbors = new List<IMySlimBlock>();
                block.SlimBlock.GetNeighbours(neighbors);
                foreach (var adjacent in neighbors)
                {
                    var fatBlock = adjacent.FatBlock;
                    if (fatBlock == null)
                        continue;

                    Turbo turbo;
                    if (TurboManager.I.TryGetTurbo(fatBlock, out turbo) && turbo.IsInlet(block))
                        OutletTurbos.Add(turbo);
                }
            }
        }

        public override void OnPartRemove(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartRemove(block, isBasePart);
            if (block.BlockDefinition.SubtypeName == "ST_T_Injector")
            {
                Injectors.Remove(block);
            }
        }

        protected override void BlockInfoCallback(IMyCubeBlock block, StringBuilder sb)
        {
            base.BlockInfoCallback(block, sb);

            float ventFac = GetVentingFactor();

            sb.AppendLine($"Carburettors: {Carburettors.Count:N0}");
            sb.AppendLine($"Injectors: {Injectors.Count:N0}");
            sb.AppendLine($"Fuel Use: ({BaseFuelBurnRate:N}/{GetMaxFuelRate(true):N})/s");
            sb.AppendLine($"Temperature: {HeatLevel*100:N0}% {(Overheated ? " !OVERHEATED! " : "")} ({(OutletExhaust.Count + OutletTurbos.Count) * ventFac:N0} cooling from exhaust)");
            if (Engine != null)
                sb.AppendLine($"Power: {(Overheated ? 0 : PowerWithNoHeat(Engine.Rpm) * (1 - HeatLevel * MaxHeatPowerPenalty)):F}/{MaxPowerNoHeat():F}");

            if (ventFac < 1)
                sb.AppendLine($"Some exhausts blocked! Cooling reduced ({ventFac*100:N0}%).");
        }

        public override void UpdateTick()
        {
            base.UpdateTick();

            OutletTurbos.RunCleanup();
            OutletExhaust.RunCleanup();

            UpdateExhaust();
            UpdateHeat();
        }

        public override void Unload()
        {
            base.Unload();
            Grid.OnBlockAdded -= OnBlockAdded;
        }

        public bool IsOutlet(IMyCubeBlock block)
        {
            return block.Position.RectangularDistance(RootBlock.Position) == 1;
        }

        private void OnBlockAdded(IMySlimBlock block)
        {
            if (block.Position.RectangularDistance(RootBlock.Position) != 1)
                return;

            var fatBlock = block.FatBlock;
            Turbo turbo;
            if (fatBlock == null || !TurboManager.I.TryGetTurbo(fatBlock, out turbo) || !turbo.IsInlet(RootBlock))
                return;
            OutletTurbos.Add(turbo);
        }

        public void UpdateExhaust()
        {
            float exhaust = 0;
            if (Engine != null && Engine.HasFuel)
            {
                float rpm = Engine.Rpm;
                BaseFuelBurnRate = GetFuelRate(rpm, false);

                exhaust = ExhaustPerFuel * BaseFuelBurnRate;
            }

            //float exhaustPerInlet = exhaust / Exhausts.Count;
            //
            //if (Math.Abs(_exhaustPerSide - exhaustPerInlet) > 0.0001)
            //{
            //    foreach (var ex in Exhausts.Keys)
            //        ex.NeedsPressureUpdate = true;
            //}
            //_exhaustPerSide = exhaustPerInlet;

            // TODO don't constantly call updates
            // no need to update if close
            //if (Math.Abs(ExhaustProduced.Amount - exhaust) < 0.001f)
            //    return;

            ExhaustProduced = new FuelEngineExhaust.Exhaust(exhaust / (OutletExhaust.Count + OutletTurbos.Count));

            foreach (var asm in OutletExhaust)
                asm.NeedsPressureUpdate = true;
            foreach (var turbo in OutletTurbos)
                turbo.UpdateExhaust(ExhaustProduced);
        }

        #region Cooling

        private float GetExhaustCooling()
        {
            int outletCt = OutletExhaust.Count + OutletTurbos.Count;
            float ventFactor = outletCt == 0 ? 0 : GetVentingFactor();
            return CoolingPerExhaust * outletCt * ventFactor;
        }

        private float GetFlatCooling()
        {
            if (Engine == null)
                return BaseCooling;

            return BaseCooling + GetExhaustCooling() + Engine.RadiatorCooling / Engine.Cylinders.Count;
        }

        private void UpdateHeat()
        {
            // Add heat from burning
            HeatLevel += BaseFuelBurnRate * HeatPerFuel / 6000;
            if (HeatLevel > 1)
                HeatLevel = 1;

            if (HeatLevel >= OverheatLevel)
            {
                Overheated = true;
            }
            else if (Overheated && HeatLevel <= EndOverheatLevel)
            {
                Overheated = false;
            }

            // Remove heat from cooling
            float coolingMod = CoolingRange * (float) Math.Pow(HeatLevel, 2f);
            float cooling = GetFlatCooling() * coolingMod;

            // magic constant times tick rate
            HeatLevel -= cooling / 6000;
        }

        /// <summary>
        /// Ratio of blocked to non-blocked exhausts
        /// </summary>
        /// <returns></returns>
        private float GetVentingFactor()
        {
            if (OutletExhaust.Count == 0 && OutletTurbos.Count == 0)
                return 0;

            float sum = 0;

            foreach (var exhaust in OutletExhaust)
            {
                if (!exhaust.ExhaustObstructed)
                    sum++;
            }
            foreach (var turbo in OutletTurbos)
            {
                if (!turbo.ExhaustObstructed)
                    sum++;
            }

            return sum / (OutletExhaust.Count + OutletTurbos.Count);
        }

        #endregion

        #region Fuel

        public float GetFuelRate(float rpmFrac, bool ignoreOverheated)
        {
            if (rpmFrac == 0)
                return 0;
            return GetMaxFuelRate(ignoreOverheated) * rpmFrac;
        }

        public float GetMaxFuelRate(bool ignoreOverheated)
        {
            if (!ignoreOverheated && Overheated)
                return 0;

            if (Carburettors.Count == 0 && Injectors.Count == 0)
                return BaseFuelRate;

            return Carburettors.Count * CarbFuelRate + Injectors.Count * InjectorFuelRate;
        }

        #endregion

        #region Power

        public float MaxPowerNoHeat(float rpmLimit = -1) => PowerWithNoHeat(rpmLimit == -1 ? Engine.MaxRpmLimit : rpmLimit);

        public float PowerWithNoHeat(float rpm)
        {
            if (Carburettors.Count == 0 && Injectors.Count == 0)
                return BaseFuelBurnRate * BasePowerPerFuel * rpm;

            float power = 0f;
            foreach (var carb in Carburettors)
            {
                power += CarbFuelRate * carb.PowerPerFuel(rpm) * rpm;
            }

            power += InjectorFuelRate * BasePowerPerFuel * rpm * Injectors.Count;

            return power;
        }

        #endregion
    }
}
