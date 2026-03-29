using Sandbox.ModAPI;
using Skytech.Engines.Shared.Exhaust;
using System;
using System.Collections.Generic;
using System.Text;
using Collections;
using VRage.Game.ModAPI;

namespace Skytech.Engines
{
    internal class FuelEngineCylinder : AssemblyBase, IExhaustProducer
    {
        public const float ExhaustPerFuel = 10; // TODO this is 1!
        public const float BaseFuelRate = 0.3f;
        public const float CarbFuelRate = 2.5f;
        public const float InjectorFuelRate = 8f;

        public FuelEngine Engine = null; // Set by owning engine
        public List<FuelEngineCarburettor> Carburettors = new List<FuelEngineCarburettor>();
        public List<IMyCubeBlock> Injectors = new List<IMyCubeBlock>();

        public bool Overheated = false;
        public float FuelBurnRate = 0;

        public List<FuelEngineExhaust> OutletAssembly { get; set; } = new List<FuelEngineExhaust>();
        public CleanedSet<Turbo> OutletTurbos { get; set; } = new CleanedSet<Turbo>();
        public FuelEngineExhaust.Exhaust ExhaustProduced { get; private set; } = FuelEngineExhaust.Exhaust.Zero;
        public IMyCubeBlock Block { get; private set; }
        public bool IsClosed { get; private set; } = false;

        public override void OnPartAdd(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartAdd(block, isBasePart); // TODO cylinders should get the same turbo connection thingy as the rest but it's kinda fucky because. turbos.
            // TODO placing SPECIFICALLY cylinders doesn't guarantee a connection, same with other producers.

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
                Block = block;
                Grid.OnBlockAdded += OnBlockAdded;

                FuelEngine eng;
                if (AssemblyManager<FuelEngine>.TryGet(block, out eng))
                {
                    Engine = eng;
                }

                // Modular Assemblies is the worst framework ever and update order is kinda wonky. This connects cylinders and adjacent consumers.
                List<IMySlimBlock> neighbors = new List<IMySlimBlock>();
                block.SlimBlock.GetNeighbours(neighbors);
                foreach (var adjacent in neighbors)
                {
                    var fatBlock = adjacent.FatBlock;
                    Turbo turbo;
                    if (fatBlock == null || !TurboManager.I.TryGetTurbo(fatBlock, out turbo) || !turbo.IsInlet(block))
                        continue;
                    OutletTurbos.Add(turbo);
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
            sb.AppendLine($"Carburettors: {Carburettors.Count:N0}");
            sb.AppendLine($"Injectors: {Injectors.Count:N0}");
            sb.AppendLine($"Fuel Use: {FuelBurnRate:N}/s");
            sb.AppendLine($"Max. Fuel Use: {GetMaxFuelRate(true):N}/s");
        }

        public override void UpdateTick()
        {
            base.UpdateTick();

            OutletTurbos.RunCleanup();

            UpdateExhaust();
        }

        public override void Unload()
        {
            base.Unload();
            Grid.OnBlockAdded -= OnBlockAdded;
            IsClosed = true;
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
            float rpm = Engine?.Rpm ?? 0;
            FuelBurnRate = GetFuelRate(rpm, true);

            float exhaust = ExhaustPerFuel * FuelBurnRate;
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

            ExhaustProduced = new FuelEngineExhaust.Exhaust(exhaust / (OutletAssembly.Count + OutletTurbos.Count));

            foreach (var asm in OutletAssembly)
                asm.NeedsPressureUpdate = true;
            foreach (var turbo in OutletTurbos)
                turbo.UpdateExhaust(ExhaustProduced);
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
