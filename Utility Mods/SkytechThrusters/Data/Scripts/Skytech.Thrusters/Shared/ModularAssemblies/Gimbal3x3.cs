using System;
using System.Collections.Generic;
using VRageMath;
using static Skytech.Thrusters.Shared.ModularAssemblies.Communication.DefinitionDefs;

namespace Skytech.Thrusters.Shared.ModularAssemblies
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
        internal ModularPhysicalDefinition Gimbal3x3 => new ModularPhysicalDefinition
        {
            // Unique name of the definition.
            Name = "Gimbal3x3",

            OnInit = AssemblyManager<Gimbal3x3>.Load,

            // Triggers whenever a new part is added to an assembly.
            OnPartAdd = AssemblyManager<Gimbal3x3>.OnPartAdd,

            // Triggers whenever a part is removed from an assembly.
            OnPartRemove = AssemblyManager<Gimbal3x3>.OnPartRemove,

            // Triggers whenever a part is destroyed, just after OnPartRemove.
            OnPartDestroy = AssemblyManager<Gimbal3x3>.OnPartDestroy,

            OnAssemblyClose = AssemblyManager<Gimbal3x3>.OnAssemblyClose,

            // Optional - if this is set, an assembly will not be created until a baseblock exists.
            // 
            BaseBlockSubtype = "Gimbal3x3Center",

            // All SubtypeIds that can be part of this assembly.
            AllowedBlockSubtypes = new[]
            {
                "Gimbal3x3Center",
                "GimbalThrustPart",
                "LargeBlockSmallAtmosphericThrust",
            },

            // Allowed connection directions & whitelists, measured in blocks.
            // If an allowed SubtypeId is not included here, connections are allowed on all sides.
            // If the connection type whitelist is empty, all allowed subtypes may connect on that side.
            AllowedConnections = new Dictionary<string, Dictionary<Vector3I, string[]>>
            {
                ["Gimbal3x3Center"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Up] = CenterConnections,
                    [Vector3I.Right] = CenterConnections,
                    [Vector3I.Down] = CenterConnections,
                    [Vector3I.Left] = CenterConnections,
                    [Vector3I.Forward] = new [] { "LargeBlockSmallAtmosphericThrust" },
                },
                ["GimbalThrustPart"] = new Dictionary<Vector3I, string[]>
                {
                    [Vector3I.Backward] = new[] { "Gimbal3x3Center" },
                }
            },
        };

        private static readonly string[] CenterConnections = { "GimbalThrustPart" };
    }
}
