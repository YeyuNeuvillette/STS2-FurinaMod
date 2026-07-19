using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace Furina.Characters.Furina.Patches;

[HarmonyPatch(typeof(NCardFlyVfx), nameof(NCardFlyVfx.Create),
    [typeof(NCard), typeof(PileType), typeof(bool), typeof(string)])]
public static class ScriptPileRemoteAnimationPatch
{
    public static void Postfix(
        NCard card,
        PileType pileType,
        bool isAddingToPile,
        NCardFlyVfx __result)
    {
        if (!isAddingToPile
            || pileType != TropeSystem.PileType
            || LocalContext.IsMine(card.Model))
            return;

        __result.Visible = false;
    }
}
