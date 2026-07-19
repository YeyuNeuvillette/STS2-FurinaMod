using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MinionLib.Action;
using Furina.Characters.Furina.Systems;

namespace Furina.Characters.Furina.Patches;

[HarmonyPatch]
public static class ActionDescriptionHotkeyPatch
{
    private const string HotkeyToken = "[hotkey]";

    private static MethodBase? TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(PowerModel), nameof(PowerModel.HoverTips));
    }

    static void Postfix(PowerModel __instance, ref IEnumerable<IHoverTip> __result)
    {
        if (__instance is not ActionModel) return;

        var hotkey = FurinaHotkeyService.GetHotkeyForLocEntry(__instance.Id.Entry);
        if (hotkey == null) return;

        var modified = new List<IHoverTip>();
        foreach (var tip in __result)
        {
            if (tip is HoverTip ht && ht.Description != null && ht.Description.Contains(HotkeyToken))
            {
                modified.Add(ht with { Description = ht.Description.Replace(HotkeyToken, hotkey) });
            }
            else
            {
                modified.Add(tip);
            }
        }

        __result = modified;
    }
}
