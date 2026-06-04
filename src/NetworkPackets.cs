using ProtoBuf;

namespace FallenKingdom
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class OpenFactionSelectionPacket
    {
        public int AtlasCount;
        public int HorizonCount;
        public int MaxCapacity;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class FactionJoinPacket
    {
        public int FactionId; // 1 = Atlas, 2 = Nouvel-Horizon
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class FactionJoinResponsePacket
    {
        public bool Success;
        public string? ErrorMessage;
    }
}
