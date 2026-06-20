using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Furina.Characters.Furina.Patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.UpgradeInternal))]
public static class CardUpgradeInternalPatch
{
    public static void Postfix(CardModel __instance)
    {
        if (!__instance.IsInCombat)
            return;
        CardUpgradeEventBus.NotifyCardUpgraded(__instance);
    }
}