using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace FallenKingdom
{
    public class FallenKingdomMod : ModSystem
    {
        // --- CLIENT VARIABLES ---
        private ICoreClientAPI capi;
        private IClientNetworkChannel clientChannel;
        private GuiDialogFallenKingdom guiDialog;

        // --- SERVER VARIABLES ---
        private ICoreServerAPI sapi;
        private IServerNetworkChannel serverChannel;
        private FallenKingdomConfig config;
        private FallenKingdomData data;

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return true; // Load on both client and server
        }

        #region Server Logic
        
        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            this.sapi = api;

            // Load configuration and data
            LoadConfig();

            // Register server network channel and message handlers
            serverChannel = api.Network.RegisterChannel("fallenkingdom")
                .RegisterMessageType<OpenFactionSelectionPacket>()
                .RegisterMessageType<FactionJoinPacket>()
                .RegisterMessageType<FactionJoinResponsePacket>()
                .SetMessageHandler<FactionJoinPacket>(OnFactionJoinRequested);

            // Register event hooks
            api.Event.PlayerJoin += OnPlayerJoin;
            api.Event.PlayerRespawn += OnPlayerRespawn;

            // Register server commands
            RegisterCommands();
        }

        private void LoadConfig()
        {
            try { config = sapi.LoadModConfig<FallenKingdomConfig>("FallenKingdomConfig.json"); } catch { }
            if (config == null)
            {
                config = new FallenKingdomConfig();
                sapi.StoreModConfig(config, "FallenKingdomConfig.json");
            }

            try { data = sapi.LoadModConfig<FallenKingdomData>("FallenKingdomData.json"); } catch { }
            if (data == null)
            {
                data = new FallenKingdomData();
                sapi.StoreModConfig(data, "FallenKingdomData.json");
            }
        }

        private void SaveConfig()
        {
            sapi.StoreModConfig(config, "FallenKingdomConfig.json");
        }

        private void SaveData()
        {
            sapi.StoreModConfig(data, "FallenKingdomData.json");
        }

        private int GetFactionPlayerCount(int factionId)
        {
            int count = 0;
            if (data?.PlayerFactions != null)
            {
                foreach (var kvp in data.PlayerFactions)
                {
                    if (kvp.Value == factionId)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        private void RegisterCommands()
        {
            sapi.ChatCommands.Create("setfactionspawn")
                .WithDescription("Définit le point de spawn d'une faction")
                .WithArgs(sapi.ChatCommands.Parsers.Int("factionId"))
                .RequiresPrivilege(Privilege.controlserver)
                .HandleWith(OnSetFactionSpawn);
        }

        private TextCommandResult OnSetFactionSpawn(TextCommandCallingArgs args)
        {
            if (args.Caller.Player is IServerPlayer player)
            {
                int factionId = (int)args[0];
                if (factionId != 1 && factionId != 2)
                {
                    return TextCommandResult.Error("ID de faction invalide (1 = Atlas, 2 = Nouvel-Horizon).");
                }

                Vec3d pos = player.Entity.Pos.XYZ;
                FactionSpawn spawn = new FactionSpawn
                {
                    X = (int)Math.Floor(pos.X),
                    Y = (int)Math.Floor(pos.Y),
                    Z = (int)Math.Floor(pos.Z)
                };

                if (factionId == 1)
                {
                    config.AtlasSpawn = spawn;
                }
                else
                {
                    config.HorizonSpawn = spawn;
                }

                SaveConfig();

                return TextCommandResult.Success($"Point de spawn de la faction {(factionId == 1 ? "Atlas" : "Nouvel-Horizon")} défini sur {spawn.X}, {spawn.Y}, {spawn.Z}.");
            }

            return TextCommandResult.Error("Cette commande doit être exécutée par un joueur.");
        }

        private void OnPlayerJoin(IServerPlayer player)
        {
            string playerUid = player.PlayerUID;
            if (data.PlayerFactions != null && !data.PlayerFactions.ContainsKey(playerUid))
            {
                // Send trigger packet to client to open selection GUI
                serverChannel.SendPacket(new OpenFactionSelectionPacket
                {
                    AtlasCount = GetFactionPlayerCount(1),
                    HorizonCount = GetFactionPlayerCount(2),
                    MaxCapacity = config.MaxFactionPlayers
                }, player);
            }
            else if (data.PlayerFactions != null)
            {
                // Set their custom spawn point to their faction spawn
                int factionId = data.PlayerFactions[playerUid];
                FactionSpawn spawn = factionId == 1 ? config.AtlasSpawn : config.HorizonSpawn;
                if (spawn != null && spawn.IsSet())
                {
                    player.SetSpawnPosition(new PlayerSpawnPos
                    {
                        x = spawn.X,
                        y = spawn.Y,
                        z = spawn.Z,
                        yaw = 0f,
                        pitch = 0f,
                        roll = 0f
                    });
                }
            }
        }

        private void OnPlayerRespawn(IServerPlayer player)
        {
            string playerUid = player.PlayerUID;
            if (data.PlayerFactions != null && data.PlayerFactions.TryGetValue(playerUid, out int factionId))
            {
                FactionSpawn spawn = factionId == 1 ? config.AtlasSpawn : config.HorizonSpawn;
                if (spawn != null && spawn.IsSet())
                {
                    // Instant teleport to faction base on respawn
                    player.Entity.TeleportTo(new Vec3d(spawn.X + 0.5, spawn.Y, spawn.Z + 0.5));
                }
            }
        }

        private void OnFactionJoinRequested(IServerPlayer player, FactionJoinPacket packet)
        {
            if (packet.FactionId != 1 && packet.FactionId != 2)
            {
                serverChannel.SendPacket(new FactionJoinResponsePacket { Success = false, ErrorMessage = "Faction invalide." }, player);
                return;
            }

            string playerUid = player.PlayerUID;
            if (data.PlayerFactions != null && data.PlayerFactions.ContainsKey(playerUid))
            {
                serverChannel.SendPacket(new FactionJoinResponsePacket { Success = false, ErrorMessage = "Vous êtes déjà dans une faction." }, player);
                return;
            }

            int currentCount = GetFactionPlayerCount(packet.FactionId);
            if (currentCount >= config.MaxFactionPlayers)
            {
                serverChannel.SendPacket(new FactionJoinResponsePacket { Success = false, ErrorMessage = "Cette faction est déjà complète." }, player);
                return;
            }

            // Register the player
            if (data.PlayerFactions == null) data.PlayerFactions = new Dictionary<string, int>();
            data.PlayerFactions[playerUid] = packet.FactionId;
            SaveData();

            // Set spawn point
            FactionSpawn spawn = packet.FactionId == 1 ? config.AtlasSpawn : config.HorizonSpawn;
            if (spawn != null && spawn.IsSet())
            {
                player.SetSpawnPosition(new PlayerSpawnPos
                {
                    x = spawn.X,
                    y = spawn.Y,
                    z = spawn.Z,
                    yaw = 0f,
                    pitch = 0f,
                    roll = 0f
                });

                // Teleport them immediately
                player.Entity.TeleportTo(new Vec3d(spawn.X + 0.5, spawn.Y, spawn.Z + 0.5));
            }

            // Notify client of success
            serverChannel.SendPacket(new FactionJoinResponsePacket { Success = true }, player);

            // Broadcast message to all online players
            string factionName = packet.FactionId == 1 ? "ATLAS" : "Nouvel-Horizon";
            string msg = $"[Fallen Kingdom] {player.PlayerName} a rejoint l'équipe {factionName} !";
            foreach (IPlayer onlinePlayer in sapi.World.AllOnlinePlayers)
            {
                (onlinePlayer as IServerPlayer)?.SendMessage(GlobalConstants.GeneralChatGroup, msg, EnumChatType.Notification);
            }
        }

        #endregion

        #region Client Logic

        public override void StartClientSide(ICoreClientAPI api)
        {
            this.capi = api;
            
            // Enregistrement du canal réseau pour le client
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

        #endregion
    }
}