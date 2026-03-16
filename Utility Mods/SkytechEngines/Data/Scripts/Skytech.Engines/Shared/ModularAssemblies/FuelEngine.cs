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
                "ST_T_Carburettor",
                "ST_T_CrankShaft",
                "ST_T_Cylinder",
                "ST_T_FuelEngineGenerator",
                "ST_T_Injector",
                "ST_T_InlineTurboLeft",
                "ST_T_InlineTurboRight",
                "ST_T_LargeRadiator",
                "ST_T_Radiator",
                "ST_T_Supercharger",
                "ST_T_TurbochargerLeft",
                "ST_T_TurbochargerRight",
            },

            // Allowed connection directions & whitelists, measured in blocks.
            // If an allowed SubtypeId is not included here, connections are allowed on all sides.
            // If the connection type whitelist is empty, all allowed subtypes may connect on that side.
            AllowedConnections = new Dictionary<string, Dictionary<Vector3I, string[]>>
            {
                ["ST_T_Adapter"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Up] = AdapterSideConnections,
                    [Vector3I.Right] = AdapterSideConnections,
                    [Vector3I.Down] = AdapterBottomConnections,
                    [Vector3I.Left] = AdapterSideConnections,
                    [Vector3I.Forward] = AdapterSideConnections,
                    [Vector3I.Backward] = AdapterSideConnections,
                },
                ["ST_T_Carburettor"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Up] = CarburettorTopConnections,
                    [Vector3I.Right] = CarburettorSideConnections,
                    [Vector3I.Down] = CarburettorSideConnections,
                    [Vector3I.Left] = CarburettorSideConnections,
                    [Vector3I.Forward] = CarburettorSideConnections,
                    [Vector3I.Backward] = CarburettorSideConnections,
                },
                ["ST_T_CrankShaft"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Up] = CrankshaftSideConnections,
                    [Vector3I.Right] = CrankshaftSideConnections,
                    [Vector3I.Down] = CrankshaftSideConnections,
                    [Vector3I.Left] = CrankshaftSideConnections,
                    [Vector3I.Forward] = CrankshaftConnections,
                    [Vector3I.Backward] = CrankshaftConnections,
                },
                ["ST_T_Cylinder"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Up] = CylinderSideConnections,
                    [Vector3I.Right] = CylinderSideConnections,
                    [Vector3I.Down] = CylinderBottomConnections,
                    [Vector3I.Left] = CylinderSideConnections,
                    [Vector3I.Forward] = CylinderSideConnections,
                    [Vector3I.Backward] = CylinderSideConnections,
                },
                ["ST_T_FuelEngineGenerator"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Forward] = CrankshaftConnections,
                },
                ["ST_T_Injector"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Down] = IntoCylinderConnections,
                    [Vector3I.Forward] = IntoCylinderConnections,
                },
                ["ST_T_InlineTurboLeft"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Down] = IntoCarburettorConnections,
                },
                ["ST_T_InlineTurboRight"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Down] = IntoCarburettorConnections,
                },
                ["ST_T_LargeRadiator"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Up * 2] = RadiatorSideConnections,
                    [Vector3I.Right * 2] = RadiatorSideConnections,
                    [Vector3I.Down * 2] = RadiatorSideConnections,
                    [Vector3I.Left * 2] = RadiatorSideConnections,
                    [Vector3I.Backward] = CrankshaftConnections,
                },
                ["ST_T_Radiator"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Up] = RadiatorSideConnections,
                    [Vector3I.Right] = RadiatorSideConnections,
                    [Vector3I.Down] = RadiatorSideConnections,
                    [Vector3I.Left] = RadiatorSideConnections,
                    [Vector3I.Backward] = CrankshaftConnections,
                },
                ["ST_T_Supercharger"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Up] = IntoCarburettorConnections,
                    [Vector3I.Right] = IntoCarburettorConnections,
                    [Vector3I.Down] = IntoCarburettorConnections,
                    [Vector3I.Left] = IntoCarburettorConnections,
                    [Vector3I.Forward] = IntoCarburettorConnections,
                    [Vector3I.Backward] = IntoCarburettorConnections,
                },
                ["ST_T_TurbochargerLeft"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Forward + Vector3I.Up] = IntoCarburettorConnections,
                    [Vector3I.Forward] = IntoCylinderConnections,
                },
                ["ST_T_TurbochargerRight"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Forward + Vector3I.Up] = IntoCarburettorConnections,
                    [Vector3I.Forward] = IntoCylinderConnections,
                },
            },
        };

        private static readonly string[] CrankshaftConnections = { "ST_T_CrankShaft", "ST_T_FuelEngineGenerator", "ST_T_Radiator", "ST_T_LargeRadiator", "ST_T_Cylinder", };
        private static readonly string[] CrankshaftSideConnections = { "ST_T_Adapter", "ST_T_Cylinder", "ST_T_Radiator", "ST_T_LargeRadiator", };

        private static readonly string[] AdapterSideConnections = { "ST_T_Cylinder", "ST_T_Radiator", "ST_T_LargeRadiator", };
        private static readonly string[] AdapterBottomConnections = { "ST_T_CrankShaft", };

        private static readonly string[] CylinderSideConnections = { "ST_T_Carburettor", "ST_T_Injector", }; // TODO exhaust
        private static readonly string[] CylinderBottomConnections = { "ST_T_CrankShaft", "ST_T_Adapter", "ST_T_FuelEngineGenerator" };

        private static readonly string[] IntoCylinderConnections = { "ST_T_Cylinder", };
        private static readonly string[] IntoCarburettorConnections = { "ST_T_Carburettor", };
       
        private static readonly string[] CarburettorSideConnections = { "ST_T_Cylinder", "ST_T_Supercharger", "ST_T_TurbochargerLeft", "ST_T_TurbochargerRight", "ST_T_InlineTurboLeft", "ST_T_InlineTurboRight", };
        private static readonly string[] CarburettorTopConnections = { "ST_T_Supercharger", "ST_T_TurbochargerLeft", "ST_T_TurbochargerRight", "ST_T_InlineTurboLeft", "ST_T_InlineTurboRight", };

        private static readonly string[] RadiatorSideConnections = { "ST_T_Radiator", "ST_T_LargeRadiator", };
    }
}
