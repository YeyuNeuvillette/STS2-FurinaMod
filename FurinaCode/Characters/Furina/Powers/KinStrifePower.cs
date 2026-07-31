using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Powers;

[RegisterPower]
public sealed class KinStrifePower : FurinaPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (amount <= 0m || creature != Owner)
            return;

        var combatState = CombatState;
        if (combatState == null)
            return;

        var hittableEnemies = combatState.HittableEnemies;
        if (hittableEnemies.Count == 0)
            return;

        var target = Owner!.Player!.RunState.Rng.CombatTargets.NextItem(hittableEnemies);
        if (target == null)
            return;

        Flash();
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, amount, ValueProp.Unpowered, Owner!);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
            await PowerCmd.Remove(this);
    }
}