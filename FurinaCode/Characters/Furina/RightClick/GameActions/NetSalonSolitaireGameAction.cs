using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace Furina.Characters.Furina.RightClick.GameActions;

public struct NetSalonSolitaireGameAction : INetAction
{
    public uint? MinionCombatId;

    public GameAction ToGameAction(Player player)
    {
        return new SalonSolitaireGameAction(player, MinionCombatId);
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteBool(MinionCombatId.HasValue);
        if (MinionCombatId.HasValue)
            writer.WriteUInt(MinionCombatId.Value, 6);
    }

    public void Deserialize(PacketReader reader)
    {
        MinionCombatId = reader.ReadBool() ? reader.ReadUInt(6) : null;
    }

    public override string ToString()
    {
        return $"{nameof(NetSalonSolitaireGameAction)} minion={MinionCombatId?.ToString() ?? "null"}";
    }
}