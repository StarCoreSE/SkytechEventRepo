namespace AriUtils
{
    /// <summary>
    /// Generic singleton class
    /// </summary>
    /// <typeparam name="TThis">This class</typeparam>
    public abstract class SingletonBase<TThis> where TThis : SingletonBase<TThis>, new()
    {
        public static TThis I { get; private set; } = null;

        public static void Init()
        {
            I = new TThis();
            I._Init();
        }

        // god please marek get rid of the mod profiler i'll do anything
        public static void Update() => I._Update();

        public static void Unload()
        {
            I._Unload();
            I = null;
        }

        protected SingletonBase() { }
        protected abstract void _Init();
        protected abstract void _Update();
        protected abstract void _Unload();
    }
}
