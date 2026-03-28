using AriUtils.HUD;
using Sandbox.Game.Entities;
using Skytech.Engines.Shared.ModularAssemblies;
using Skytech.Engines.Shared.ModularAssemblies.Communication;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;

namespace Skytech.Engines
{
    /// <summary>
    /// Single assembly instance logic class. You should put your assembly logic here!
    /// </summary>
    internal abstract class AssemblyBase
    {
        protected static ModularDefinitionApi ModularApi => ModularDefinition.ModularApi;

        public int AssemblyId { get; private set; }
        public IMyCubeGrid Grid { get; private set; }
        public MyCubeGrid MyGrid { get; private set; }
        public DefinitionDefs.ModularPhysicalDefinition Definition { get; private set; }

        public IMyCubeBlock RootBlock { get; private set; } = null;
        public long RootId { get; private set; } = -1;
        public Action OnClose = null;

        protected HashSet<IMyCubeBlock> Blocks = new HashSet<IMyCubeBlock>();

        protected AssemblyBase()
        {
            // fuckery to get around generic constructor limitations
        }

        public static TAssembly Create<TAssembly>(int assemblyId) where TAssembly : AssemblyBase, new()
        {
            TAssembly asm = new TAssembly
            {
                AssemblyId = assemblyId,
                Grid = ModularApi.GetAssemblyGrid(assemblyId),
                Definition = AssemblyManager<TAssembly>.Definition,
            };
            asm.MyGrid = (MyCubeGrid) asm.Grid;

            asm.Init();
            return asm;
        }

        public static void OnDefinitionInit<TAssembly>() where TAssembly : AssemblyBase, new()
        {
            new TAssembly().DefinitionInit();
        }

        protected virtual void DefinitionInit()
        {
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
            OnClose?.Invoke();

            foreach (var block in Blocks)
            {
                BlockInfo.Unregister(block, BlockInfoCallback);
            }
        }

        /// <summary>
        /// Triggers whenever a new part is added to an assembly.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="isBasePart"></param>
        public virtual void OnPartAdd(IMyCubeBlock block, bool isBasePart)
        {
            Blocks.Add(block);
            if (RootBlock == null)
            {
                RootBlock = block;
                RootId = RootBlock.EntityId ^ GetType().Name.GetHashCode();
            }
            BlockInfo.Register(block, BlockInfoCallback);
        }

        /// <summary>
        /// Triggers whenever a part is removed from an assembly.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="isBasePart"></param>
        public virtual void OnPartRemove(IMyCubeBlock block, bool isBasePart)
        {
            Blocks.Remove(block);
            BlockInfo.Unregister(block, BlockInfoCallback);
        }

        /// <summary>
        /// Triggers whenever a part is destroyed, just after OnPartRemove.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="isBasePart"></param>
        public virtual void OnPartDestroy(IMyCubeBlock block, bool isBasePart)
        {
        }

        protected virtual void BlockInfoCallback(IMyCubeBlock block, StringBuilder sb)
        {
            sb.AppendLine($"{GetType().Name}: {Blocks.Count} parts");
        }
    }
}
