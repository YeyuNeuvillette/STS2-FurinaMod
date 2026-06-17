using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class MiseEnScene : OusiaPneumaCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword>
    {
        ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_OUSIA_PNEUMA")
    };

    public MiseEnScene()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override TargetType GetOusiaTargetType() => TargetType.Self;
    protected override TargetType GetPneumaTargetType() => TargetType.Self;

    protected override async Task OnOusiaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SelectFromDrawPile(choiceContext, 1);
    }

    protected override async Task OnPneumaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SelectFromDiscardPile(choiceContext, 1);
    }

    protected override async Task OnNoArkheEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SelectFromDrawPile(choiceContext, 1);
    }

    protected override async Task OnChorusEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SelectFromDrawPile(choiceContext, 1);
        await SelectFromDiscardPile(choiceContext, 1);
    }

    private async Task SelectFromDrawPile(PlayerChoiceContext choiceContext, int count)
    {
        if (Owner == null)
            return;

        var drawPile = PileType.Draw.GetPile(Owner);
        if (drawPile.Cards.Count == 0)
            return;

        var maxSelect = System.Math.Min(count, drawPile.Cards.Count);
        var selected = await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            drawPile.Cards,
            Owner,
            new CardSelectorPrefs(new LocString("card_selection", "FURINA-MISE_EN_SCENE.selectionScreenPrompt"), maxSelect));

        await CardPileCmd.Add(selected, PileType.Hand);
    }

    private async Task SelectFromDiscardPile(PlayerChoiceContext choiceContext, int count)
    {
        if (Owner == null)
            return;

        var discardPile = PileType.Discard.GetPile(Owner);
        if (discardPile.Cards.Count == 0)
            return;

        var maxSelect = System.Math.Min(count, discardPile.Cards.Count);
        var selected = await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            discardPile.Cards,
            Owner,
            new CardSelectorPrefs(new LocString("card_selection", "FURINA-MISE_EN_SCENE.selectionScreenPrompt"), maxSelect));

        await CardPileCmd.Add(selected, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}