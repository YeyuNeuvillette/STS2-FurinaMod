using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Rush : FurinaCard
{
    private int HandThreshold => IsUpgraded ? 8 : 10;

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new CardsVar(2)
    };

    public Rush()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var drawnCards = await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

        var hand = Owner?.PlayerCombatState?.Hand;
        if (hand != null && hand.Cards.Count >= HandThreshold)
        {
            foreach (var card in drawnCards)
            {
                card.EnergyCost.AddUntilPlayed(-1);
            }
        }
    }
}