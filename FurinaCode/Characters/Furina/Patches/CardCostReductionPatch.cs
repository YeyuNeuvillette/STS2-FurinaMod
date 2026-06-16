using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace Furina.Characters.Furina.Patches;

[HarmonyPatch]
public static class CardCostReductionPatch
{
    private static MethodBase? TargetMethod()
    {
        return AccessTools.Method(typeof(CardEnergyCost), "GetWithModifiers");
    }

    private static void Postfix(CardEnergyCost __instance, ref int __result)
    {
        if (__result <= 0)
            return;

        var card = AccessTools.Field(typeof(CardEnergyCost), "_card")?.GetValue(__instance) as CardModel;
        if (card == null || card.IsCanonical)
            return;

        var owner = card.Owner;
        if (owner == null)
            return;

        if (owner.RunState is not RunState runState)
            return;

        if (!FurinaRunData.CardCostReductions.TryGet(runState, owner.NetId, out var data))
            return;

        if (data.ReducedCards.TryGetValue(card.Id.Entry, out int reduction))
        {
            __result = System.Math.Max(0, __result - reduction);
        }
    }
}