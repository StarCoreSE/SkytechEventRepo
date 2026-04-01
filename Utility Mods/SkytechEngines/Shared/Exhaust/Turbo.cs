using AriUtils.HUD;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;
using VRageMath;

namespace Skytech.Engines.Shared.Exhaust
{
    internal class Turbo : IExhaustProducer, IExhaustConsumer
    {
        public const float BonusMultiplier = 1/3f;
        public const float GasForMaxBonus = 6f;

        public IMyCubeBlock Block { get; private set; }
        public TurboManager.TurboDef Definition { get; private set; } // non-readonly because c# is a bit weird

        public Vector3I ExhaustIn { get; private set; }
        public Vector3I ExhaustOut { get; private set; }
        public Vector3I Carburettor { get; private set; }

        public float PressureUse { get; private set; } = 0;
        public float TurboBonus { get; private set; } = 0;
        public bool ExhaustObstructed => OutletAssembly.Count == 0 || OutletAssembly[0].ExhaustObstructed; // safe to assume there's only one outlet assembly


        public List<FuelEngineExhaust> OutletAssembly { get; set; } = new List<FuelEngineExhaust>();
        public FuelEngineExhaust.Exhaust ExhaustProduced { get; private set; } = FuelEngineExhaust.Exhaust.Zero;
        public bool IsClosed => Block.Closed;



        public Turbo(IMyCubeBlock block, TurboManager.TurboDef def)
        {
            Block = block;
            Definition = def;

            Block.CubeGridChanged += OnGridChanged;
            OnGridChanged(block.CubeGrid);

            BlockInfo.Register(block, BlockInfoCallback);
        }

        private void BlockInfoCallback(IMyCubeBlock block, StringBuilder sb)
        {
            sb.AppendLine($"Turbo Bonus: {TurboBonus*100:N0}%");
            sb.AppendLine($"Pressure Used: {PressureUse:F1}/{GasForMaxBonus:F1}");
            if (ExhaustObstructed)
            {
                sb.AppendLine($"Exhaust obstructed!");
            }
        }

        /// <summary>
        /// Checks if block could be an inlet for this turbo.
        /// </summary>
        /// <param name="exhaustBlock"></param>
        /// <returns></returns>
        public bool IsInlet(IMyCubeBlock exhaustBlock)
        {
            return ExhaustIn.IsInsideInclusiveEnd(exhaustBlock.Min, exhaustBlock.Max);
        }

        /// <summary>
        /// Checks if block could be an outlet for this turbo.
        /// </summary>
        /// <param name="exhaustBlock"></param>
        /// <returns></returns>
        public bool IsOutlet(IMyCubeBlock exhaustBlock)
        {
            return ExhaustOut.IsInsideInclusiveEnd(exhaustBlock.Min, exhaustBlock.Max);
        }

        public void UpdateExhaust(FuelEngineExhaust.Exhaust available)
        {
            PressureUse = Math.Min(available.Pressure, GasForMaxBonus);
            TurboBonus = (float) MathHelper.Clamp(Math.Pow(PressureUse / GasForMaxBonus, 0.35f), 0, 1) * BonusMultiplier;

            ExhaustProduced = new FuelEngineExhaust.Exhaust(available.Pressure - PressureUse, available.Amount);

            foreach (var asm in OutletAssembly)
                asm.NeedsPressureUpdate = true;
        }

        /// <summary>
        /// Update grid-aligned block directions
        /// </summary>
        /// <param name="grid"></param>
        private void OnGridChanged(IMyCubeGrid grid)
        {
            ExhaustIn = Definition.BlockExhaustIn(Block);
            ExhaustOut = Definition.BlockExhaustOut(Block);
            Carburettor = Definition.BlockCarburettor(Block);
        }
    }
}
