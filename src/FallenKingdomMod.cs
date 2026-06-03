using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace FallenKingdom
{
    public class FallenKingdomMod : ModSystem
    {
        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Client;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            // Création de la commande de test
            api.ChatCommands.Create("testgui")
                .WithDescription("Affiche le menu de test Fallen Kingdom")
                .HandleWith(_ => {
                    var gui = new GuiDialogFallenKingdom(api);
                    gui.TryOpen();
                    return TextCommandResult.Success();
                });
        }
    }
}