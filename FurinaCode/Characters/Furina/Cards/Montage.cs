using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Furina.Characters.Base;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Montage : BaseCard
{
    private static readonly CardType[] ReplayOrder =
    [
        CardType.Attack,
        CardType.Skill,
        CardType.Power
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        base.AdditionalHoverTips.Concat(GetCardsToReplay().Select(card => HoverTipFactory.FromCard(card)));

    protected override bool ShouldGlowGoldInternal => GetCardsToReplay().Any();

    public Montage()
        : base(4, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null)
            return;

        foreach (var card in GetCardsToReplay())
        {
            if (CombatManager.Instance.IsOverOrEnding)
                break;

            await CardCmd.AutoPlay(
                choiceContext,
                card.CreateDupe(Owner),
                null,
                AutoPlayType.Default,
                skipXCapture: true,
                skipCardPileVisuals: false);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    private IEnumerable<CardModel> GetCardsToReplay()
    {
        if (CombatState == null || Owner == null)
            return [];

        return ReplayOrder
            .Select(FindLastPlayedCardOfType)
            .OfType<CardModel>();
    }

    private CardModel? FindLastPlayedCardOfType(CardType cardType)
    {
        return CombatManager.Instance.History.CardPlaysFinished
            .LastOrDefault(entry =>
                entry.CardPlay.Player == Owner
                && entry.CardPlay.Card.Type == cardType
                && entry.CardPlay.Card != this
                && entry.CardPlay.Card is not Montage)
            ?.CardPlay.Card;
    }
}
