using System.Collections.Generic;
using System.Threading.Tasks;
using Furina.Characters.Base;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class DaughterOfWaterThirdMovement : MovementCard
{
    protected override int[] TargetSequence => new[] { 2, 1, 0 };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword>
    {
        ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_MOVEMENT"),
        CardKeyword.Exhaust
    };

    public DaughterOfWaterThirdMovement()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null)
            return;

        var selectedCards = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(new LocString("card_selection", "FURINA-DAUGHTER_OF_WATER_THIRD_MOVEMENT.selectionScreenPrompt"), 0, 999999999),
            card => card != this,
            this);

        foreach (var card in selectedCards)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}