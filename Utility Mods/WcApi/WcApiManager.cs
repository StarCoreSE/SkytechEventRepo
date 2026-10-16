using AriUtils;
using System;

namespace CoreSystems.Api
{
    internal class WcApiManager : SingletonBase<WcApiManager>
    {
        public override int InitPriority => int.MinValue;
        public static WeaponCoreApi WcApi { get; private set; } = null;

        private static Action _onWcApiReady = () => Log.Info("WeaponCoreApi", "Ready.");
        public static event Action OnWcApiReady
        {
            add
            {
                if (WcApi != null && WcApi.IsReady)
                    value.Invoke();
                else
                    _onWcApiReady += value;
            }
            remove
            {
                _onWcApiReady -= value;
            }
        }

        public override void Init()
        {
            
        }

        public override void Update()
        {
            if (WcApi == null) // init late so that weapon defs have time to load in
            {
                Log.IncreaseIndent();

                try
                {
                    WcApi = new WeaponCoreApi();
                    WcApi.Load(_onWcApiReady, true);
                }
                catch (Exception ex)
                {
                    Log.Exception("WcApiManager", new Exception("Failed to load WeaponCoreApi!", ex));
                }

                Log.DecreaseIndent();
                Log.Info("WcApiManager", "Ready.");
            }
        }

        public override void Unload()
        {
            Log.IncreaseIndent();

            _onWcApiReady = null;

            Log.DecreaseIndent();
            Log.Info("WcApiManager", "Unloaded.");
            base.Unload();
        }
    }
}
