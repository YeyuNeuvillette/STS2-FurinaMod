using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Furina.Cards;

namespace Furina.Characters.Furina.Powers;

[RegisterPower]
public sealed class DodgeAndWeavePneumaPower : FurinaPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || cardPlay.Card.Owner != Owner.Player || cardPlay.Card is DodgeAndWeave)
            return;

        Flash();
        await AgilityPowerRef.Apply(choiceContext, Owner, Amount, Owner, cardPlay.Card);
    }
}
