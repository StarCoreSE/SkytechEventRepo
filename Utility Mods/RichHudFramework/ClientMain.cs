using AriUtils.HUD;
using RichHudFramework;

namespace AriUtils.Components
{
    partial class ClientMain
    {
        private ApiManager _apiManager = ApiManager.CreateWithOwner<ClientMain>();
        private BlockInfo _blockInfo = BlockInfo.CreateWithOwner<ClientMain>();
    }
}
