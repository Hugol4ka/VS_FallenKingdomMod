using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace FallenKingdom
{
    public class GuiDialogFallenKingdom : GuiDialog
    {
        public override string ToggleKeyCombinationCode => null!;
        
        // Bloque la souris et les mouvements du joueur selon les specs du Dev A
        public override bool PrefersUngrabbedMouse => true;
        public override bool CaptureAllInputs() => true;

        ElementBounds textBounds;
        ElementBounds clipBounds;
        
        private IClientNetworkChannel clientChannel;
        private long timeoutListenerId = 0;

        // On a modifié le constructeur pour accepter le canal réseau
        public GuiDialogFallenKingdom(ICoreClientAPI capi, IClientNetworkChannel channel) : base(capi)
        {
            this.clientChannel = channel;
            SetupDialog();
        }

        private void SetupDialog()
        {
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            clipBounds = ElementBounds.Fixed(15, 200, 590, 400); 
            textBounds = ElementBounds.Fixed(0, 0, 570, 10); 

            SingleComposer = capi.Gui
                .CreateCompo("fallenkingdom", ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle))
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Fallen Kingdom - Inscription", OnTitleBarClose)
                .BeginChildElements(bgBounds)
                
                    .AddInset(ElementBounds.Fixed(10, 10, 620, 180)) // Inset remonté
                    
                    .AddRichtext("<font size=\"22\" color=\"#c8953c\"><strong>[ FALLEN KINGDOM — SAISON 1 ]</strong></font><br/>Bienvenue à vous pour la saison 1 du FALLEN KINGDOM.<br/>Préparez-vous à défendre notre civilisation et affronter la communauté adverse.<br/><br/><font color=\"#4ab4c8\">Nos missions :</font> Préparer nos défenses • Farmer • Forger • Coudre • Construire • Piéger • Explorer • Attaquer dans le fairplay.<br/><br/><font color=\"#c8953c\">Objectif :</font> Faire survivre et prospérer notre civilisation face à l’adversaire.", CairoFont.WhiteSmallText(), ElementBounds.Fixed(20, 20, 600, 160), "e_text_top")
                    
                    .AddInset(ElementBounds.Fixed(10, 195, 620, 410))
                    
                    .BeginClip(clipBounds)
                        .AddRichtext("<font size=\"18\" color=\"#c8953c\"><strong>Règlement à lire impérativement !</strong></font><br/><br/><font size=\"16\" color=\"#4ab4c8\"><strong>[ META GAMING & ESPIONNAGE ]</strong></font><br/><font color=\"#a0a0a0\">• Le meta gaming est strictement interdit.<br/>• Interdiction de transmettre des informations à l’équipe adverse.<br/>• Fuite de renseignements et espionnage hors jeu interdits.<br/>• Utilisation d’informations Discord externes interdite.<br/>• Toutes les informations doivent rester au sein de l’équipe concernée.</font><br/><br/><font size=\"16\" color=\"#4ab4c8\"><strong>[ PIÈGES ]</strong></font><br/><font color=\"#a0a0a0\">• Tous les pièges sont autorisés dans la zone de votre civilisation.<br/>• Ils peuvent défendre villages, routes et fortifications.<br/>• Les pièges abusifs exploitant des bugs sont strictement interdits.</font><br/><br/><font size=\"16\" color=\"#4ab4c8\"><strong>[ RÈGLES — RAID ]</strong></font><br/><font color=\"#a0a0a0\">• L’escalade des enceintes doit obligatoirement se faire avec des échelles.<br/>• L’utilisation de blocs (tours de terre) pour grimper est interdite.<br/>• Le planeur est autorisé.<br/>• Destruction de base : uniquement avec des bombes (pioche interdite).<br/>• Destruction des portes/lockers : autorisée avec des armes.<br/>• Les raids doivent respecter l’esprit immersif de l’événement.</font><br/><br/><font size=\"16\" color=\"#4ab4c8\"><strong>[ RÈGLES — STUFF ]</strong></font><br/><font color=\"#a0a0a0\">• Toute mort entraîne une perte définitive du stuff.<br/>• Aucun remboursement d’équipement ne sera effectué.<br/>• Le loot des ennemis vaincus est autorisé.<br/>• Chaque combat possède un risque réel.</font><br/><br/><font size=\"16\" color=\"#4ab4c8\"><strong>[ RÈGLES — CONSTRUCTIONS ]</strong></font><br/><font color=\"#a0a0a0\">• Les constructions doivent rester cohérentes, réalistes et accessibles.<br/>• Chaque structure doit être accessible via porte, trappe ou échelle.<br/>• Les constructions de type \"parcours\" ou bases irréalistes sont interdites.<br/>• Les bases volontairement imprenables seront supprimées ou sanctionnées.<br/>• Le réalisme et l’immersion priment sur l’optimisation abusive.</font>", CairoFont.WhiteSmallText(), textBounds, "e_text_rules")
                    .EndClip()
                    
                    .AddVerticalScrollbar(OnScroll, ElementBounds.Fixed(612, 197, 14, 406), "e_scroll")
                    
                    .AddSwitch(OnAcceptToggle, ElementBounds.Fixed(20, 620, 30, 30), "e_accept_switch")
                    .AddRichtext("<font color=\"#d0c8b8\">J'ai lu et j'accepte le règlement. Je comprends que mon choix d'équipe est </font><font color=\"#c8953c\"><strong>définitif</strong></font>.", CairoFont.WhiteSmallText(), ElementBounds.Fixed(60, 626, 550, 30), "e_accept_text")
                    
                    .AddSmallButton("EQUIPE ATLAS", OnJoinAtlas, ElementBounds.Fixed(90, 665, 200, 40), EnumButtonStyle.Normal, "btn_atlas")
                    .AddSmallButton("EQUIPE Nouvel-Horizon", OnJoinHorizon, ElementBounds.Fixed(350, 665, 200, 40), EnumButtonStyle.Normal, "btn_horizon")
                    .AddSmallButton("Spectateur (Admin)", OnJoinSpectator, ElementBounds.Fixed(220, 715, 200, 40), EnumButtonStyle.Normal, "btn_spectator")
                
                .EndChildElements()
                .Compose();

            SingleComposer.GetScrollbar("e_scroll").SetHeights(
                (float)clipBounds.fixedHeight, 
                (float)textBounds.fixedHeight 
            );

            SingleComposer.GetButton("btn_atlas").Enabled = false;
            SingleComposer.GetButton("btn_horizon").Enabled = false;
        }

        private void OnAcceptToggle(bool on)
        {
            SingleComposer.GetButton("btn_atlas").Enabled = on;
            SingleComposer.GetButton("btn_horizon").Enabled = on;
        }

        private bool OnJoinAtlas() 
        { 
            clientChannel.SendPacket(new FactionJoinPacket { FactionId = 1 });
            SetButtonsEnabled(false);
            return true; 
        }
        
        private bool OnJoinHorizon() 
        { 
            clientChannel.SendPacket(new FactionJoinPacket { FactionId = 2 });
            SetButtonsEnabled(false);
            return true; 
        }

        private void SetButtonsEnabled(bool enabled)
        {
            SingleComposer.GetButton("btn_atlas").Enabled = enabled;
            SingleComposer.GetButton("btn_horizon").Enabled = enabled;
            SingleComposer.GetButton("btn_spectator").Enabled = enabled;
            
            // Lancement ou arrêt du timeout de sécurité
            if (!enabled)
            {
                if (timeoutListenerId != 0) capi.Event.UnregisterCallback(timeoutListenerId);
                timeoutListenerId = capi.Event.RegisterCallback(OnServerTimeout, 5000);
            }
            else
            {
                if (timeoutListenerId != 0) capi.Event.UnregisterCallback(timeoutListenerId);
                timeoutListenerId = 0;
            }
        }

        private void OnServerTimeout(float dt)
        {
            SetButtonsEnabled(true);
            capi.TriggerIngameError(this, "joinerror", "Le serveur n'a pas répondu à la demande. Veuillez réessayer.");
        }

        public void HandleServerResponse(bool success, string errorMessage)
        {
            if (success)
            {
                TryClose();
            }
            else
            {
                SetButtonsEnabled(true);
                capi.TriggerIngameError(this, "joinerror", errorMessage != null ? errorMessage : "Erreur inconnue");
            }
        }
        
        private bool OnJoinSpectator() 
        { 
            capi.ShowChatMessage("Mode Spectateur activé."); 
            TryClose(); 
            return true; 
        }

        private void OnScroll(float value) 
        { 
            textBounds.fixedY = 0 - value;
            textBounds.CalcWorldBounds();
        }

        private void OnTitleBarClose() { TryClose(); }
    }
}
