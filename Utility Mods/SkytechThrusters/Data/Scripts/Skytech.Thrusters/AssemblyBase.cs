using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using Skytech.Thrusters.ModularAssemblies.Communication;

namespace Skytech.Thrusters
{
    /// <summary>
    /// Single assembly instance logic class. You should put your assembly logic here!
    /// </summary>
    internal abstract class AssemblyBase
    {
        private static ModularDefinitionApi ModularApi => ModularAssemblies.ModularDefinition.ModularApi;

        public int AssemblyId { get; private set; }
        public IMyCubeGrid Grid { get; private set; }

        private HashSet<IMyCubeBlock> _blocks = new HashSet<IMyCubeBlock>();

        protected AssemblyBase()
        {
            // fuckery to get around generic constructor limitations
        }

        public static TAssembly Create<TAssembly>(int assemblyId) where TAssembly : AssemblyBase, new()
        {
            TAssembly asm = new TAssembly
            {
                AssemblyId = assemblyId,
                Grid = ModularApi.GetAssemblyGrid(assemblyId)
            };

            asm.Init();
            return asm;
        }

        protected virtual void Init()
        {
        }

        /// <summary>
        /// Triggered once per game tick (60ups).
        /// </summary>
        public virtual void UpdateTick()
        {
        }

        /// <summary>
        /// Triggered when the assembly is closed (zero blocks contained).
        /// </summary>
        public virtual void Unload()
        {
        }

        /// <summary>
        /// Triggers whenever a new part is added to an assembly.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="isBasePart"></param>
        public virtual void OnPartAdd(IMyCubeBlock block, bool isBasePart)
        {
            _blocks.Add(block);
        }

        /// <summary>
        /// Triggers whenever a part is removed from an assembly.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="isBasePart"></param>
        public virtual void OnPartRemove(IMyCubeBlock block, bool isBasePart)
        {
            _blocks.Remove(block);
        }

        /// <summary>
        /// Triggers whenever a part is destroyed, just after OnPartRemove.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="isBasePart"></param>
        public virtual void OnPartDestroy(IMyCubeBlock block, bool isBasePart)
        {
        }
    }
}
