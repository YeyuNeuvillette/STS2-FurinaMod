using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.HandSize;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class FullHouse : FurinaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(9)];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    public FullHouse()
        : base(4, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.PlayerCombatState == null)
            return;

        var hand = PileType.Hand.GetPile(Owner);
        var requested = DynamicVars.Cards.IntValue;
        var availableCards = PileType.Draw.GetPile(Owner).Cards.Count
            + PileType.Discard.GetPile(Owner).Cards.Count;
        var freeSlots = Math.Max(0, MaxHandSizeCalculator.Calculate(Owner) - hand.Cards.Count);
        var overflow = Math.Max(0, Math.Min(requested, availableCards) - freeSlots);

        await CardPileCmd.Draw(choiceContext, requested, Owner);

        for (var i = 0; i < overflow && hand.Cards.Count > 0; i++)
        {
            var card = Owner.RunState.Rng.CombatCardSelection.NextItem(hand.Cards);
            card?.EnergyCost.AddUntilPlayed(-1);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(4m);
    }
}
