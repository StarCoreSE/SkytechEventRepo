using System;
using AriUtils;
using RichHudFramework.Client;
using VRage.Game.ModAPI;

namespace RichHudFramework
{
    internal static class ApiManager
    {
        private static Action _onRichHudReady = () => Log.Info("RichHud", "Ready.");

        public static void Init(IMyModContext context)
        {
            Log.IncreaseIndent();

            try
            {
                RichHudClient.Init(context.ModName, () => _onRichHudReady.Invoke(), null);
            }
            catch (Exception ex)
            {
                Log.Exception("ApiManager", new Exception("Failed to load RichHudClient!", ex));
            }

            Log.DecreaseIndent();
            Log.Info("ApiManager", "Ready.");
        }

        public static void Unload()
        {
            Log.IncreaseIndent();

            _onRichHudReady = null;

            Log.DecreaseIndent();
            Log.Info("ApiManager", "Unloaded.");
        }

        /// <summary>
        /// Registers an action to invoke when the API is ready, or calls it immediately if ready.
        /// </summary>
        /// <param name="action"></param>
        public static void RichHudOnLoadRegisterOrInvoke(Action action)
        {
            if (RichHudClient.Registered)
                action.Invoke();
            else
                _onRichHudReady += action;
        }
    }
}
