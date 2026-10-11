using Furina.Characters.Furina.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Furina.Characters.Furina.Patches;

/// <summary>
/// CardModel renders its Replay line and hover tip from GetEnchantedReplayCount,
/// while Epic Masterpiece grants the actual extra play through
/// ModifyCardPlayCount. Bridge those two paths only while the card is physically
/// inside the Script pile; AutoPlay moves it to the Play pile before resolving,
/// so the existing power remains the sole source of the gameplay effect.
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetEnchantedReplayCount))]
public static class ScriptPileReplayDisplayPatch
{
    public static void Postfix(CardModel __instance, ref int __result)
    {
        if (__instance.Pile?.Type != TropeSystem.PileType)
            return;

        var power = __instance.Owner.Creature.GetPower<EpicMasterpiecePower>();
        if (power is null || power.Amount <= 0)
            return;

        __result += power.Amount;
    }
}
