using Skytech.Engines.Shared.Exhaust;
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
        internal ModularPhysicalDefinition FuelEngineExhaust => new ModularPhysicalDefinition
        {
            // Unique name of the definition.
            Name = "FuelEngineExhaust",

            OnInit = AssemblyManager<FuelEngineExhaust>.Load,

            // Triggers whenever a new part is added to an assembly.
            OnPartAdd = AssemblyManager<FuelEngineExhaust>.OnPartAdd,

            // Triggers whenever a part is removed from an assembly.
            OnPartRemove = AssemblyManager<FuelEngineExhaust>.OnPartRemove,

            // Triggers whenever a part is destroyed, just after OnPartRemove.
            OnPartDestroy = AssemblyManager<FuelEngineExhaust>.OnPartDestroy,

            OnAssemblyClose = AssemblyManager<FuelEngineExhaust>.OnAssemblyClose,

            // Optional - if this is set, an assembly will not be created until a baseblock exists.
            // 
            BaseBlockSubtype = null,

            // All SubtypeIds that can be part of this assembly.
            AllowedBlockSubtypes = new[]
            {
                "ST_T_4WayPipe",
                "ST_T_5WayPipe",
                "ST_T_6WayPipe",
                "ST_T_CornerPipe",
                //"ST_T_Cylinder",
                "ST_T_HullPipe",
                //"ST_T_InlineTurboLeft",
                //"ST_T_InlineTurboRight",
                "ST_T_JunctionPipe",
                "ST_T_LJunctionPipe",
                "ST_T_StraightPipe",
                //"ST_T_TurbochargerLeft",
                //"ST_T_TurbochargerRight",
                "ST_T_XJunctionPipe",
            },

            // Allowed connection directions & whitelists, measured in blocks.
            // If an allowed SubtypeId is not included here, connections are allowed on all sides.
            // If the connection type whitelist is empty, all allowed subtypes may connect on that side.
            AllowedConnections = new Dictionary<string, Dictionary<Vector3I, string[]>>
            {
                ["ST_T_4WayPipe"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Right] = Array.Empty<string>(),
                    [Vector3I.Down] = Array.Empty<string>(),
                    [Vector3I.Left] = Array.Empty<string>(),
                    [Vector3I.Forward] = Array.Empty<string>(),
                },
                ["ST_T_5WayPipe"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Up] = Array.Empty<string>(),
                    [Vector3I.Right] = Array.Empty<string>(),
                    [Vector3I.Down] = Array.Empty<string>(),
                    [Vector3I.Left] = Array.Empty<string>(),
                    [Vector3I.Forward] = Array.Empty<string>(),
                },
                ["ST_T_6WayPipe"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Up] = Array.Empty<string>(),
                    [Vector3I.Right] = Array.Empty<string>(),
                    [Vector3I.Down] = Array.Empty<string>(),
                    [Vector3I.Left] = Array.Empty<string>(),
                    [Vector3I.Forward] = Array.Empty<string>(),
                    [Vector3I.Backward] = Array.Empty<string>(),
                },
                //["ST_T_Cylinder"] = new Dictionary<Vector3I, string[]>
                //{
                //    [Vector3I.Up] = FuelEngineExhCons.CylinderConnections,
                //    [Vector3I.Right] = FuelEngineExhCons.CylinderConnections,
                //    [Vector3I.Left] = FuelEngineExhCons.CylinderConnections,
                //    [Vector3I.Forward] = FuelEngineExhCons.CylinderConnections,
                //    [Vector3I.Backward] = FuelEngineExhCons.CylinderConnections,
                //},
                ["ST_T_CornerPipe"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Down] = Array.Empty<string>(),
                    [Vector3I.Forward] = Array.Empty<string>(),
                },
                ["ST_T_HullPipe"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Forward] = Array.Empty<string>(),
                    [Vector3I.Backward] = Array.Empty<string>(),
                },
                //["ST_T_InlineTurboLeft"] = new Dictionary<Vector3I, string[]>
                //{
                //    [Vector3I.Right + Vector3I.Backward] = Array.Empty<string>(),
                //    [Vector3I.Backward * 2] = Array.Empty<string>(),
                //},
                //["ST_T_InlineTurboRight"] = new Dictionary<Vector3I, string[]>
                //{
                //    [Vector3I.Left + Vector3I.Backward] = Array.Empty<string>(),
                //    [Vector3I.Backward * 2] = Array.Empty<string>(),
                //},
                ["ST_T_JunctionPipe"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Up] = Array.Empty<string>(),
                    [Vector3I.Right] = Array.Empty<string>(),
                    [Vector3I.Left] = Array.Empty<string>(),
                },
                ["ST_T_LJunctionPipe"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Right] = Array.Empty<string>(),
                    [Vector3I.Down] = Array.Empty<string>(),
                    [Vector3I.Forward] = Array.Empty<string>(),
                },
                ["ST_T_StraightPipe"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Forward] = Array.Empty<string>(),
                    [Vector3I.Backward] = Array.Empty<string>(),
                },
                //["ST_T_TurbochargerLeft"] = new Dictionary<Vector3I, string[]>
                //{
                //    [Vector3I.Right] = Array.Empty<string>(),
                //    [Vector3I.Forward] = Array.Empty<string>(),
                //},
                //["ST_T_TurbochargerRight"] = new Dictionary<Vector3I, string[]>
                //{
                //    [Vector3I.Left] = Array.Empty<string>(),
                //    [Vector3I.Forward] = Array.Empty<string>(),
                //},
                ["ST_T_XJunctionPipe"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Up] = Array.Empty<string>(),
                    [Vector3I.Right] = Array.Empty<string>(),
                    [Vector3I.Down] = Array.Empty<string>(),
                    [Vector3I.Left] = Array.Empty<string>(),
                },
            },
        };

        //private static class FuelEngineExhCons
        //{
        //    public static readonly string[] CylinderConnections =
        //    {
        //        "ST_T_4WayPipe",
        //        "ST_T_5WayPipe",
        //        "ST_T_6WayPipe",
        //        "ST_T_CornerPipe",
        //        "ST_T_HullPipe",
        //        "ST_T_InlineTurboLeft",
        //        "ST_T_InlineTurboRight",
        //        "ST_T_JunctionPipe",
        //        "ST_T_LJunctionPipe",
        //        "ST_T_StraightPipe",
        //        "ST_T_TurbochargerLeft",
        //        "ST_T_TurbochargerRight",
        //    };
        //}
    }
}
