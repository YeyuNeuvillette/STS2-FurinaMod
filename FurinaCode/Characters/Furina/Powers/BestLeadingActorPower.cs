using System.Collections.Generic;
using System.Threading.Tasks;
using Furina.Characters.Furina.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Furina.Characters.Furina.Powers;

[RegisterPower]
public sealed class BestLeadingActorPower : FurinaPower, IArkheDualEffectProvider
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private int _arkheCardsPlayedThisTurn;

    public bool ShouldApplyBothEffects => _arkheCardsPlayedThisTurn < Amount;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_OUSIA_PNEUMA"))];

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);
        ArkheDualEffectHelper.SetForceNoneStateForHand(Owner, true);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        await base.AfterPlayerTurnStart(choiceContext, player);
        _arkheCardsPlayedThisTurn = 0;
        ArkheDualEffectHelper.SetForceNoneStateForHand(Owner, ShouldApplyBothEffects);
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        ArkheDualEffectHelper.OnCardDrawn(Owner, card);
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await base.AfterRemoved(oldOwner);
        ArkheDualEffectHelper.OnProviderRemoved(oldOwner);
    }

    public Task OnArkheCardPlayed(PlayerChoiceContext choiceContext, CardModel cardSource)
    {
        _arkheCardsPlayedThisTurn++;
        ArkheDualEffectHelper.SetForceNoneStateForHand(Owner, ShouldApplyBothEffects);
        return Task.CompletedTask;
    }
}