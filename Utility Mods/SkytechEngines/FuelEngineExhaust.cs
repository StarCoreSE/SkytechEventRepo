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
            
        }

        public override void OnPartRemove(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartRemove(block, isBasePart);
            
        }

        protected override void BlockInfoCallback(IMyCubeBlock block, StringBuilder sb)
        {
            base.BlockInfoCallback(block, sb);
            sb.AppendLine($"hi line 3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3 :3");
            sb.AppendLine($"hi line 4 :3");
        }
    }
}
