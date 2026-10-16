using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using AriUtils;
using ModularAssemblies.Utils;
using Sandbox.Game.Components;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRageMath;

namespace Skytech.Thrusters.Shared
{
    internal class Gimbal3x3 : AssemblyBase
    {
        public MyThrust Thruster = null;
        public float ThrustMultiplier { get; set; } = 0;
        public float DesiredThrusterPowerPct { get; private set; } = 0;
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
                Unload(); // removing any non-thruster part destroys the entire assembly
            }
        }

        private void OnPartUpdateVisual(MyCubeBlock block)
        {
            // slight delay is intentional, helps with order of operations for painting and skinning
            MyAPIGateway.Utilities.InvokeOnGameThread(() =>
            {
                if (Thruster == null || DrawDummy == null)
                    return;

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
            ((IMyThrust)Thruster).ThrustMultiplier = 0.0001f;
            //MyAPIGateway.Utilities.ShowNotification($"HasThrust: {Thruster != null}. Damping: {GridDamping}", 1000/60);

            if (ThrustMultiplier == 0) // thruster can't run so don't do anything
                return;

            Vector3 ctrlInput = Vector3.Zero;
            if (Grid.ControlSystem.IsControlled)
            {
                GridDamping = Grid.ControlSystem.CurrentShipController.EnabledDamping;
                ctrlInput = Vector3.Transform(Grid.ControlSystem.CurrentShipController.LastMotionIndicator, ((IMyShipController)Grid.ControlSystem.CurrentShipController.Entity).LocalMatrix.GetOrientation());
            }

            if (GridDamping)
            {
                var damping = GetDampingForce() / (Thruster.BlockDefinition.ForceMagnitude * ThrustMultiplier); // TODO adjust based on *total* gimbal thruster force
                float dLen = damping.Length();
                if (dLen > 1)
                    damping /= dLen;
                MyAPIGateway.Utilities.ShowNotification($"Damping Force: {GetDampingForce().Length():N0} ({damping.Length()*100:N0}%)", 1000/60);

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
            DesiredThrusterPowerPct = 0;
            if (ctrlInLength > 0.05f) // don't thrust/rotate if input is low enough
            {
                LastTargetRotation = ctrlInput;

                if (Thruster.IsWorking)
                {
                    float thrustForceMult = Vector3.Dot(ctrlInput, DrawDummy.PositionComp.LocalMatrixRef.Backward);
                    if (thrustForceMult > 0)
                    {
                        DesiredThrusterPowerPct = thrustForceMult; // TODO REMOVE TEST
                        thrustForceMult *= ThrustMultiplier;
                        float thrust = Thruster.BlockDefinition.ForceMagnitude * thrustForceMult;
                        
                        MyAPIGateway.Utilities.ShowNotification($"Gimbal Thrust: {thrust/1000:N}kN ({Vector3.Dot(ctrlInput, DrawDummy.PositionComp.LocalMatrixRef.Backward)*100:N0}%->{DesiredThrusterPowerPct*100:N0}%)", 1000/60);

                        // TODO decide if we should apply impulse to thruster position (realistic) instead of direct impulse
                        Grid.Physics.ApplyImpulse(DrawDummy.WorldMatrix.Backward * thrust / 60f, Grid.Physics.CenterOfMassWorld); // J = F * dT
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

        private Vector3 GetDampingForce() // relative damping is for nerds
        {
            Vector3 gravityForce = Vector3.Zero;
            float counterGravityForce = 0;
            if (Grid.NaturalGravity.Volume != 0) // vector length expensive
            {
                List<IMyCubeGrid> gridGroup = GlobalData.I.GridListPool.Pop();
                Grid.GetGridGroup(GridLinkTypeEnum.Physical).GetGrids(gridGroup);
                foreach (var grid in gridGroup)
                {
                    gravityForce += grid.Physics.Mass * grid.NaturalGravity; // grids can probably have different gravity idk
                    counterGravityForce += GridThrustTracker.I.GetCounterGravityThrust(grid);
                }
                GlobalData.I.GridListPool.Push(gridGroup);
            }
            MyAPIGateway.Utilities.ShowNotification($"GForceNet: {gravityForce.Length():N0} vs {counterGravityForce:N0} ({gravityForce.Length() - counterGravityForce:N0})", 1000/60);
            float gravityForceNet = Math.Max(0, gravityForce.Normalize() - counterGravityForce);
            gravityForce *= gravityForceNet;

            Vector3 velocityForce = Grid.Physics.LinearVelocity * Grid.Physics.LinearVelocity / 60 * Grid.Physics.Mass; // TODO this needs to be tied into [all] thrusters

            Matrix matrix = Grid.PositionComp.WorldMatrixNormalizedInv; // idk what's going on with velocity help
            return -Vector3.TransformNormal((Grid.Physics.LinearVelocity * Grid.Physics.LinearVelocity / 60) * Grid.Physics.Mass + gravityForce, matrix);
        }

        private sealed class AnimationPanel : MyEntity
        {
            public AnimationPanel(string model, Matrix localMatrix, MyEntity parent)
            {
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
