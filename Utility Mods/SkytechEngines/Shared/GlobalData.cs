
using System;

namespace AriUtils
{
    public static partial class GlobalData
    {
        public const string FriendlyModName = "SkyTech Engines";
        public static readonly string LastBuildTime = "$MDK_DATETIME$";

        private static Func<string, bool> KillswitchCheck => modIdFormatted => modIdFormatted.Contains("skytech") && modIdFormatted.Contains("engines");
    }
}
