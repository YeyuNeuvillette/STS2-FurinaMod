using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;
using MinionLib.Action;
using Furina.Characters.Furina.Minions;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Furina.Characters.Furina.Patches;

public partial class MinionActionHotkeyNode : Node
{
    private static readonly Dictionary<Key, Type> HotkeyToMinion = new()
    {
        { Key.C, typeof(SurintendanteChevalmarin) },
        { Key.V, typeof(GentilhommeUsher) },
        { Key.B, typeof(MademoiselleCrabaletta) }
    };

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventKey { Pressed: true } keyEvent)
            return;

        if (!HotkeyToMinion.TryGetValue(keyEvent.Keycode, out var minionType))
            return;

        if (!CombatManager.Instance.IsInProgress || CombatManager.Instance.PlayerActionsDisabled)
            return;

        var queueSynchronizer = RunManager.Instance.ActionQueueSynchronizer;
        if (queueSynchronizer.CombatState != ActionSynchronizerCombatState.PlayPhase)
            return;

        var me = LocalContext.GetMe(CombatManager.Instance.DebugOnlyGetState());
        if (me == null) return;

        var pet = me.PlayerCombatState?.Pets
            .FirstOrDefault(p => p.Monster?.GetType() == minionType);
        if (pet == null || !pet.IsAlive) return;

        var combatState = pet.CombatState;
        if (combatState == null) return;

        var actionPower = pet.Powers
            .OfType<ActionModel>()
            .FirstOrDefault(a => !a.OnlyRespondIconClick && a.CanAct(combatState));
        if (actionPower == null) return;

        var creatureNode = NCombatRoom.Instance?.GetCreatureNode(pet);
        if (creatureNode == null) return;

        GetViewport().SetInputAsHandled();
        TaskHelper.RunSafely(TriggerActionAsync(creatureNode, actionPower));
    }

    private static async Task TriggerActionAsync(NCreature actorNode, ActionModel actionPower)
    {
        var actor = actorNode.Entity;
        var combatState = actor.CombatState;
        if (combatState == null) return;

        var targetType = actionPower.TargetType;

        if (targetType == TargetType.None)
        {
            actionPower.Flash();
            await actionPower.TryAct(new ThrowingPlayerChoiceContext(), null);
            return;
        }

        var validTargets = actionPower.GetValidTargets(combatState);
        if (validTargets.Count == 0) return;

        if (!targetType.IsSingleTarget())
        {
            actionPower.Flash();
            await actionPower.TryAct(new ThrowingPlayerChoiceContext(), null);
            return;
        }

        if (targetType == TargetType.Self)
        {
            await actionPower.TryAct(new ThrowingPlayerChoiceContext(), null);
            return;
        }

        actionPower.StartPulsing();
        try
        {
            var startPosition = actorNode.Hitbox.GlobalPosition + actorNode.Hitbox.Size / 2f;
            NTargetManager.Instance.StartTargeting(targetType, startPosition, TargetMode.ClickMouseToTarget,
                () => !GodotObject.IsInstanceValid(actorNode) || !actor.IsAlive, null);

            var selectedNode = await NTargetManager.Instance.SelectionFinished();
            if (selectedNode is not NCreature targetNode) return;

            var target = targetNode.Entity;
            if (!actionPower.IsValidTarget(combatState, target)) return;

            await actionPower.TryAct(new ThrowingPlayerChoiceContext(), target);
        }
        finally
        {
            actionPower.StopPulsing();
        }
    }
}