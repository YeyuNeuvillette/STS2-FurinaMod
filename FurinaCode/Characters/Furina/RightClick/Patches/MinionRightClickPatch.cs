using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;
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

        var salonSolitairePower = creature.GetPower<Powers.SalonSolitairePower>();
        if (salonSolitairePower == null) return;

        if (creature.Monster is SingerOfManyWaters)
            return;

        var player = me;
        if (player == null) return;

        if (MinionStorage.IsSalonSolitaireUsed(player))
            return;

        MinionStorage.MarkSalonSolitaireUsed(player);

        RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new SalonSolitaireGameAction(player, creature));
        creatureNode.GetViewport().SetInputAsHandled();
    }
}