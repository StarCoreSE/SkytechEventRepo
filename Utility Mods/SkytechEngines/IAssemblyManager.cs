namespace Skytech.Engines
{
    internal interface IAssemblyManager
    {
        void Unload(bool isSessionUnload = false);
        void Update();
    }
}
