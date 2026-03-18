using System;
using System.Collections.Generic;
using System.Text;
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
                            _builder.Clear();
                        }
                    }
                }
            }

            _tick++;
        }

        private void _Display(Action<IMyCubeBlock, StringBuilder> info, IMyCubeBlock block)
        {
            info.Invoke(block, _builder);

            DebugDraw.AddGridPoint(block.Position, block.CubeGrid, Color.Red, 29/60f);

            // TODO display
            Log.Info("BlockInfo", $"Display info {_builder.ToString()}");
        }
    }
}
