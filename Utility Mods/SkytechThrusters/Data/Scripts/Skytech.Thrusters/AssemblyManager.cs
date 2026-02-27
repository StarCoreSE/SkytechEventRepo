using System.Collections.Generic;
using VRage.Game.ModAPI;
using Skytech.Thrusters.ModularAssemblies.Communication;
namespace Skytech.Thrusters
{
    internal class AssemblyManager<TAssembly> : IAssemblyManager
        where TAssembly : AssemblyBase, new()
    {
        // This class is a singleton.
        protected static AssemblyManager<TAssembly> I { get; private set; } = null;
        private static ModularDefinitionApi ModularApi => ModularAssemblies.ModularDefinition.ModularApi;


        private Dictionary<int, AssemblyBase> _assemblies = new Dictionary<int, AssemblyBase>();


        public static void Load()
        {
            if (I != null)
                return;
            I = new AssemblyManager<TAssembly>();
        }

        public void Unload()
        {
            foreach (var system in _assemblies.Values)
            {
                system.Unload();
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

        public static void OnPartAdd(int assemblyId, IMyCubeBlock block, bool isBaseBlock)
        {
            if (I == null)
                return;

            // find assembly, and register new one if not present.
            // ID is unique per-session
            AssemblyBase assemblyBase;
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
            AssemblyBase assemblyBase;
            if (I == null || !I._assemblies.TryGetValue(assemblyId, out assemblyBase))
                return;

            assemblyBase.OnPartRemove(block, isBaseBlock);
        }

        public static void OnPartDestroy(int assemblyId, IMyCubeBlock block, bool isBaseBlock)
        {
            // find assembly, and skip if not present.
            AssemblyBase assemblyBase;
            if (I == null || !I._assemblies.TryGetValue(assemblyId, out assemblyBase))
                return;

            assemblyBase.OnPartDestroy(block, isBaseBlock);
        }

        public static void OnAssemblyClose(int assemblyId)
        {
            // find assembly, and skip if not present.
            AssemblyBase assemblyBase;
            if (I == null || !I._assemblies.TryGetValue(assemblyId, out assemblyBase))
                return;

            assemblyBase.Unload();
            I._assemblies.Remove(assemblyId);
            ModularApi.Log($"AssemblyManager removed assembly {assemblyId}.");
        }
    }
}
