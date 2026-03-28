using Skytech.Engines.Shared;
using Skytech.Engines.Shared.ModularAssemblies;
using Skytech.Engines.Shared.ModularAssemblies.Communication;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRageMath;

namespace Skytech.Engines
{
    internal class AssemblyManager<TAssembly> : IAssemblyManager
        where TAssembly : AssemblyBase, new()
    {
        // This class is a singleton.
        protected static AssemblyManager<TAssembly> I { get; private set; } = null;
        private static ModularDefinitionApi ModularApi => ModularDefinition.ModularApi;

        public static DefinitionDefs.ModularPhysicalDefinition Definition { get; private set; }
        private Dictionary<int, TAssembly> _assemblies = new Dictionary<int, TAssembly>();


        public static void Load(DefinitionDefs.ModularPhysicalDefinition definition)
        {
            if (I != null)
                return;
            I = new AssemblyManager<TAssembly>();
            Definition = definition;
            SharedMain.I.AssemblyManagers.Add(I);
            AssemblyBase.OnDefinitionInit<TAssembly>();
        }

        public void Unload(bool isSessionUnload = false)
        {
            foreach (var system in _assemblies.Values)
            {
                system.Unload();
            }

            if (isSessionUnload)
            {
                SharedMain.I.AssemblyManagers.Remove(I);
            }
            I = null;
        }

        public void Update()
        {
            foreach (var assembly in _assemblies.Values)
            {
                assembly.UpdateTick();
            }
        }

        public static bool TryGet(int assemblyId, out TAssembly asm) => I._assemblies.TryGetValue(assemblyId, out asm);

        public static bool TryGet(IMyCubeBlock block, out TAssembly asm)
        {
            asm = null;
            int id = ModularApi.GetContainingAssembly(block, Definition.Name);
            if (id == -1)
                return false;

            return I._assemblies.TryGetValue(id, out asm);
        }

        public static bool TryGet(IMyCubeGrid grid, Vector3I position, out TAssembly asm)
        {
            asm = null;

            IMyCubeBlock block = grid.GetCubeBlock(position)?.FatBlock;
            if (block == null)
                return false;

            int id = ModularApi.GetContainingAssembly(block, Definition.Name);
            if (id == -1)
                return false;

            return I._assemblies.TryGetValue(id, out asm);
        }

        public static void OnPartAdd(int assemblyId, IMyCubeBlock block, bool isBaseBlock)
        {
            if (I == null)
                return;

            // find assembly, and register new one if not present.
            // ID is unique per-session
            TAssembly assemblyBase;
            if (!I._assemblies.TryGetValue(assemblyId, out assemblyBase))
            {
                assemblyBase = AssemblyBase.Create<TAssembly>(assemblyId);
                I._assemblies.Add(assemblyId, assemblyBase);
                ModularApi.Log($"AssemblyManager created new assembly {assemblyId}.");
            }

            assemblyBase.OnPartAdd(block, isBaseBlock);
        }

        public static void OnPartRemove(int assemblyId, IMyCubeBlock block, bool isBaseBlock)
        {
            // find assembly, and skip if not present.
            TAssembly assemblyBase;
            if (I == null || !I._assemblies.TryGetValue(assemblyId, out assemblyBase))
                return;

            assemblyBase.OnPartRemove(block, isBaseBlock);
        }

        public static void OnPartDestroy(int assemblyId, IMyCubeBlock block, bool isBaseBlock)
        {
            // find assembly, and skip if not present.
            TAssembly assemblyBase;
            if (I == null || !I._assemblies.TryGetValue(assemblyId, out assemblyBase))
                return;

            assemblyBase.OnPartDestroy(block, isBaseBlock);
        }

        public static void OnAssemblyClose(int assemblyId)
        {
            // find assembly, and skip if not present.
            TAssembly assemblyBase;
            if (I == null || !I._assemblies.TryGetValue(assemblyId, out assemblyBase))
                return;

            assemblyBase.Unload();
            I._assemblies.Remove(assemblyId);
            ModularApi.Log($"AssemblyManager removed assembly {assemblyId}.");
        }
    }
}
