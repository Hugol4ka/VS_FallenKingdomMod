using System.Collections.Generic;

namespace FallenKingdom
{
    public class FactionSpawn
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public FactionSpawn() { }

        public FactionSpawn(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool IsSet()
        {
            return X != 0 || Y != 0 || Z != 0;
        }
    }

    public class FallenKingdomConfig
    {
        public int MaxFactionPlayers { get; set; } = 15;
        public FactionSpawn AtlasSpawn { get; set; } = new FactionSpawn();
        public FactionSpawn HorizonSpawn { get; set; } = new FactionSpawn();
    }

    public class FallenKingdomData
    {
        // Player UID -> Faction ID (1 = Atlas, 2 = Nouvel-Horizon)
        public Dictionary<string, int> PlayerFactions { get; set; } = new Dictionary<string, int>();
    }
}
