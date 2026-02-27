using Skytech.Thrusters.Shared.ModularAssemblies.Communication;
using static Skytech.Thrusters.Shared.ModularAssemblies.Communication.DefinitionDefs;

// ReSharper disable once CheckNamespace
namespace Skytech.Thrusters.Shared.ModularAssemblies
{
    internal partial class ModularDefinition
    {
        internal static ModularDefinitionApi ModularApi = new ModularDefinitionApi();
        internal ModularDefinitionContainer Container = new ModularDefinitionContainer();

        internal void LoadDefinitions(params ModularPhysicalDefinition[] defs)
        {
            Container.PhysicalDefs = defs;
        }

        /// <summary>
        ///     Load all definitions for DefinitionSender
        /// </summary>
        /// <param name="baseDefs"></param>
        internal static ModularDefinitionContainer GetBaseDefinitions()
        {
            return new Skytech.Thrusters.Shared.ModularAssemblies.ModularDefinition().Container;
        }
    }
}