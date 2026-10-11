using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Review : FurinaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(2)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        base.AdditionalHoverTips.Concat(HoverTipFactory.FromCardWithCardHoverTips<Script>());

    public Review()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null)
            return;

        var scriptPile = TropeSystem.GetPile(Owner);
        if (scriptPile.Cards.Count == 0)
            return;

        var count = DynamicVars.Cards.IntValue;
        var selectedCards = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            scriptPile.Cards,
            Owner,
            new CardSelectorPrefs(
                new LocString("card_selection", "FURINA-REVIEW.selectionScreenPrompt"),
                count))).ToList();

        foreach (var card in selectedCards)
            card.EnergyCost.SetThisTurnOrUntilPlayed(0);

        await CardPileCmd.Add(selectedCards, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
