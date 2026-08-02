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
        internal ModularPhysicalDefinition Driveshaft => new ModularPhysicalDefinition
        {
            // Unique name of the definition.
            Name = "ST_T_Driveshaft",

            OnInit = AssemblyManager<Driveshaft>.Load,

            // Triggers whenever a new part is added to an assembly.
            OnPartAdd = AssemblyManager<Driveshaft>.OnPartAdd,

            // Triggers whenever a part is removed from an assembly.
            OnPartRemove = AssemblyManager<Driveshaft>.OnPartRemove,

            // Triggers whenever a part is destroyed, just after OnPartRemove.
            OnPartDestroy = AssemblyManager<Driveshaft>.OnPartDestroy,

            OnAssemblyClose = AssemblyManager<Driveshaft>.OnAssemblyClose,

            // Optional - if this is set, an assembly will not be created until a baseblock exists.
            // 
            BaseBlockSubtype = "",

            // All SubtypeIds that can be part of this assembly.
            AllowedBlockSubtypes = new[]
            {
                "ST_T_Driveshaft",
                "ST_T_DriveshaftCorner",
                "ST_T_Driveshaft6",
                "ST_T_CrankShaft",
            },

            // Allowed connection directions & whitelists, measured in blocks.
            // If an allowed SubtypeId is not included here, connections are allowed on all sides.
            // If the connection type whitelist is empty, all allowed subtypes may connect on that side.
            AllowedConnections = new Dictionary<string, Dictionary<Vector3I, string[]>>
            {
                ["ST_T_Driveshaft"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Forward] = Array.Empty<string>(),
                    [Vector3I.Backward] = Array.Empty<string>(),
                },
                ["ST_T_DriveshaftCorner"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Forward] = Array.Empty<string>(),
                    [Vector3I.Right] = Array.Empty<string>(),
                },
                // Driveshaft6 allows all connections on all sides
                ["ST_T_CrankShaft"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Forward] = Array.Empty<string>(),
                    [Vector3I.Backward] = Array.Empty<string>(),
                },
            },
        };
    }
}
