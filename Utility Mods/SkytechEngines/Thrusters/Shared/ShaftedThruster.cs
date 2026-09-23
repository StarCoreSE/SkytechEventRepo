using ModularAssemblies.Utils;
using VRage.Game.ModAPI;

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
