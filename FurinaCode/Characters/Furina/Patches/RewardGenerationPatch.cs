using HarmonyLib;
using MegaCrit.Sts2.Core.Rewards;

namespace Furina.Characters.Furina.Patches;

[HarmonyPatch(typeof(RewardsSet), nameof(RewardsSet.GenerateWithoutOffering))]
public static class RewardGenerationPatch
{
    public static void Postfix(RewardsSet __instance)
    {
        RewardModifierBus.RunAll(__instance.Player, __instance.Rewards, __instance.Room);
    }
}