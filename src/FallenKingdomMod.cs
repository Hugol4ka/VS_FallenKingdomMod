using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace FallenKingdom
{
    public class FallenKingdomMod : ModSystem
    {
        private ICoreClientAPI capi;
        private IClientNetworkChannel clientChannel;
        private GuiDialogFallenKingdom guiDialog;

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Client;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            this.capi = api;
            
            // Enregistrement du canal réseau comme spécifié par le Dev A
            clientChannel = api.Network.RegisterChannel("fallenkingdom")
                .RegisterMessageType<OpenFactionSelectionPacket>()
                .RegisterMessageType<FactionJoinPacket>()
                .RegisterMessageType<FactionJoinResponsePacket>()
                .SetMessageHandler<OpenFactionSelectionPacket>(OnOpenSelection)
                .SetMessageHandler<FactionJoinResponsePacket>(OnJoinResponse);

            // Ouvre automatiquement le menu quand le joueur a fini de charger le monde
            api.Event.LevelFinalize += OnLevelLoaded;
        }

        private void OnLevelLoaded()
        {
            if (guiDialog == null || !guiDialog.IsOpened())
            {
                guiDialog = new GuiDialogFallenKingdom(capi, clientChannel);
                guiDialog.TryOpen();
            }
        }

        private void OnOpenSelection(OpenFactionSelectionPacket packet)
        {
            if (guiDialog == null || !guiDialog.IsOpened())
            {
                guiDialog = new GuiDialogFallenKingdom(capi, clientChannel);
                guiDialog.TryOpen();
            }
        }

        private void OnJoinResponse(FactionJoinResponsePacket packet)
        {
            if (guiDialog != null && guiDialog.IsOpened())
            {
                guiDialog.HandleServerResponse(packet.Success, packet.ErrorMessage);
            }
        }
    }
}
