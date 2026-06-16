using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using Furina.Characters.Furina.Minions;

namespace Furina.Characters.Furina.Patches;

[HarmonyPatch]
public static class SalonMemberOrderPatch
{
    private static readonly Dictionary<Type, int> SalonMemberOrder = new()
    {
        { typeof(MademoiselleCrabaletta), 0 },
        { typeof(GentilhommeUsher), 1 },
        { typeof(SurintendanteChevalmarin), 2 }
    };

    private static MethodBase? TargetMethod()
    {
        return AccessTools.Method(typeof(PlayerCombatState), nameof(PlayerCombatState.AddPetInternal));
    }

    private static void Postfix(PlayerCombatState __instance)
    {
        var petsField = AccessTools.Field(typeof(PlayerCombatState), "_pets");
        if (petsField?.GetValue(__instance) is not List<Creature> pets || pets.Count <= 1)
            return;

        bool hasSalonMember = false;
        foreach (var pet in pets)
        {
            if (pet.Monster != null && SalonMemberOrder.ContainsKey(pet.Monster.GetType()))
            {
                hasSalonMember = true;
                break;
            }
        }

        if (!hasSalonMember)
            return;

        var salonMembers = pets
            .Where(p => p.Monster != null && SalonMemberOrder.ContainsKey(p.Monster.GetType()))
            .OrderBy(p => SalonMemberOrder[p.Monster!.GetType()])
            .ToList();
        var others = pets
            .Where(p => p.Monster == null || !SalonMemberOrder.ContainsKey(p.Monster.GetType()))
            .ToList();

        pets.Clear();
        pets.AddRange(salonMembers);
        pets.AddRange(others);
    }
}