using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace FallenKingdom
{
    public class GuiDialogFallenKingdom : GuiDialog
    {
        public override string? ToggleKeyCombinationCode => null;

        private readonly int atlasCount;
        private readonly int horizonCount;
        private readonly int maxCapacity;
        private readonly IClientNetworkChannel clientChannel;

        public override bool CaptureAllInputs() => true;
        public override bool PrefersUngrabbedMouse => true;

        public GuiDialogFallenKingdom(ICoreClientAPI capi, int atlasCount, int horizonCount, int maxCapacity, IClientNetworkChannel clientChannel) : base(capi)
        {
            this.atlasCount = atlasCount;
            this.horizonCount = horizonCount;
            this.maxCapacity = maxCapacity;
            this.clientChannel = clientChannel;
            SetupDialog();
        }

        private void SetupDialog()
        {
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            string atlasLabel = atlasCount > 0 || maxCapacity > 0 ? $"EQUIPE ATLAS ({atlasCount}/{maxCapacity})" : "EQUIPE ATLAS";
            string horizonLabel = horizonCount > 0 || maxCapacity > 0 ? $"EQUIPE Nouvel-Horizon ({horizonCount}/{maxCapacity})" : "EQUIPE Nouvel-Horizon";

            SingleComposer = capi.Gui
                .CreateCompo("fallenkingdom", ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle))
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Fallen Kingdom - Inscription", OnTitleBarClose)
                .BeginChildElements()
                
                    // Espace réservé pour votre image PNG
                    .AddInset(ElementBounds.Fixed(10, 10, 620, 160))
                    
                    // Présentation
                    .AddRichtext("<font size=\"22\" color=\"#c8953c\"><strong>⚔️ FALLEN KINGDOM — SAISON 1</strong></font><br/>Bienvenue à vous pour la saison 1 du FALLEN KINGDOM.<br/>Préparez-vous à défendre notre civilisation et affronter la communauté adverse.<br/><br/><font color=\"#4ab4c8\">Nos missions :</font> Préparer nos défenses • Farmer • Forger • Coudre • Construire • Piéger • Explorer • Attaquer dans le fairplay.<br/><br/><font color=\"#c8953c\">Objectif :</font> Faire survivre et prospérer notre civilisation face à l’adversaire.", CairoFont.WhiteSmallText(), ElementBounds.Fixed(20, 20, 600, 140), "e_text_top")
                    
                    // Inset Règles
                    .AddInset(ElementBounds.Fixed(10, 180, 620, 410))
                    
                    // Texte Règles
                    .AddRichtext("<font size=\"18\" color=\"#c8953c\"><strong>Règlement à lire impérativement !</strong></font><br/><br/><font size=\"16\" color=\"#4ab4c8\"><strong>🕵️ META GAMING & ESPIONNAGE</strong></font><br/><font color=\"#a0a0a0\">• Le meta gaming est strictement interdit.<br/>• Interdiction de transmettre des informations à l’équipe adverse.<br/>• Fuite de renseignements et espionnage hors jeu interdits.<br/>• Utilisation d’informations Discord externes interdite.<br/>• Toutes les informations doivent rester au sein de l’équipe concernée.</font><br/><br/><font size=\"16\" color=\"#4ab4c8\"><strong>🪤 PIÈGES</strong></font><br/><font color=\"#a0a0a0\">• Tous les pièges sont autorisés dans la zone de votre civilisation.<br/>• Ils peuvent défendre villages, routes et fortifications.<br/>• Les pièges abusifs exploitant des bugs sont strictement interdits.</font><br/><br/><font size=\"16\" color=\"#4ab4c8\"><strong>⚔️ RÈGLES — RAID</strong></font><br/><font color=\"#a0a0a0\">• L’escalade des enceintes doit obligatoirement se faire avec des échelles.<br/>• L’utilisation de blocs (tours de terre) pour grimper est interdite.<br/>• Le planeur est autorisé.<br/>• Destruction de base : uniquement avec des bombes (pioche interdite).<br/>• Destruction des portes/lockers : autorisée avec des armes.<br/>• Les raids doivent respecter l’esprit immersif de l’événement.</font><br/><br/><font size=\"16\" color=\"#4ab4c8\"><strong>💀 RÈGLES — STUFF</strong></font><br/><font color=\"#a0a0a0\">• Toute mort entraîne une perte définitive du stuff.<br/>• Aucun remboursement d’équipement ne sera effectué.<br/>• Le loot des ennemis vaincus est autorisé.<br/>• Chaque combat possède un risque réel.</font><br/><br/><font size=\"16\" color=\"#4ab4c8\"><strong>🏗️ RÈGLES — CONSTRUCTIONS</strong></font><br/><font color=\"#a0a0a0\">• Les constructions doivent rester cohérentes, réalistes et accessibles.<br/>• Chaque structure doit être accessible via porte, trappe ou échelle.<br/>• Les constructions de type \"parcours\" ou bases irréalistes sont interdites.<br/>• Les bases volontairement imprenables seront supprimées ou sanctionnées.<br/>• Le réalisme et l’immersion priment sur l’optimisation abusive.</font>", CairoFont.WhiteSmallText(), ElementBounds.Fixed(20, 190, 580, 390), "e_text_rules")
                    
                    // Scrollbar et Switch d'acceptation
                    .AddVerticalScrollbar(OnScroll, ElementBounds.Fixed(612, 182, 14, 406), "e_scroll")
                    .AddSwitch(OnAcceptToggle, ElementBounds.Fixed(20, 605, 30, 30), "e_accept_switch")
                    .AddRichtext("<font color=\"#d0c8b8\">J'ai lu et j'accepte le règlement. Je comprends que mon choix d'équipe est </font><font color=\"#c8953c\"><strong>définitif</strong></font>.", CairoFont.WhiteSmallText(), ElementBounds.Fixed(60, 611, 550, 30), "e_accept_text")
                    
                    // Boutons Équipes
                    .AddSmallButton(atlasLabel, OnJoinAtlas, ElementBounds.Fixed(90, 650, 200, 40), EnumButtonStyle.Normal, "btn_atlas")
                    .AddSmallButton(horizonLabel, OnJoinHorizon, ElementBounds.Fixed(350, 650, 200, 40), EnumButtonStyle.Normal, "btn_horizon")
                
                .EndChildElements()
                .Compose();

            // Les boutons sont bloqués à l'ouverture
            SingleComposer.GetButton("btn_atlas").Enabled = false;
            SingleComposer.GetButton("btn_horizon").Enabled = false;
        }

        private void OnAcceptToggle(bool on)
        {
            // Débloque les boutons si la case est cochée
            SingleComposer.GetButton("btn_atlas").Enabled = on;
            SingleComposer.GetButton("btn_horizon").Enabled = on;
        }

        private void SetButtonsEnabled(bool enabled)
        {
            SingleComposer.GetButton("btn_atlas").Enabled = enabled;
            SingleComposer.GetButton("btn_horizon").Enabled = enabled;
        }

        private bool OnJoinAtlas()
        {
            if (clientChannel != null)
            {
                clientChannel.SendPacket(new FactionJoinPacket { FactionId = 1 });
                SetButtonsEnabled(false); // Désactive temporairement en attendant la réponse
            }
            else
            {
                capi.ShowChatMessage("TEST : ATLAS REJOINT !");
                TryClose();
            }
            return true;
        }

        private bool OnJoinHorizon()
        {
            if (clientChannel != null)
            {
                clientChannel.SendPacket(new FactionJoinPacket { FactionId = 2 });
                SetButtonsEnabled(false); // Désactive temporairement en attendant la réponse
            }
            else
            {
                capi.ShowChatMessage("TEST : NOUVEL-HORIZON REJOINT !");
                TryClose();
            }
            return true;
        }

        public void OnServerResponse(FactionJoinResponsePacket packet)
        {
            if (packet.Success)
            {
                capi.ShowChatMessage("Inscription validée avec succès !");
                TryClose();
            }
            else
            {
                capi.TriggerIngameError(this, "faction_error", packet.ErrorMessage);
                // Réactive les boutons pour permettre au joueur de réessayer
                SetButtonsEnabled(true);
            }
        }

        private void OnScroll(float value) { }
        private void OnTitleBarClose() { TryClose(); }
    }
}