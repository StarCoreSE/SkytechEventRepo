using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI;
using Skytech.Engines.Shared.Exhaust;
using VRage.Game.ModAPI;

namespace Skytech.Engines
{
    internal class FuelEngineCylinder : AssemblyBase, IExhaustProducer
    {
        public const float ExhaustPerFuel = 1;
        public const float BaseFuelRate = 0.3f;
        public const float CarbFuelRate = 2.5f;
        public const float InjectorFuelRate = 8f;

        public FuelEngine Engine = null; // Set by owning engine
        public Dictionary<int, int> Exhausts = new Dictionary<int, int>(); // Set by owning exhaust. ID, count.
        public List<FuelEngineCarburettor> Carburettors = new List<FuelEngineCarburettor>();
        public List<IMyCubeBlock> Injectors = new List<IMyCubeBlock>();

        Action<IExhaustProducer> IExhaustProducer.OnClose { get; set; }

        public bool Overheated = false;
        public float FuelBurnRate = 0;

        private float _exhaustPerSide = 0;

        public override void OnPartAdd(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartAdd(block, isBasePart); // TODO cylinders should get the same turbo connection thingy as the rest but it's kinda fucky because. turbos.

            // delay a tick to let everything init right
            MyAPIGateway.Utilities.InvokeOnGameThread(() =>
            {
                FuelEngineCarburettor carb;
                if (AssemblyManager<FuelEngineCarburettor>.TryGet(block, out carb))
                {
                    Carburettors.Add(carb);
                }
            });

            if (block.BlockDefinition.SubtypeName == "ST_T_Injector")
            {
                Injectors.Add(block);
            }

            if (isBasePart)
            {
                foreach (var pos in ModularApi.GetGridConnectingPositions(block, Definition.Name))
                {
                    var adjBlock = Grid.GetCubeBlock(pos)?.FatBlock;
                    
                    if (adjBlock == null)
                        continue;

                    FuelEngineExhaust exhaust;
                    if (AssemblyManager<FuelEngineExhaust>.TryGet(adjBlock, out exhaust))
                    {
                        exhaust.TryRegisterInlet(this);
                    }
                }
            }
        }

        public override void OnPartRemove(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartRemove(block, isBasePart);
            Carburettors.RemoveAll(c => c.RootBlock == block);
            if (block.BlockDefinition.SubtypeName == "ST_T_Injector")
            {
                Injectors.Remove(block);
            }
        }

        protected override void BlockInfoCallback(IMyCubeBlock block, StringBuilder sb)
        {
            base.BlockInfoCallback(block, sb);
            sb.AppendLine($"Exhausts: {Exhausts.Count:N0}");
            sb.AppendLine($"Carburettors: {Carburettors.Count:N0}");
            sb.AppendLine($"Injectors: {Injectors.Count:N0}");
            sb.AppendLine($"Fuel Use: {FuelBurnRate:N}/s");
            sb.AppendLine($"Max. Fuel Use: {GetMaxFuelRate(true):N}/s");
        }

        public override void UpdateTick()
        {
            base.UpdateTick();

            UpdateExhaust();
        }

        public override void Unload()
        {
            base.Unload();
            ((IExhaustProducer) this).OnClose?.Invoke(this);
        }

        public void UpdateExhaust()
        {
            float rpm = Engine?.Rpm ?? 0;
            FuelBurnRate = GetFuelRate(rpm, true);

            float exhaust = ExhaustPerFuel * FuelBurnRate;
            float exhaustPerInlet = exhaust / Exhausts.Count;

            _exhaustPerSide = exhaustPerInlet;
        }

        public FuelEngineExhaust.Exhaust GetExhaustProduced(int id)
        {
            return new FuelEngineExhaust.Exhaust(_exhaustPerSide * Exhausts.GetValueOrDefault(id, 0));
        }

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
    }
}
