using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Skytech.Thrusters.Shared.Utils;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Skytech.Thrusters
{
    internal class Gimbal3x3 : AssemblyBase
    {
        public IMyThrust Thruster = null;
        private AnimationPanel DrawDummy = null;

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

            if (block is IMyThrust)
            {
                Thruster = (IMyThrust) block;
                Thruster.ThrustMultiplier = 0.000001f;

                Matrix discard;
                DrawDummy = new AnimationPanel(Thruster.CalculateCurrentModel(out discard), Thruster.LocalMatrix * Matrix.Invert(RootBlock.LocalMatrix), (MyEntity) RootBlock);
                DrawDummy.Render.EnableColorMaskHsv = true;
                DrawDummy.Render.ColorMaskHsv = Thruster.Render.ColorMaskHsv; // dummy isn't skinnable due to API limitations but is recolorable. TODO update color automatically
                Thruster.Render.Visible = false;
            }
        }

        public override void OnPartRemove(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartRemove(block, isBasePart);

            if (block == Thruster)
            {
                Thruster = null;
                DrawDummy.Close();
                DrawDummy = null;
            }
            else
            {
                foreach (var part in Blocks)
                {
                    if (part == Thruster)
                        continue;

                    Grid.RemoveBlock(part.SlimBlock);
                }
            }
        }

        public override void UpdateTick()
        {
            if (Thruster == null)
                return;

            MyAPIGateway.Utilities.ShowNotification($"HasThrust: {Thruster != null}", 1000/60);

            DrawDummy.RotateAroundLocalAxis(Vector3.Forward, 1f / 60f);
        }

        private sealed class AnimationPanel : MyEntity
        {
            public AnimationPanel(string model, Matrix localMatrix, MyEntity parent)
            {
                Init(null, model, parent, 1);
                if (string.IsNullOrEmpty(model))
                    Flags &= ~EntityFlags.Visible;
                Save = false;
                NeedsWorldMatrix = true;

                PositionComp.SetLocalMatrix(ref localMatrix);
                MyEntities.Add(this);
            }

            public void RotateAroundLocalAxis(Vector3 axis, float angle)
            {
                var newMatrix = PositionComp.LocalMatrixRef.GetOrientation();
                newMatrix *= Matrix.CreateFromAxisAngle(axis, angle);
                newMatrix.Translation = PositionComp.LocalMatrixRef.Translation;

                PositionComp.SetLocalMatrix(ref newMatrix);
            }
        }
    }
}
