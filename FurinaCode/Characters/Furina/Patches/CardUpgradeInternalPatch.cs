using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Furina.Characters.Furina.Cards;

namespace Furina.Characters.Furina.Patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.UpgradeInternal))]
public static class CardUpgradeInternalPatch
{
    public static void Postfix(CardModel __instance)
    {
        if (__instance.Pile == null)
            return;

        if (RunManager.Instance?.IsInProgress == true && RunManager.Instance.DebugOnlyGetState() is RunState runState)
        {
            BackupDancer.SubscribeAllForCombat(runState.Players);
        }
        CardUpgradeEventBus.NotifyCardUpgraded(__instance);
    }
}