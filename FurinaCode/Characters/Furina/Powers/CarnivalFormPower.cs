using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Powers;

[RegisterPower]
public sealed class CarnivalFormPower : FurinaPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (fromHandDraw || card.Owner is not { Creature: { } creature } || creature != Owner)
            return;

        if (creature.CombatState?.CurrentSide != creature.Side)
            return;

        Flash();
        await PowerCmd.Apply<CarnivalFormStrengthPower>(choiceContext, Owner, Amount, Owner, null);
        await PowerCmd.Apply<CarnivalFormDexterityPower>(choiceContext, Owner, Amount, Owner, null);
    }
}