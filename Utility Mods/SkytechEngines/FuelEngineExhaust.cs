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

        }
    }
}
