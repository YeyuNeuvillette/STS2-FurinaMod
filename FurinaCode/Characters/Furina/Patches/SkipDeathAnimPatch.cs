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
    private static readonly HashSet<uint?> _skipDeathAnimCreatures = new();
    private static readonly Task _completedTask = Task.CompletedTask;

    public static void RegisterCreatureToSkipDeathAnim(Creature creature)
    {
        if (creature != null)
        {
            _skipDeathAnimCreatures.Add(creature.CombatId);
        }
    }

    public static void UnregisterCreatureToSkipDeathAnim(Creature creature)
    {
        if (creature != null)
        {
            _skipDeathAnimCreatures.Remove(creature.CombatId);
        }
    }

    public static void ClearAll()
    {
        _skipDeathAnimCreatures.Clear();
    }

    [HarmonyPrefix]
    private static bool Prefix(NCreature __instance, bool shouldRemove)
    {
        if (__instance.Entity == null)
        {
            return true;
        }
        
        GD.Print($"[SkipDeathAnimPatch] Prefix called for {__instance.Entity.Name}, CombatId: {__instance.Entity.CombatId}, shouldRemove: {shouldRemove}");
        GD.Print($"[SkipDeathAnimPatch] Skip list contains: {_skipDeathAnimCreatures.Contains(__instance.Entity.CombatId)}");
        
        if (_skipDeathAnimCreatures.Contains(__instance.Entity.CombatId))
        {
            _skipDeathAnimCreatures.Remove(__instance.Entity.CombatId);
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