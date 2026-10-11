using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using MinionLib.Powers;
using MinionLib.Action;
using Furina.Characters.Furina.Actions;
using Furina.Characters.Furina.Systems;

namespace Furina.Characters.Furina.Powers;

[RegisterPower]
public sealed class SalonSolitairePower : FurinaPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<MinionGuardianPower>() };

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        await base.AfterPlayerTurnStart(choiceContext, player);
        Amount = 0;
    }

    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        await base.BeforeSideTurnEnd(choiceContext, side, participants);

        if (!FurinaHotkeyService.AutoUseUnusedSalonActions
            || side != CombatSide.Player
            || Owner.Side != side
            || !Owner.IsAlive
            || Owner.PetOwner is not { } petOwner
            || Owner.CombatState is not { } combatState)
            return;

        var action = Owner.Powers
            .OfType<ActionModel>()
            .FirstOrDefault(candidate => candidate is HydroBarrage or CrabShield);
        if (action == null)
            return;

        while (action.CanAct(combatState))
        {
            Creature? target;
            if (action is CrabShield)
            {
                target = petOwner.Creature;
                if (!action.IsValidTarget(combatState, target))
                    return;
            }
            else
            {
                var validTargets = action.GetValidTargets(combatState)
                    .Where(candidate => candidate.IsAlive)
                    .ToList();
                if (validTargets.Count == 0)
                    return;

                target = petOwner.RunState.Rng.CombatTargets.NextItem(validTargets);
                if (target == null)
                    return;
            }

            await action.TryAct(choiceContext, target);
        }
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        await base.AfterDeath(choiceContext, creature, wasRemovalPrevented, deathAnimLength);
        if (!wasRemovalPrevented && creature == Owner)
            InvitationManager.OnMinionDeath(creature);
    }
}
