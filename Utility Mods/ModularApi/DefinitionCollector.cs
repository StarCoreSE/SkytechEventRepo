using static ModularAssemblies.DefinitionDefs;

namespace ModularAssemblies
{
    public partial class ModularDefinition
    {
        public static ModularDefinitionApi ModularApi = new ModularDefinitionApi();
        private ModularDefinitionContainer Container = new ModularDefinitionContainer();

        private void LoadDefinitions(params ModularPhysicalDefinition[] defs)
        {
            Container.PhysicalDefs = defs;
        }

        /// <summary>
        ///     Load all definitions for DefinitionSender
        /// </summary>
        /// <param name="baseDefs"></param>
        internal static ModularDefinitionContainer GetBaseDefinitions()
        {
            return new ModularAssemblies.ModularDefinition().Container;
        }
    }
}