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
        internal ModularPhysicalDefinition FuelEngineCarburettor => new ModularPhysicalDefinition
        {
            // Unique name of the definition.
            Name = "FuelEngineCarburettor",

            OnInit = AssemblyManager<FuelEngineCarburettor>.Load,

            // Triggers whenever a new part is added to an assembly.
            OnPartAdd = AssemblyManager<FuelEngineCarburettor>.OnPartAdd,

            // Triggers whenever a part is removed from an assembly.
            OnPartRemove = AssemblyManager<FuelEngineCarburettor>.OnPartRemove,

            // Triggers whenever a part is destroyed, just after OnPartRemove.
            OnPartDestroy = AssemblyManager<FuelEngineCarburettor>.OnPartDestroy,

            OnAssemblyClose = AssemblyManager<FuelEngineCarburettor>.OnAssemblyClose,

            // Optional - if this is set, an assembly will not be created until a baseblock exists.
            // 
            BaseBlockSubtype = "ST_T_Carburettor",

            // All SubtypeIds that can be part of this assembly.
            AllowedBlockSubtypes = new[]
            {
                "ST_T_Carburettor",
                "ST_T_Cylinder",
                "ST_T_InlineTurboLeft",
                "ST_T_InlineTurboRight",
                "ST_T_Supercharger",
                "ST_T_TurbochargerLeft",
                "ST_T_TurbochargerRight",
            },

            // Allowed connection directions & whitelists, measured in blocks.
            // If an allowed SubtypeId is not included here, connections are allowed on all sides.
            // If the connection type whitelist is empty, all allowed subtypes may connect on that side.
            AllowedConnections = new Dictionary<string, Dictionary<Vector3I, string[]>>
            {
                ["ST_T_Carburettor"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Up] = FuelEngineCarbCons.CarburettorSideConnections,
                    [Vector3I.Right] = FuelEngineCarbCons.CarburettorSideConnections,
                    [Vector3I.Down] = FuelEngineCarbCons.CarburettorSideConnections,
                    [Vector3I.Left] = FuelEngineCarbCons.CarburettorSideConnections,
                    [Vector3I.Forward] = FuelEngineCarbCons.CarburettorSideConnections,
                    [Vector3I.Backward] = FuelEngineCarbCons.CarburettorSideConnections,
                },
                ["ST_T_InlineTurboLeft"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Down] = FuelEngineCarbCons.IntoCarburettorConnections,
                },
                ["ST_T_InlineTurboRight"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Down] = FuelEngineCarbCons.IntoCarburettorConnections,
                },
                ["ST_T_Supercharger"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Up] = FuelEngineCarbCons.IntoCarburettorConnections,
                    [Vector3I.Right] = FuelEngineCarbCons.IntoCarburettorConnections,
                    [Vector3I.Down] = FuelEngineCarbCons.IntoCarburettorConnections,
                    [Vector3I.Left] = FuelEngineCarbCons.IntoCarburettorConnections,
                    [Vector3I.Forward] = FuelEngineCarbCons.IntoCarburettorConnections,
                    [Vector3I.Backward] = FuelEngineCarbCons.IntoCarburettorConnections,
                },
                ["ST_T_TurbochargerLeft"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Forward + Vector3I.Up] = FuelEngineCarbCons.IntoCarburettorConnections,
                },
                ["ST_T_TurbochargerRight"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Forward + Vector3I.Up] = FuelEngineCarbCons.IntoCarburettorConnections,
                },
            },
        };

        private static class FuelEngineCarbCons
        {
            public static readonly string[] IntoCarburettorConnections = { "ST_T_Carburettor", };
       
            public static readonly string[] CarburettorSideConnections = { "ST_T_Supercharger", "ST_T_TurbochargerLeft", "ST_T_TurbochargerRight", "ST_T_InlineTurboLeft", "ST_T_InlineTurboRight", };
        }
    }
}
