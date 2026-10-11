using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class LeadDancer : FurinaCard
{
    private const string SwiftAmountKey = "SwiftAmount";

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new List<CardKeyword> { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[]
        {
            new CardsVar(2),
            new IntVar(SwiftAmountKey, 1)
        };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        base.AdditionalHoverTips.Concat(
            HoverTipFactory.FromEnchantment<Swift>(DynamicVars[SwiftAmountKey].IntValue));

    public LeadDancer()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.PlayerCombatState == null)
            return;

        var drawPile = Owner.PlayerCombatState.DrawPile;
        var swift = ModelDb.Enchantment<Swift>();
        var cardsToDraw = drawPile.Cards
            .Where(card => card.Enchantment == null && swift.CanEnchant(card))
            .Take(DynamicVars.Cards.IntValue)
            .ToList();

        if (cardsToDraw.Count == 0)
            return;

        // Moving the selected cards to the top lets the normal draw pipeline
        // retain its animations, hooks, no-draw checks, and hand-size limit.
        await CardPileCmd.Add(
            cardsToDraw.AsEnumerable().Reverse(),
            drawPile,
            CardPilePosition.Top,
            skipVisuals: true);

        var drawnCards = await CardPileCmd.Draw(choiceContext, cardsToDraw.Count, Owner);
        foreach (var card in drawnCards.Where(cardsToDraw.Contains))
            CardCmd.Enchant<Swift>(card, DynamicVars[SwiftAmountKey].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
