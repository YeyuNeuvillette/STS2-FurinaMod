using Furina.Characters.Furina.UI;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Furina.Characters.Furina.Patches;

[HarmonyPatch(typeof(NCreature), nameof(NCreature._Ready))]
public static class SalonMemberIntentPatch
{
    public static void Postfix(NCreature __instance)
    {
        SalonMemberIntentPresenter.TryAttach(__instance);
    }
}

[HarmonyPatch(typeof(NCreature), nameof(NCreature.UpdateIntent))]
public static class SalonMemberIntentRefreshPatch
{
    public static void Postfix(NCreature __instance)
    {
        SalonMemberIntentPresenter.TryRefresh(__instance);
    }
}
