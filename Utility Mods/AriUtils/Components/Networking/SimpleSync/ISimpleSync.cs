namespace AriUtils.Components.Networking.SimpleSync
{
    internal interface ISimpleSync
    {
        long SyncId { get; set; }
        ISyncableComponent Component { get; }
        void UpdateFromNetwork(byte[] data);
    }
}
