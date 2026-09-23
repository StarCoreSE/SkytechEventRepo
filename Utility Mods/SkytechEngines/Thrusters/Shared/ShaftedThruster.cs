using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Linq;
using AriUtils;
using ModularAssemblies.Utils;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRageMath;

namespace Skytech.Thrusters.Shared
{
    internal class ShaftedThruster : AssemblyBase
    {      
        public override void OnPartAdd(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartAdd(block, isBasePart);
        }

        public override void OnPartRemove(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartRemove(block, isBasePart);
        }

        public override void Unload()
        {
            base.Unload();
        }

        public override void UpdateTick()
        {
            
        }
    }
}
