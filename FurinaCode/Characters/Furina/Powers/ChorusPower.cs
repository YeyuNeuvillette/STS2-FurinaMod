using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Furina.Characters.Furina.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Furina.Characters.Furina.Powers;

[RegisterPower]
public sealed class ChorusPower : FurinaPower, IArkheDualEffectProvider
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public bool ShouldApplyBothEffects => Amount > 0;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_OUSIA_PNEUMA"))];

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);
        ArkheDualEffectHelper.SetForceNoneStateForHand(Owner, true);
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

    public async Task OnArkheCardPlayed(PlayerChoiceContext choiceContext, CardModel cardSource)
    {
        await PowerCmd.ModifyAmount(choiceContext, this, -1m, Owner, cardSource);
    }
}