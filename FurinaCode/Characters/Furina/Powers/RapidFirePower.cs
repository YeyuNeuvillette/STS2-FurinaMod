using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Furina.Characters.Furina.Actions;
using Furina.Characters.Furina.Minions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Powers;

[RegisterPower]
public sealed class RapidFirePower : FurinaPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterSideTurnStartLate(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player || Owner?.Player == null)
            return Task.CompletedTask;

        var usher = Owner.Player.PlayerCombatState?.Pets
            .FirstOrDefault(pet => pet.Monster is GentilhommeUsher);
        usher?.GetPower<HydroBarrage>()?.AddUses(Amount);
        if (usher != null)
            Flash();

        return Task.CompletedTask;
    }
}
