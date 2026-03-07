using Sandbox.Definitions;
using Sandbox.ModAPI;
using Skytech.Thrusters.Shared.Utils;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace Skytech.Thrusters
{
    internal class Gimbal3x3 : AssemblyBase
    {
        public override void OnPartAdd(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartAdd(block, isBasePart);

            if (isBasePart)
            {
                MiscUtils.AddBlock<MyObjectBuilder_CubeBlock>(block, "GimbalThrustPart", (Vector3I) block.LocalMatrix.Up + block.Position);
                MiscUtils.AddBlock<MyObjectBuilder_CubeBlock>(block, "GimbalThrustPart", (Vector3I) block.LocalMatrix.Right + block.Position);
                MiscUtils.AddBlock<MyObjectBuilder_CubeBlock>(block, "GimbalThrustPart", (Vector3I) block.LocalMatrix.Down + block.Position);
                MiscUtils.AddBlock<MyObjectBuilder_CubeBlock>(block, "GimbalThrustPart", (Vector3I) block.LocalMatrix.Left + block.Position);
            }
        }

        public override void OnPartRemove(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartRemove(block, isBasePart);

            foreach (var part in Blocks)
                Grid.RemoveBlock(part.SlimBlock);
        }
    }
}
