using System.Linq;
using System.Threading.Tasks;
using Furina.Characters.Base;
using Furina.Characters.Furina.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class WaterAndJustice : FurinaCard
{
    public override bool CanBeGeneratedInCombat => false;

    public WaterAndJustice()
        : base(0, CardType.Power, CardRarity.Ancient, TargetType.None)
    {
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var targetCard = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(new LocString("card_selection", "FURINA-WATERANDJUSTICE.selectionScreenPrompt"), 1),
            card => true,
            this)).FirstOrDefault();

        if (targetCard == null)
            return;

        var deckCard = targetCard.DeckVersion ?? targetCard;
        FurinaRunData.CardCostReductions.Modify(Owner, data =>
        {
            if (data.ReducedCards.ContainsKey(deckCard.Id.Entry))
                data.ReducedCards[deckCard.Id.Entry] += 2;
            else
                data.ReducedCards[deckCard.Id.Entry] = 2;
        });

        await PowerCmd.Apply<WaterAndJusticePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }
}