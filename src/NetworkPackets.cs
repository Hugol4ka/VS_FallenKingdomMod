using ProtoBuf;

namespace FallenKingdom
{
    // Envoyé par le serveur au client à la connexion s'il n'a pas de faction
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class OpenFactionSelectionPacket
    {
        public int AtlasCount;
        public int HorizonCount;
        public int MaxCapacity;
    }

    // Envoyé par le client au serveur lors du clic sur un bouton
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class FactionJoinPacket
    {
        public int FactionId; // 1 = Atlas, 2 = Nouvel-Horizon
    }

    // Renvoyé par le serveur au client après vérification
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class FactionJoinResponsePacket
    {
        public bool Success;
        public string ErrorMessage;
    }
}
