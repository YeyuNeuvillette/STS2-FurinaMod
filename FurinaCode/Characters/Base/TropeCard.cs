using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;

namespace Furina.Characters.Base;

public abstract class TropeCard : BaseCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword>
    {
        ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_TROPE"),
        ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_SCRIPT")
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        base.AdditionalHoverTips.Concat(
            HoverTipFactory.FromCardWithCardHoverTips<Furina.Cards.Script>());

    protected TropeCard(
        int energyCost,
        CardType type,
        CardRarity rarity,
        TargetType targetType,
        bool shouldShowInCardLibrary = true)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override CardLocation GetResultLocationForCardPlay()
    {
        var result = PileType.None;
        var position = CardPilePosition.Top;
        
        if (IsDupe || Type == CardType.Power)
        {
            result = PileType.None;
            position = CardPilePosition.Top;
        }
        else if (TropeSystem.IsReplaying)
        {
            result = PileType.Discard;
            position = CardPilePosition.Bottom;
        }
        else
        {
            result = TropeSystem.PileType;
            position = CardPilePosition.Top;
        }
        
        return new CardLocation(Owner, result, position);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!TropeSystem.IsReplaying && Owner != null)
            await TropeSystem.EnsureScriptCard(Owner);
    }
}
