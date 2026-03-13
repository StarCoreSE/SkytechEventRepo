using Sandbox.Definitions;
using Sandbox.Engine.Physics;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Skytech.Thrusters.Shared;
using Skytech.Thrusters.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Skytech.Thrusters
{
    internal class Gimbal3x3 : AssemblyBase
    {
        public MyThrust Thruster = null;
        private AnimationPanel DrawDummy = null;
        private bool GridDamping = true;
        private Vector3 LastTargetRotation = Vector3.Zero;
        
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
                Thruster = (MyThrust) block;
                ((IMyThrust)Thruster).ThrustMultiplier = 0.000001f;

                // hide thrust flame, yoinked from Mexpex's setup
                //Thruster.BlockDefinition.FlameFullColor = new Vector4(0);
                //Thruster.BlockDefinition.FlameIdleColor = new Vector4(0);
                //Thruster.BlockDefinition.FlameVisibilityDistance = 0;
                //Thruster.Render.UpdateFlameProperties(false, 0);

                // update color/skinning automatically
                Thruster.OnBlockUpdateVisual += OnPartUpdateVisual;

                // TODO maybe attach other parts connected only to thruster?
                Matrix discard;
                DrawDummy = new AnimationPanel(Thruster.CalculateCurrentModel(out discard), Thruster.PositionComp.LocalMatrixRef, MyGrid);
                OnPartUpdateVisual(Thruster);
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
                Unload();
            }
        }

        private void OnPartUpdateVisual(MyCubeBlock block)
        {
            // slight delay is intentional, helps with order of operations for painting and skinning
            MyAPIGateway.Utilities.InvokeOnGameThread(() =>
            {
                DrawDummy.Render.EnableColorMaskHsv = true;
                DrawDummy.Render.ColorMaskHsv = Thruster.Render.ColorMaskHsv;
                DrawDummy.Render.UpdateRenderTextureChanges(Thruster.Render.TextureChanges);
                Thruster.Render.Visible = false;
            });
        }

        private bool _didUnload = false;
        public override void Unload()
        {
            if (_didUnload)
                return;
            _didUnload = true; // avoid multiple unload

            base.Unload();

            if (Thruster != null) // reset thruster
            {
                Thruster.Render.Visible = true;
                ((IMyThrust)Thruster).ThrustMultiplier = 1f;
            }

            DrawDummy?.Close();
            DrawDummy = null;

            foreach (var part in Blocks.ToArray()) // copy blocks array
            {
                if (part == Thruster || part.Closed)
                    continue;

                Grid.RemoveBlock(part.SlimBlock, true);
            }
        }

        public override void UpdateTick()
        {
            if (Thruster == null)
                return;
            Thruster.Render.UpdateFlameProperties(false, 0);
            //MyAPIGateway.Utilities.ShowNotification($"HasThrust: {Thruster != null}. Damping: {GridDamping}", 1000/60);

            Vector3 ctrlInput = Vector3.Zero;
            if (Grid.ControlSystem.IsControlled)
            {
                GridDamping = Grid.ControlSystem.CurrentShipController.EnabledDamping;
                ctrlInput = Vector3.Transform(Grid.ControlSystem.CurrentShipController.LastMotionIndicator, ((IMyShipController)Grid.ControlSystem.CurrentShipController.Entity).LocalMatrix.GetOrientation());
            }

            if (GridDamping)
            {
                var damping = GetDamping();
                float dLen = damping.Length();
                if (dLen > 1)
                    damping /= dLen;

                if (ctrlInput.X == 0)
                    ctrlInput.X = damping.X;
                if (ctrlInput.Y == 0)
                    ctrlInput.Y = damping.Y;
                if (ctrlInput.Z == 0)
                    ctrlInput.Z = damping.Z;
            }

            Vector3 blockFwd = RootBlock.LocalMatrix.Forward;
            ctrlInput = Vector3.ProjectOnPlane(ref ctrlInput, ref blockFwd);

            if (ModularApi.IsDebug())
            {
                MatrixD m = MatrixD.CreateWorld(DrawDummy.PositionComp.WorldMatrixRef.Translation, DrawDummy.PositionComp.WorldMatrixRef.Forward, RootBlock.PositionComp.WorldMatrixRef.Forward);
                Vector4 c = new Vector4(255f, 0, 0, 125f);
                MySimpleObjectDraw.DrawTransparentCylinder(ref m, 5f, 5f, 1f, ref c, true, 32, 0.1f);

                DebugDraw.AddLine(DrawDummy.PositionComp.WorldMatrixRef.Translation, Vector3D.Transform(ctrlInput.Normalized() * 5 + DrawDummy.PositionComp.LocalMatrixRef.Translation, Grid.WorldMatrix), new Color(0, 255, 0, 125), 0);
                DebugDraw.AddLine(DrawDummy.PositionComp.WorldMatrixRef.Translation, DrawDummy.PositionComp.WorldMatrixRef.Translation + DrawDummy.PositionComp.WorldMatrixRef.Backward * 5, new Color(255, 0, 0, 125), 0);
            }

            float ctrlInLength = ctrlInput.Length();
            if (ctrlInLength > 0.05f) // don't thrust/rotate if input is low enough
            {
                ctrlInput /= ctrlInLength;
                LastTargetRotation = ctrlInput;

                if (Thruster.IsWorking)
                {
                    float thrustForceMult = Vector3.Dot(ctrlInput, DrawDummy.PositionComp.LocalMatrixRef.Backward);

                    MyAPIGateway.Utilities.ShowNotification($"Thrust: {thrustForceMult * Thruster.ThrustForceLength:F}", 1000/60);

                    if (thrustForceMult > 0.5)
                    {
                        // TODO apply to thrusters instead of direct impulse
                        Grid.Physics.ApplyImpulse(DrawDummy.WorldMatrix.Backward * Thruster.ThrustForceLength, Grid.Physics.CenterOfMassWorld);
                        DebugDraw.AddLine(DrawDummy.WorldMatrix.Translation, DrawDummy.WorldMatrix.Translation + DrawDummy.WorldMatrix.Forward * thrustForceMult * 5, Color.Red, 0);
                    }
                }
            }

            // Rotation
            {
                var vecFromTarget = Vector3D.Rotate(LastTargetRotation, MatrixD.Invert(MatrixD.CreateWorld(Vector3.Zero, DrawDummy.PositionComp.LocalMatrixRef.Forward, blockFwd)));
                double tgtAzimuth = Math.Atan2(vecFromTarget.X, vecFromTarget.Z);
                if (double.IsNaN(tgtAzimuth))
                    tgtAzimuth = Math.PI;

                // only rotate if actually needed
                if (Math.Abs(tgtAzimuth) > double.Epsilon)
                {
                    var constrainedAngle = MathUtils.LimitRotationSpeed(0, tgtAzimuth, 1.5f/60);

                    var newMatrix = DrawDummy.PositionComp.LocalMatrixRef.GetOrientation();
                    newMatrix *= Matrix.CreateFromAxisAngle(blockFwd, (float) constrainedAngle);
                    newMatrix.Translation = DrawDummy.PositionComp.LocalMatrixRef.Translation;

                    DrawDummy.PositionComp.SetLocalMatrix(ref newMatrix);

                    //var gridMtrx = Grid.PositionComp.WorldMatrixRef;
                    //DrawDummy.PositionComp.UpdateWorldMatrix(ref gridMtrx);
                }
            }

            //MyAPIGateway.Utilities.ShowNotification($"Thrust: {ctrlInput.ToString("F")}, Angle: {MathHelper.ToDegrees(desiredAzimuth):N0}", 1000/60);
            //DebugDraw.AddLine(Grid.GetPosition(), Vector3.Transform(ctrlInput.X * 5 * Vector3.Right, Grid.WorldMatrix), Color.Red, 0);
            //DebugDraw.AddLine(Grid.GetPosition(), Vector3.Transform(ctrlInput.Y * 5 * Vector3.Up, Grid.WorldMatrix), Color.Green, 0);
            //DebugDraw.AddLine(Grid.GetPosition(), Vector3.Transform(ctrlInput.Z * 5 * Vector3.Backward, Grid.WorldMatrix), Color.Blue, 0);
        }

        private Vector3 GetDamping() // relative damping is for nerds
        {
            Matrix matrix = Grid.PositionComp.WorldMatrixNormalizedInv;
            Vector3 vector = Grid.Physics.Gravity * 0.5f;
            return -Vector3.TransformNormal(Grid.Physics.LinearVelocity + vector, matrix);
        }

        private sealed class AnimationPanel : MyEntity
        {
            public readonly Matrix RefMatrix;
            public readonly Matrix RefMatrixWorld;

            public AnimationPanel(string model, Matrix localMatrix, MyEntity parent)
            {
                RefMatrix = localMatrix;
                Init(null, model, parent, 1);
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
