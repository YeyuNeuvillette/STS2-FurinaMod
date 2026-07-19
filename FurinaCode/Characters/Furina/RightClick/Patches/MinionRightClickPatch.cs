using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;
using Furina.Characters.Furina.Minions;
using Furina.Characters.Furina.RightClick.GameActions;

namespace Furina.Characters.Furina.RightClick.Patches;

[HarmonyPatch(typeof(NCreature), nameof(NCreature._Ready))]
public static class MinionRightClickPatch
{
    [HarmonyPostfix]
    private static void Postfix(NCreature __instance)
    {
        if (!InvitationManager.IsFurinaMinion(__instance.Entity))
            return;

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

        if (!InvitationManager.IsFurinaMinion(creature)) return;
        if (creature.PetOwner == null || !LocalContext.IsMe(creature.PetOwner)) return;

        RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new SalonSolitaireGameAction(creature.PetOwner, creature));
        creatureNode.GetViewport().SetInputAsHandled();
    }
}
