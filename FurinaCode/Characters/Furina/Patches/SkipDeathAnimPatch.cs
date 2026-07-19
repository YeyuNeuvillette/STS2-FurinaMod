using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using static Godot.Control;

namespace Furina.Characters.Furina.Patches;

[HarmonyPatch(typeof(NCreature), nameof(NCreature.StartDeathAnim))]
public static class SkipDeathAnimPatch
{
    private static readonly HashSet<uint> _skipDeathAnimCreatureIds = new();
    private static readonly Task _completedTask = Task.CompletedTask;

    public static void RegisterCreatureToSkipDeathAnim(Creature creature)
    {
        if (creature?.CombatId is uint combatId)
        {
            _skipDeathAnimCreatureIds.Add(combatId);
        }
    }

    public static void UnregisterCreatureToSkipDeathAnim(Creature creature)
    {
        if (creature?.CombatId is uint combatId)
        {
            _skipDeathAnimCreatureIds.Remove(combatId);
        }
    }

    public static void ClearAll()
    {
        _skipDeathAnimCreatureIds.Clear();
    }

    [HarmonyPrefix]
    private static bool Prefix(NCreature __instance, bool shouldRemove)
    {
        if (__instance.Entity?.CombatId is not uint combatId)
        {
            return true;
        }
        
        if (_skipDeathAnimCreatureIds.Remove(combatId))
        {
            GD.Print($"[SkipDeathAnimPatch] Skipping death animation for {__instance.Entity.Name}");
            
            __instance.Hitbox.FocusMode = FocusModeEnum.None;
            __instance.Hitbox.MouseFilter = MouseFilterEnum.Ignore;
            __instance.DeathAnimationTask = _completedTask;
            
            if (shouldRemove)
            {
                GD.Print($"[SkipDeathAnimPatch] Removing creature node for {__instance.Entity.Name}");
                NCombatRoom.Instance?.RemoveCreatureNode(__instance);
                __instance.QueueFreeSafely();
            }
            
            return false;
        }
        return true;
    }
}
