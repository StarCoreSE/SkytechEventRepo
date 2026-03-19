using System;
using System.Collections.Generic;
using System.Text;
using RichHudFramework.Client;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;

namespace AriUtils.HUD
{
    public class BlockInfo
    {
        private const int UpdateIntervalTicks = 29;
        private const int MaxCastDistance = 50;

        private static BlockInfo I;

        private Dictionary<IMyCubeBlock, Action<IMyCubeBlock, StringBuilder>> _infos = new Dictionary<IMyCubeBlock, Action<IMyCubeBlock, StringBuilder>>();
        private int _lastUpdated = 0;
        private int _tick = 0;
        private StringBuilder _builder = new StringBuilder();
        private BlockInfoDisplay _display = new BlockInfoDisplay(HudMain.HighDpiRoot);

        public static void Init()
        {
            if (MyAPIGateway.Utilities.IsDedicated || I != null)
                return;
            I = new BlockInfo();
        }

        public static void Update()
        {
            if (MyAPIGateway.Utilities.IsDedicated || I == null)
                return;
            I._Update();
        }

        public static void Close()
        {
            if (MyAPIGateway.Utilities.IsDedicated)
                return;
            I = null;
        }

        public static void Register(IMyCubeBlock block, Action<IMyCubeBlock, StringBuilder> info)
        {
            if (I == null)
                return;

            if (I._infos.ContainsKey(block))
            {
                I._infos[block] += info;
            }
            else
            {
                I._infos.Add(block, info);
            }
        }

        public static void Unregister(IMyCubeBlock block, Action<IMyCubeBlock, StringBuilder> info)
        {
            if (I == null || !I._infos.ContainsKey(block))
                return;

            I._infos[block] -= info;

            if (I._infos[block] == null)
                I._infos.Remove(block);
        }

        private BlockInfo()
        {
            I = this;
        }

        private void _Update()
        {
            //if (_tick % UpdateIntervalTicks == 0)
            //{
            //    RayD camLine = new RayD(MyAPIGateway.Session.Camera.Position, MyAPIGateway.Session.Camera.WorldMatrix.Forward);
            //
            //    foreach (var infoKvp in _infos)
            //    {
            //        if (Vector3D.DistanceSquared(block.WorldVolume.Center, camLine.Position) > 15 * 15)
            //            continue;
            //
            //        double? intersect = block.WorldVolume.Intersects(camLine);
            //        if (intersect < 15)
            //        {
            //            _Display(infoKvp.Key, block);
            //        }
            //    }
            //}

            // rolling update
            // evenly distribute updates along UpdateIntervalTicks ticks
            //int nToUpdate = (int) Math.Ceiling((float) _infos.Count / UpdateIntervalTicks);
            //for (int i = _lastUpdated; i < _lastUpdated + nToUpdate && i < _infos.Count; i++)
            //{
            //    IBlockInfo info = _infos[i];
            //    double? intersect = info.Block.WorldVolume.Intersects(camLine);
            //    if (intersect < 50)
            //    {
            //        _Display(info);
            //    }
            //}
            //
            //_lastUpdated += nToUpdate;
            //if (_tick % UpdateIntervalTicks == 0)
            //    _lastUpdated = 0;

            if (_tick % UpdateIntervalTicks == 0 && GlobalData.HudVisible == GlobalData.HudState.VisibleDesc)
            {
                IHitInfo hitInfo;
                MyAPIGateway.Physics.CastRay(MyAPIGateway.Session.Camera.Position,
                    MyAPIGateway.Session.Camera.Position + MyAPIGateway.Session.Camera.WorldMatrix.Forward * 15,
                    out hitInfo);

                if (hitInfo?.HitEntity != null)
                {
                    IMyCubeGrid grid = hitInfo.HitEntity as IMyCubeGrid;

                    if (grid != null)
                    {
                        IMyCubeBlock block = grid.GetCubeBlock(grid.WorldToGridInteger(hitInfo.Position - hitInfo.Normal))?.FatBlock;

                        Action<IMyCubeBlock, StringBuilder> action;
                        if (block != null && _infos.TryGetValue(block, out action))
                        {
                            _Display(action, block);
                        }
                    }
                }
                else
                {
                    _display.Visible = false;
                }
            }
            else if (GlobalData.HudVisible != GlobalData.HudState.VisibleDesc)
            {
                _display.Visible = false;
            }

            _display.UpdatePosition();

            _tick++;
        }

        private void _Display(Action<IMyCubeBlock, StringBuilder> info, IMyCubeBlock block)
        {
            info.Invoke(block, _builder);

            DebugDraw.AddGridPoint(block.Position, block.CubeGrid, Color.Red, 29/60f);
            _display.UpdateInfo(_builder, block.GetPosition());
            _display.Visible = true;
            
            //Log.Info("BlockInfo", $"Display info {_builder.ToString()}");
            _builder.Clear();
        }

        private class BlockInfoDisplay : WindowBase
        {
            private List<LabelBox> _labels = new List<LabelBox>();
            private Vector3D _lastPos = Vector3D.Zero;

            public BlockInfoDisplay(HudParentBase parent) : base(parent)
            {
                BodyColor = new Color(41, 54, 62, 0);
                BorderColor = new Color(58, 68, 77);
                Size = new Vector2(250, 250);
            }

            public void UpdateInfo(StringBuilder sb, Vector3D worldPos)
            {
                _lastPos = worldPos;

                var lines = sb.ToString().Split('\n');
                foreach (var l in _labels) // TODO cache the labels 3:
                    l.Unregister();
                _labels.Clear();
                for (var i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (i == 0)
                    {
                        this.HeaderText = line;
                        continue;
                    }

                    HudParentBase parent = i <= 1 ? (HudParentBase) this.header : (HudParentBase) _labels[i - 2];
                    _labels.Add(new LabelBox(parent)
                    {
                        DimAlignment = DimAlignments.Width,
                        ParentAlignment = ParentAlignments.Bottom,
                        Text = line,
                        Padding = new Vector2(2, 2),
                        Background =
                        {
                            Color = new Color(41, 54, 62, 77),
                        },
                        TextBoard =
                        {
                            BuilderMode = TextBuilderModes.Wrapped,
                            LineWrapWidth = Width
                        }
                    });
                }
            }

            public void UpdatePosition()
            {
                if (!Visible)
                    return;

                var offsetPos = Vector3D.Transform(_lastPos, MatrixD.Invert(Parent.HudSpace.PlaneToWorld));
                var scalar = -offsetPos.Z / MyAPIGateway.Session.Camera.NearPlaneDistance;

                if (scalar < 0)
                {
                    Visible = false;
                    return;
                }
                else
                {
                    Visible = true;
                }

                Offset = new Vector2((float)(offsetPos.X / scalar) + Size.X / 2 + 30, (float)(offsetPos.Y / scalar));
            }
        }
    }
}
