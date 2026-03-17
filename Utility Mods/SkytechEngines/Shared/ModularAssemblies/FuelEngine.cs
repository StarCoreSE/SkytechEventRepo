using System;
using System.Collections.Generic;
using VRageMath;
using static Skytech.Engines.Shared.ModularAssemblies.Communication.DefinitionDefs;

namespace Skytech.Engines.Shared.ModularAssemblies
{
    /* Hey there modders!
     *
     * This file is a *template*. Make sure to keep up-to-date with the latest version, which can be found at https://github.com/StarCoreSE/Modular-Assemblies-Client-Mod-Template.
     *
     * If you're just here for the API, head on over to https://github.com/StarCoreSE/Modular-Assemblies/wiki/The-Modular-API for a (semi) comprehensive guide.
     *
     * This class uses internal logic. See also ExampleDefinition_WithLogic.cs.
     */
    internal partial class ModularDefinition
    {
        // You can declare functions in here, and they are shared between all other ModularDefinition files.
        // However, for all but the simplest of assemblies it would be wise to have a separate utilities class.

        // This is the important bit.
        internal ModularPhysicalDefinition FuelEngine => new ModularPhysicalDefinition
        {
            // Unique name of the definition.
            Name = "FuelEngine",

            OnInit = AssemblyManager<FuelEngine>.Load,

            // Triggers whenever a new part is added to an assembly.
            OnPartAdd = AssemblyManager<FuelEngine>.OnPartAdd,

            // Triggers whenever a part is removed from an assembly.
            OnPartRemove = AssemblyManager<FuelEngine>.OnPartRemove,

            // Triggers whenever a part is destroyed, just after OnPartRemove.
            OnPartDestroy = AssemblyManager<FuelEngine>.OnPartDestroy,

            OnAssemblyClose = AssemblyManager<FuelEngine>.OnAssemblyClose,

            // Optional - if this is set, an assembly will not be created until a baseblock exists.
            // 
            BaseBlockSubtype = "ST_T_FuelEngineGenerator",

            // All SubtypeIds that can be part of this assembly.
            AllowedBlockSubtypes = new[]
            {
                "ST_T_Adapter",
                "ST_T_CrankShaft",
                "ST_T_Cylinder",
                "ST_T_FuelEngineGenerator",
                "ST_T_LargeRadiator",
                "ST_T_Radiator",
            },

            // Allowed connection directions & whitelists, measured in blocks.
            // If an allowed SubtypeId is not included here, connections are allowed on all sides.
            // If the connection type whitelist is empty, all allowed subtypes may connect on that side.
            AllowedConnections = new Dictionary<string, Dictionary<Vector3I, string[]>>
            {
                ["ST_T_Adapter"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Up] = FuelEngineCons.AdapterSideConnections,
                    [Vector3I.Right] = FuelEngineCons.AdapterSideConnections,
                    [Vector3I.Down] = FuelEngineCons.AdapterBottomConnections,
                    [Vector3I.Left] = FuelEngineCons.AdapterSideConnections,
                    [Vector3I.Forward] = FuelEngineCons.AdapterSideConnections,
                    [Vector3I.Backward] = FuelEngineCons.AdapterSideConnections,
                },
                ["ST_T_CrankShaft"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Up] = FuelEngineCons.CrankshaftSideConnections,
                    [Vector3I.Right] = FuelEngineCons.CrankshaftSideConnections,
                    [Vector3I.Down] = FuelEngineCons.CrankshaftSideConnections,
                    [Vector3I.Left] = FuelEngineCons.CrankshaftSideConnections,
                    [Vector3I.Forward] = FuelEngineCons.CrankshaftConnections,
                    [Vector3I.Backward] = FuelEngineCons.CrankshaftConnections,
                },
                ["ST_T_Cylinder"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Down] = FuelEngineCons.CylinderBottomConnections,
                },
                ["ST_T_FuelEngineGenerator"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Forward] = FuelEngineCons.CrankshaftConnections,
                },
                ["ST_T_LargeRadiator"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Up * 2] = FuelEngineCons.RadiatorSideConnections,
                    [Vector3I.Right * 2] = FuelEngineCons.RadiatorSideConnections,
                    [Vector3I.Down * 2] = FuelEngineCons.RadiatorSideConnections,
                    [Vector3I.Left * 2] = FuelEngineCons.RadiatorSideConnections,
                    [Vector3I.Backward] = FuelEngineCons.CrankshaftConnections,
                },
                ["ST_T_Radiator"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Up] = FuelEngineCons.RadiatorSideConnections,
                    [Vector3I.Right] = FuelEngineCons.RadiatorSideConnections,
                    [Vector3I.Down] = FuelEngineCons.RadiatorSideConnections,
                    [Vector3I.Left] = FuelEngineCons.RadiatorSideConnections,
                    [Vector3I.Backward] = FuelEngineCons.CrankshaftConnections,
                },
            },
        };

        private static class FuelEngineCons
        {
            public static readonly string[] CrankshaftConnections = { "ST_T_CrankShaft", "ST_T_FuelEngineGenerator", "ST_T_Radiator", "ST_T_LargeRadiator", "ST_T_Cylinder", };
            public static readonly string[] CrankshaftSideConnections = { "ST_T_Adapter", "ST_T_Cylinder", "ST_T_Radiator", "ST_T_LargeRadiator", };

            public static readonly string[] AdapterSideConnections = { "ST_T_Cylinder", "ST_T_Radiator", "ST_T_LargeRadiator", };
            public static readonly string[] AdapterBottomConnections = { "ST_T_CrankShaft", };

            public static readonly string[] CylinderBottomConnections = { "ST_T_CrankShaft", "ST_T_Adapter", "ST_T_FuelEngineGenerator" };

            public static readonly string[] RadiatorSideConnections = { "ST_T_Radiator", "ST_T_LargeRadiator", };
        }
    }
}
