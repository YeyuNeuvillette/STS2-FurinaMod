using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class PreviouslyOn : FurinaCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new List<CardKeyword> { CardKeyword.Exhaust };

    public PreviouslyOn()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.PlayerCombatState == null)
            return;

        var drawPile = Owner.PlayerCombatState.DrawPile;
        var cardsWithDrawEffects = drawPile.Cards
            .Where(CardDrawEffectDetector.HasCardDrawEffect)
            .ToList();

        if (cardsWithDrawEffects.Count == 0)
            return;

        // Add inserts every card at index 0. Reversing here keeps their original
        // relative order when they are moved to the top of the draw pile.
        await CardPileCmd.Add(
            cardsWithDrawEffects.AsEnumerable().Reverse(),
            drawPile,
            CardPilePosition.Top,
            skipVisuals: true);

        await CardPileCmd.Draw(choiceContext, cardsWithDrawEffects.Count, Owner);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
