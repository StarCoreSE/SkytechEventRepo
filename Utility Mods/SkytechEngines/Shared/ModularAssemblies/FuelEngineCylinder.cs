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
        internal ModularPhysicalDefinition FuelEngineCylinder => new ModularPhysicalDefinition
        {
            // Unique name of the definition.
            Name = "FuelEngineCylinder",

            OnInit = AssemblyManager<FuelEngineCylinder>.Load,

            // Triggers whenever a new part is added to an assembly.
            OnPartAdd = AssemblyManager<FuelEngineCylinder>.OnPartAdd,

            // Triggers whenever a part is removed from an assembly.
            OnPartRemove = AssemblyManager<FuelEngineCylinder>.OnPartRemove,

            // Triggers whenever a part is destroyed, just after OnPartRemove.
            OnPartDestroy = AssemblyManager<FuelEngineCylinder>.OnPartDestroy,

            OnAssemblyClose = AssemblyManager<FuelEngineCylinder>.OnAssemblyClose,

            // Optional - if this is set, an assembly will not be created until a baseblock exists.
            // 
            BaseBlockSubtype = "ST_T_Cylinder",

            // All SubtypeIds that can be part of this assembly.
            AllowedBlockSubtypes = new[]
            {
                "ST_T_Carburettor",
                "ST_T_Cylinder",
                "ST_T_Injector",
            },

            // Allowed connection directions & whitelists, measured in blocks.
            // If an allowed SubtypeId is not included here, connections are allowed on all sides.
            // If the connection type whitelist is empty, all allowed subtypes may connect on that side.
            AllowedConnections = new Dictionary<string, Dictionary<Vector3I, string[]>>
            {
                ["ST_T_Carburettor"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Right] = FuelEngineCylCons.IntoCylinderConnections,
                    [Vector3I.Down] = FuelEngineCylCons.IntoCylinderConnections,
                    [Vector3I.Left] = FuelEngineCylCons.IntoCylinderConnections,
                    [Vector3I.Forward] = FuelEngineCylCons.IntoCylinderConnections,
                    [Vector3I.Backward] = FuelEngineCylCons.IntoCylinderConnections,
                },
                ["ST_T_Cylinder"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Up] = FuelEngineCylCons.CylinderSideConnections,
                    [Vector3I.Right] = FuelEngineCylCons.CylinderSideConnections,
                    [Vector3I.Left] = FuelEngineCylCons.CylinderSideConnections,
                    [Vector3I.Forward] = FuelEngineCylCons.CylinderSideConnections,
                    [Vector3I.Backward] = FuelEngineCylCons.CylinderSideConnections,
                },
                ["ST_T_Injector"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Down] = FuelEngineCylCons.IntoCylinderConnections,
                    [Vector3I.Forward] = FuelEngineCylCons.IntoCylinderConnections,
                },
            },
        };

        private static class FuelEngineCylCons
        {
            public static readonly string[] CylinderSideConnections = { "ST_T_Carburettor", "ST_T_Injector", }; // TODO exhaust

            public static readonly string[] IntoCylinderConnections = { "ST_T_Cylinder", };
        }
    }
}
