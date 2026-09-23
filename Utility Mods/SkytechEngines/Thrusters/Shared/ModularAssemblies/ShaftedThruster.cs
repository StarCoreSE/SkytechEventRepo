using ModularAssemblies.Utils;
using System.Collections.Generic;
using AriUtils;
using VRageMath;
using static ModularAssemblies.DefinitionDefs;
using Skytech.Thrusters.Shared;

namespace ModularAssemblies
{
    /* Hey there modders!
     *
     * This file is a *template*. Make sure to keep up-to-date with the latest version, which can be found at https://github.com/StarCoreSE/Modular-Assemblies-Client-Mod-Template.
     *
     * If you're just here for the API, head on over to https://github.com/StarCoreSE/Modular-Assemblies/wiki/The-Modular-API for a (semi) comprehensive guide.
     *
     * This class uses internal logic. See also ExampleDefinition_WithLogic.cs.
     */
    public partial class ModularDefinition
    {
        // You can declare functions in here, and they are shared between all other ModularDefinition files.
        // However, for all but the simplest of assemblies it would be wise to have a separate utilities class.

        // This is the important bit.
        internal ModularPhysicalDefinition ShaftedThruster => new ModularPhysicalDefinition
        {
            // Unique name of the definition.
            Name = "ShaftedThruster",

            OnInit = AssemblyManager<ShaftedThruster>.Load,

            // Triggers whenever a new part is added to an assembly.
            OnPartAdd = AssemblyManager<ShaftedThruster>.OnPartAdd,

            // Triggers whenever a part is removed from an assembly.
            OnPartRemove = AssemblyManager<ShaftedThruster>.OnPartRemove,

            // Triggers whenever a part is destroyed, just after OnPartRemove.
            OnPartDestroy = AssemblyManager<ShaftedThruster>.OnPartDestroy,

            OnAssemblyClose = AssemblyManager<ShaftedThruster>.OnAssemblyClose,

            // Optional - if this is set, an assembly will not be created until a baseblock exists.
            // 
            //BaseBlockSubtype = "",

            // All SubtypeIds that can be part of this assembly.
            AllowedBlockSubtypes = MiscUtils.ArrayJoin(new[]
            {
                "Gimbal3x3Center",
                "LargeBlockSmallAtmosphericThrust",
                "LargeBlockLargeAtmosphericThrust",
            }, Driveshaft.AllowedBlockSubtypes),

            // Allowed connection directions & whitelists, measured in blocks.
            // If an allowed SubtypeId is not included here, connections are allowed on all sides.
            // If the connection type whitelist is empty, all allowed subtypes may connect on that side.
            AllowedConnections = MiscUtils.DictJoin(new Dictionary<string, Dictionary<Vector3I, string[]>>
            {
                ["Gimbal3x3Center"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Forward] = new[]
                    {
                        "LargeBlockSmallAtmosphericThrust",
                        "LargeBlockLargeAtmosphericThrust",
                    },
                    [Vector3I.Backward] = Driveshaft.AllowedBlockSubtypes
                },
                // TODO connections on thrusters
            }, MiscUtils.EditPartDict(Driveshaft.AllowedConnections, "Gimbal3x3Center")),
        };
    }
}
