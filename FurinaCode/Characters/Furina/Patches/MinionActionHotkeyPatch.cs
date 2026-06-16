using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Furina.Characters.Furina.Patches;

[HarmonyPatch]
public static class MinionActionHotkeyPatch
{
    private static MinionActionHotkeyNode? _node;

    private static MethodBase? TargetMethod()
    {
        return AccessTools.Method(typeof(NCombatRoom), nameof(NCombatRoom._Ready));
    }

    private static void Postfix(NCombatRoom __instance)
    {
        if (_node != null && GodotObject.IsInstanceValid(_node))
        {
            _node.QueueFree();
        }

        _node = new MinionActionHotkeyNode();
        __instance.AddChild(_node);
    }
}