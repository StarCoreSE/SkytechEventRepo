using System.Collections.Generic;
using AriUtils;
using Sandbox.Game.Entities;
using Skytech.Engines.Shared.ModularAssemblies;
using VRage.Game.ModAPI;
using VRageMath;

namespace Skytech.Engines.Shared.Exhaust
{
    internal class TurboManager : SingletonBase<TurboManager>
    {
        private static Dictionary<IMyCubeBlock, Turbo> _turbos = new Dictionary<IMyCubeBlock, Turbo>();

        private static readonly Dictionary<string, TurboDef> TurboSubtypes = new Dictionary<string, TurboDef>
        {
            ["ST_T_InlineTurboLeft"] = new TurboDef(
                Vector3I.Right + Vector3I.Backward,
                Vector3I.Backward * 2,
                Vector3I.Up + Vector3I.Forward
                ),
            ["ST_T_InlineTurboRight"] = new TurboDef(
                Vector3I.Left + Vector3I.Backward,
                Vector3I.Backward * 2,
                Vector3I.Up + Vector3I.Forward
                ),
            ["ST_T_TurbochargerLeft"] = new TurboDef(
                Vector3I.Right,
                Vector3I.Forward,
                Vector3I.Down
                ),
            ["ST_T_TurbochargerRight"] = new TurboDef(
                Vector3I.Left,
                Vector3I.Forward,
                Vector3I.Down
                ),
        };


        protected override void _Init()
        {
            GlobalData.RegisterOnBlockAdded(OnBlockAdded);
            GlobalData.RegisterOnBlockRemoved(OnBlockRemoved);
        }

        protected override void _Update()
        {
            
        }

        protected override void _Unload()
        {
            GlobalData.UnregisterOnBlockAdded(OnBlockAdded);
            GlobalData.UnregisterOnBlockRemoved(OnBlockRemoved);
        }

        public bool TryGetTurbo(IMyCubeBlock block, out Turbo turbo) => _turbos.TryGetValue(block, out turbo);

        private void OnBlockAdded(IMyCubeBlock block)
        {
            TurboDef def;
            if (!TurboSubtypes.TryGetValue(block.BlockDefinition.SubtypeName, out def))
                return;

            _turbos[block] = new Turbo(block, def);
        }

        private void OnBlockRemoved(IMyCubeBlock block)
        {
            _turbos.Remove(block);
        }

        public struct TurboDef
        {
            /// <summary>
            /// Block-local exhaust position (in)
            /// </summary>
            public readonly Vector3I ExhaustIn;
            /// <summary>
            /// Block-local exhaust position (out)
            /// </summary>
            public readonly Vector3I ExhaustOut;
            /// <summary>
            /// Block-local carburettor position
            /// </summary>
            public readonly Vector3I Carburettor;

            public TurboDef(Vector3I exhaustIn, Vector3I exhaustOut, Vector3I carburettor)
            {
                ExhaustIn = exhaustIn;
                ExhaustOut = exhaustOut;
                Carburettor = carburettor;
            }

            public Vector3I BlockExhaustIn(IMyCubeBlock block) => ModularDefinition.ModularApi.GetGridConnectingPosition(block, ExhaustIn);
            public Vector3I BlockExhaustOut(IMyCubeBlock block) => ModularDefinition.ModularApi.GetGridConnectingPosition(block, ExhaustOut);
            public Vector3I BlockCarburettor(IMyCubeBlock block) => ModularDefinition.ModularApi.GetGridConnectingPosition(block, Carburettor);
        }
    }
}
