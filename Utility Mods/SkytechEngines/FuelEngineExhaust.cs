using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AriUtils.HUD;
using VRage.Game.ModAPI;

namespace Skytech.Engines
{
    internal class FuelEngineExhaust : AssemblyBase
    {
        public override void OnPartAdd(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartAdd(block, isBasePart);
            BlockInfo.Register(block, BlockInfoCallback);
        }

        public override void OnPartRemove(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartRemove(block, isBasePart);
            BlockInfo.Unregister(block, BlockInfoCallback);
        }

        private void BlockInfoCallback(IMyCubeBlock block, StringBuilder sb)
        {
            sb.AppendLine($"Exhaust Assembly {AssemblyId}");
            sb.AppendLine($"hi line 1 (aimed block is {block.DisplayNameText})");
            sb.AppendLine($"hi line 2 (current time is {DateTime.Now:T})");
            sb.AppendLine($"hi line 3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3");
            sb.AppendLine($"hi line 4 :3");
        }
    }
}
