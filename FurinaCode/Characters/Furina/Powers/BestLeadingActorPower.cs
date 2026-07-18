using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Furina.Characters.Furina.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Furina.Characters.Furina.Powers;

[RegisterPower]
public sealed class BestLeadingActorPower : FurinaPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private static bool _globalIsPneumaActive = true;

    public static bool GlobalIsPneumaActive => _globalIsPneumaActive;

    public static void ToggleGlobalState()
    {
        _globalIsPneumaActive = !_globalIsPneumaActive;
    }

    private bool _isPneumaActive = true;

    public bool IsPneumaActive => _isPneumaActive;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_OUSIA_PNEUMA"))];

    protected override string SmartDescriptionLocKey => _isPneumaActive
        ? base.Id.Entry + ".smartDescription_pneuma"
        : base.Id.Entry + ".smartDescription_ousia";

    public override LocString Description => _isPneumaActive
        ? new LocString("powers", base.Id.Entry + ".description_pneuma")
        : new LocString("powers", base.Id.Entry + ".description_ousia");

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);
        _isPneumaActive = _globalIsPneumaActive;
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || Owner == null || !participants.Contains(Owner))
            return;

        ToggleGlobalState();
        _isPneumaActive = _globalIsPneumaActive;
        InvokeDisplayAmountChanged();
        PlayTransformVfxOnBestLeadingActorCard();
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await base.AfterRemoved(oldOwner);
    }

    private void PlayTransformVfxOnBestLeadingActorCard()
    {
        if (Owner?.Player == null)
            return;

        foreach (var card in PileType.Hand.GetPile(Owner.Player).Cards)
        {
            if (card is BestLeadingActor bestLeadingActorCard)
            {
                bestLeadingActorCard.PlayTransformVfx();
            }
        }
    }
}