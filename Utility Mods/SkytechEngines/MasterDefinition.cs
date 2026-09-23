// ReSharper disable once CheckNamespace
namespace ModularAssemblies
{
    // turns out whoever wrote the CoreSystems definition handler is REALLY SMART. hats off to you
    public partial class ModularDefinition
    {
        internal ModularDefinition()
        {
            // it's just like assemblycore, insert definitions here

            LoadDefinitions
            (
                // SkyTech Engines
                FuelEngine, FuelEngineCylinder, FuelEngineCarburettor, FuelEngineExhaust, Driveshaft,
                // SkyTech Thrusters
                Gimbal3x3
            );
        }
    }
}