using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;
using MinionLib.Action;
using Furina.Characters.Furina.Minions;
using Furina.Characters.Furina.RightClick.GameActions;

namespace Furina.Characters.Furina.RightClick.Patches;

[HarmonyPatch(typeof(NCreature), nameof(NCreature._Ready))]
public static class MinionRightClickPatch
{
    private const string Module = "MinionRightClickPatch";

    [HarmonyPostfix]
    private static void Postfix(NCreature __instance)
    {
        __instance.Hitbox.Connect(Control.SignalName.GuiInput,
            Callable.From<InputEvent>(inputEvent => OnMinionGuiInput(__instance, inputEvent)));
    }

    private static void OnMinionGuiInput(NCreature creatureNode, InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton { ButtonIndex: MouseButton.Left } leftBtn && leftBtn.IsReleased())
        {
            var c = creatureNode.Entity;
            var inputHandled = creatureNode.GetViewport().IsInputHandled();
            var hasAction = c.Powers.OfType<ActionModel>().Any();
            var isAlive = c.IsAlive;
            var cs = c.CombatState;
            var isInProgress = CombatManager.Instance.IsInProgress;
            var playerActionsDisabled = CombatManager.Instance.PlayerActionsDisabled;
            var queueState = RunManager.Instance.ActionQueueSynchronizer.CombatState;
            var isPetOwnerMe = c.PetOwner != null && LocalContext.IsMe(c.PetOwner);
            var sideMatch = cs != null && cs.CurrentSide == c.Side;
            GD.Print($"[MinionLeftClickDebug] Left-click on {c.Name}: " +
                     $"inputHandled={inputHandled}, hasAction={hasAction}, isAlive={isAlive}, " +
                     $"isInProgress={isInProgress}, playerActionsDisabled={playerActionsDisabled}, " +
                     $"queueState={queueState}, isPetOwnerMe={isPetOwnerMe}, sideMatch={sideMatch}");
            if (hasAction)
            {
                foreach (var act in c.Powers.OfType<ActionModel>())
                {
                    var canAct = cs != null && act.CanAct(cs);
                    var onlyIcon = act.OnlyRespondIconClick;
                    GD.Print($"[MinionLeftClickDebug]   Action {act.Id.Entry}: canAct={canAct}, onlyIconClick={onlyIcon}, amount={act.Amount}");
                }
            }
        }

        if (creatureNode.GetViewport().IsInputHandled())
            return;

        var triggeredByMouse =
            inputEvent is InputEventMouseButton { ButtonIndex: MouseButton.Right } mouseButton &&
            mouseButton.IsReleased();

        var triggeredByController =
            inputEvent is InputEventAction { Action: var action } actionEvent &&
            action == MegaInput.cancel &&
            actionEvent.IsPressed() &&
            creatureNode.Hitbox.HasFocus();

        if (!triggeredByMouse && !triggeredByController) return;
        if (NTargetManager.Instance.IsInSelection) return;

        var creature = creatureNode.Entity;

        var me = LocalContext.GetMe(creature.CombatState);
        if (me == null) return;

        if (!InvitationManager.IsFurinaMinion(creature)) return;

        var player = me;
        if (player == null) return;

        RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new SalonSolitaireGameAction(player, creature));
        creatureNode.GetViewport().SetInputAsHandled();
    }
}
