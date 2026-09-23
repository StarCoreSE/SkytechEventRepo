
using System;

namespace AriUtils
{
    public static partial class GlobalData
    {
        public const string FriendlyModName = "SkyTech Engines";
        public static readonly string LastBuildTime = "$MDK_DATETIME$";
        public const ushort ServerNetworkId = 15189;
        public const ushort DataNetworkId = 15188;
        public const ushort ClientNetworkId = 15187;

        private static Func<string, bool> KillswitchCheck => modIdFormatted => modIdFormatted.Contains("skytech") && modIdFormatted.Contains("engines");
    }
}
