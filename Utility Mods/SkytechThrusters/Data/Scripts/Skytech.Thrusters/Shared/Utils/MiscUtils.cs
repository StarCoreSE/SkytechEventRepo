using Sandbox.Game.Entities.Planet;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Skytech.Thrusters.Shared.Utils
{
    internal static class MiscUtils
    {
        public static IMyEntity RaycastEntityFromMatrix(MatrixD matrix)
        {
            var hits = new List<IHitInfo>();
            MyAPIGateway.Physics.CastRay(matrix.Translation + matrix.Forward, matrix.Translation + matrix.Forward * 5000, hits);
            foreach (var hit in hits)
            {
                var ent = hit.HitEntity;

                if (ent?.Physics != null)
                    return ent;
            }

            return null;
        }

        public static string RemoveChars(this string str, params char[] excluded)
        {
            return str == null ? null : string.Join("", str.Split(excluded));
        }

        public static void SafeChat(string sender, string message)
        {
            if (Environment.CurrentManagedThreadId == GlobalData.MainThreadId)
                MyAPIGateway.Utilities.ShowMessage(sender, message);
            else
                MyAPIGateway.Utilities.InvokeOnGameThread(() => MyAPIGateway.Utilities.ShowMessage(sender, message));
        }

        public static long NextLong(this Random rand)
        {
            var buf = new byte[8];
            rand.NextBytes(buf);

            return BitConverter.ToInt64(buf, 0);
        }

        public static long NextLong(this Random rand, long min, long max)
        {
            var buf = new byte[8];
            rand.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return (Math.Abs(longRand % (max - min)) + min);
        }

        public static float GetAtmosphereDensity(Vector3D position)
        {
            foreach (var planet in GlobalData.Planets)
            {
                if (planet.Closed || planet.MarkedForClose)
                    continue;

                if (Vector3D.DistanceSquared(position, planet.PositionComp.GetPosition()) >
                    planet.AtmosphereRadius * planet.AtmosphereRadius)
                    continue;
                return planet.GetAirDensity(position);
            }

            return 0;
        }

        public static void AddBlock<T>(IMyCubeBlock baseBlock, string subtypeName, Vector3I position) where T : MyObjectBuilder_CubeBlock, new()
        {
            var grid = baseBlock.CubeGrid;
            var existingBlock = grid.GetCubeBlock(position);
            if (existingBlock != null)
                grid.RemoveBlock(existingBlock);
            
            var nextBlockBuilder = new T
            {
                SubtypeName = subtypeName,
                Min = position,
                BlockOrientation = new MyBlockOrientation(Base6Directions.GetClosestDirection(position - baseBlock.Position), baseBlock.Orientation.Forward),
                ColorMaskHSV = baseBlock.Render.ColorMaskHsv,
                SkinSubtypeId = baseBlock.SlimBlock.SkinSubtypeId.String,
                Owner = baseBlock.OwnerId,
                EntityId = 0,
                ShareMode = MyOwnershipShareModeEnum.None
            };

            IMySlimBlock newBlock = grid.AddBlock(nextBlockBuilder, false);

            if (newBlock == null)
            {
                MyAPIGateway.Utilities.ShowNotification($"Failed to add {subtypeName}", 1000);
                return;
            }
            MyAPIGateway.Utilities.ShowNotification($"{subtypeName} added at {position}", 1000);
        }
    }
}
