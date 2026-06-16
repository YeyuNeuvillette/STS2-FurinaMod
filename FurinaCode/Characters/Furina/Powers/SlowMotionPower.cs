using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Powers;

[RegisterPower]
public sealed class SlowMotionPower : FurinaPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => 0;

    private bool _hasOffsetSelf;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_hasOffsetSelf)
            return;

        var slowPower = base.Owner?.GetPower<SlowPower>();
        if (slowPower != null)
        {
            slowPower.DynamicVars["SlowAmount"].BaseValue--;
            slowPower.InvokeDisplayAmountChanged();
            _hasOffsetSelf = true;
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(base.Owner))
            return;

        var slowPower = base.Owner?.GetPower<SlowPower>();
        if (slowPower != null)
        {
            await PowerCmd.Remove(slowPower);
        }
        await PowerCmd.Remove(this);
    }
}