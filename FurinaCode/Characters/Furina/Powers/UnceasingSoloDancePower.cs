using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Powers;

[RegisterPower]
public sealed class UnceasingSoloDancePower : FurinaPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterHandEmptied(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player
            || player.PlayerCombatState is not { } playerCombatState
            || !IsValidPhase(playerCombatState.Phase))
            return;

        Flash();
        await CardPileCmd.Draw(choiceContext, Amount, player);
    }

    private static bool IsValidPhase(PlayerTurnPhase phase)
    {
        return (uint)(phase - 2) <= 2u;
    }
}
