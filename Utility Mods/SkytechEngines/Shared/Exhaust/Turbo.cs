using AriUtils.HUD;
using System;
using System.Text;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Skytech.Engines.Shared.Exhaust
{
    internal class Turbo : IExhaustProducer, IExhaustConsumer
    {
        public const float BonusMultiplier = 1/3f;
        public const float GasForMaxBonus = 6f;

        public readonly IMyCubeBlock Block;
        public TurboManager.TurboDef Definition { get; private set; } // non-readonly because c# is a bit weird

        public Vector3I ExhaustIn { get; private set; }
        public Vector3I ExhaustOut { get; private set; }
        public Vector3I Carburettor { get; private set; }

        public float PressureUse { get; private set; } = 0;
        public float TurboBonus { get; private set; } = 0;
        public FuelEngineExhaust.Exhaust ExhaustProduced { get; private set; } = FuelEngineExhaust.Exhaust.Zero;
        public bool ExhaustObstructed { get; private set; } = true;
        public float MaxPressureUsage { get; } = GasForMaxBonus;

        Action<IExhaustProducer> IExhaustProducer.OnClose { get; set; }
        Action<IExhaustConsumer> IExhaustConsumer.OnClose { get; set; }

        private bool _isUpdateAvailable = false;

        public Turbo(IMyCubeBlock block, TurboManager.TurboDef def)
        {
            Block = block;
            Definition = def;

            Block.CubeGridChanged += OnGridChanged;
            OnGridChanged(block.CubeGrid);

            Block.OnMarkForClose += OnMarkForClose;

            BlockInfo.Register(block, BlockInfoCallback);
        }

        public bool GetIsUpdateAvailable(int id)
        {
            bool tmp = _isUpdateAvailable;
            _isUpdateAvailable = false;
            return tmp;
        }

        private void OnMarkForClose(IMyEntity obj)
        {
            FuelEngineExhaust exhaustAsm;
            if (!GetOutletAssembly(out exhaustAsm))
                return;
            
            // notify exhaust assembly that this doesn't exist anymore
            exhaustAsm.RemoveInlet(this);
            ((IExhaustProducer) this).OnClose?.Invoke(this);
            ((IExhaustConsumer) this).OnClose?.Invoke(this);
        }

        private void BlockInfoCallback(IMyCubeBlock block, StringBuilder sb)
        {
            sb.AppendLine($"Turbo Bonus: {TurboBonus*100:N0}%");
            sb.AppendLine($"Pressure Used: {PressureUse:F1}");
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
        public bool IsExhaustInlet(IMyCubeBlock exhaustBlock)
        {
            return exhaustBlock.LocalAABB.Contains((Vector3) ExhaustIn) != ContainmentType.Disjoint;
        }

        public void UpdateExhaust(FuelEngineExhaust.Exhaust exhaust, out float pressureConsumed)
        {
            pressureConsumed = 0;
            ExhaustObstructed = true;

            FuelEngineExhaust exhaustAsm;
            if (!GetOutletAssembly(out exhaustAsm))
                return;

            pressureConsumed = Math.Max(exhaust.Pressure, GasForMaxBonus);
            PressureUse = pressureConsumed;
            TurboBonus = (float) MathHelper.Clamp(Math.Pow(pressureConsumed / GasForMaxBonus, 0.35f), 0, 1) * BonusMultiplier;

            ExhaustProduced = new FuelEngineExhaust.Exhaust(exhaust.Pressure - pressureConsumed, exhaust.Amount);
            ExhaustObstructed = false;

            exhaustAsm.NeedsPressureUpdate = true;
        }

        private bool GetOutletAssembly(out FuelEngineExhaust exhaustAsm) => AssemblyManager<FuelEngineExhaust>.TryGet(Block.CubeGrid, ExhaustOut, out exhaustAsm);

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

        public FuelEngineExhaust.Exhaust GetExhaustProduced(int id)
        {
            return ExhaustProduced;
        }
    }
}
