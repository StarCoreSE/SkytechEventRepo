using AriUtils.HUD;
using RichHudFramework;

namespace AriUtils.Components
{
    partial class ClientMain
    {
        private ApiManager _apiManager = ApiManager.Create<ClientMain>();
        private BlockInfo _blockInfo = BlockInfo.Create<ClientMain>();
    }
}
