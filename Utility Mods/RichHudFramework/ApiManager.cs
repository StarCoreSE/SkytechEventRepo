using System;
using AriUtils;
using RichHudFramework.Client;

namespace RichHudFramework
{
    internal class ApiManager : SingletonBase<ApiManager>
    {
        public override int InitPriority => int.MinValue;
        private static Action _onRichHudReady = () => Log.Info("RichHud", "Ready.");

        public override void Init()
        {
            Log.IncreaseIndent();

            try
            {
                RichHudClient.Init(ModContext.ModName, () => _onRichHudReady.Invoke(), null);
            }
            catch (Exception ex)
            {
                Log.Exception("ApiManager", new Exception("Failed to load RichHudClient!", ex));
            }

            Log.DecreaseIndent();
            Log.Info("ApiManager", "Ready.");
        }

        public override void Update()
        {
            
        }

        public override void Unload()
        {
            Log.IncreaseIndent();

            _onRichHudReady = null;

            Log.DecreaseIndent();
            Log.Info("ApiManager", "Unloaded.");
            base.Unload();
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
