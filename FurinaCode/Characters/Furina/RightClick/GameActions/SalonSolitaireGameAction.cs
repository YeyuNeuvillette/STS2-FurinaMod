using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MinionLib.Powers;
using MinionLib.Action;

namespace Furina.Characters.Furina.RightClick.GameActions;

public sealed class SalonSolitaireGameAction : GameAction
{
    public override ulong OwnerId => Owner.NetId;

    public override GameActionType ActionType => GameActionType.CombatPlayPhaseOnly;

    private Player Owner { get; }

    private uint? MinionCombatId { get; }

    public SalonSolitaireGameAction(Player owner, Creature minion)
    {
        Owner = owner;
        MinionCombatId = minion.CombatId;
    }

    public SalonSolitaireGameAction(Player owner, uint? minionCombatId)
    {
        Owner = owner;
        MinionCombatId = minionCombatId;
    }

    protected override async Task ExecuteAction()
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null)
        {
            Cancel();
            return;
        }

        Creature? minion = null;
        if (MinionCombatId.HasValue)
            minion = await combatState.GetCreatureAsync(MinionCombatId, 10.0);

        var pets = Owner.PlayerCombatState?.Pets.ToList();
        if (pets == null)
        {
            Cancel();
            return;
        }

        var hasGuardian = minion?.GetPower<MinionGuardianPower>() != null;

        foreach (var pet in pets)
        {
            if (InvitationManager.IsFurinaMinion(pet) && pet.GetPower<MinionGuardianPower>() != null)
            {
                await PowerCmd.Remove<MinionGuardianPower>(pet);
            }
        }

        if (!hasGuardian && minion != null && minion.IsAlive)
        {
            await PowerCmd.Apply<MinionGuardianPower>(
                new GameActionPlayerChoiceContext(this), minion, 1m, Owner.Creature, null);
        }
    }

    public override INetAction ToNetAction()
    {
        return new NetSalonSolitaireGameAction
        {
            MinionCombatId = MinionCombatId
        };
    }

    public override string ToString()
    {
        return $"{nameof(SalonSolitaireGameAction)} owner={OwnerId} minion={MinionCombatId?.ToString() ?? "null"}";
    }
}

public sealed class MinionActGameAction : GameAction
{
    private Player Owner { get; }
    private uint MinionCombatId { get; }
    private uint? TargetCombatId { get; }

    public override ulong OwnerId => Owner.NetId;
    public override GameActionType ActionType => GameActionType.CombatPlayPhaseOnly;

    public MinionActGameAction(Player owner, Creature minion, Creature? target)
        : this(owner, minion.CombatId ?? throw new InvalidOperationException("Minion has no combat id."), target?.CombatId)
    {
    }

    public MinionActGameAction(Player owner, uint minionCombatId, uint? targetCombatId)
    {
        Owner = owner;
        MinionCombatId = minionCombatId;
        TargetCombatId = targetCombatId;
    }

    protected override async Task ExecuteAction()
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null)
        {
            Cancel();
            return;
        }

        var minion = await combatState.GetCreatureAsync(MinionCombatId, 10.0);
        var action = minion?.Powers.OfType<ActionModel>()
            .FirstOrDefault(candidate => !candidate.OnlyRespondIconClick && candidate.CanAct(combatState));
        if (minion == null || action == null)
        {
            Cancel();
            return;
        }

        Creature? target = null;
        if (TargetCombatId.HasValue)
            target = await combatState.GetCreatureAsync(TargetCombatId.Value, 10.0);
        if (target != null && !action.IsValidTarget(combatState, target))
        {
            Cancel();
            return;
        }

        await action.TryAct(new GameActionPlayerChoiceContext(this), target);
    }

    public override INetAction ToNetAction() => new NetMinionActGameAction
    {
        MinionCombatId = MinionCombatId,
        TargetCombatId = TargetCombatId
    };
}
