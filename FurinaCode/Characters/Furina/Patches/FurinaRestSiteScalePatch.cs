using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Characters.Visuals;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace Furina.Characters.Furina.Patches;

[HarmonyPatch(typeof(NRestSiteCharacter), "Create")]
public static class FurinaRestSiteScalePatch
{
    private static readonly Logger Logger = new("Furina", LogType.Generic);

    [HarmonyPostfix]
    public static void Postfix(Player player, int characterIndex, NRestSiteCharacter __result)
    {
        if (__result == null || player == null)
            return;

        if (player.Character is Furina furina)
        {
            if (furina.WorldProceduralVisuals?.RestSite != null)
            {
                __result.Scale = new Vector2(0.3f, 0.3f);
                __result.Position = new Vector2(66, -46);
                Logger.Info($"[FurinaRestSiteScalePatch] Applied scale={__result.Scale}, position={__result.Position}");
            }
        }
    }
}

[HarmonyPatch(typeof(NRestSiteRoom), "_Ready")]
public static class FurinaRestSiteRoomPositionPatch
{
    private static readonly Logger Logger = new("Furina", LogType.Generic);

    [HarmonyPostfix]
    public static void Postfix(NRestSiteRoom __instance)
    {
        foreach (var character in __instance.Characters)
        {
            if (character.Player?.Character is Furina furina &&
                furina.WorldProceduralVisuals?.RestSite != null)
            {
                character.Position = new Vector2(66, -46);
                Logger.Info($"[FurinaRestSiteRoomPositionPatch] Adjusted position for {character.Name} to (66, -46)");

                AdjustThoughtBubblePositions(character);
            }
        }
    }

    private static void AdjustThoughtBubblePositions(NRestSiteCharacter character)
    {
        try
        {
            var controlRoot = character.GetNode<Control>("ControlRoot");
            var thoughtLeft = controlRoot.GetNode<Control>("%ThoughtBubbleLeft");
            var thoughtRight = controlRoot.GetNode<Control>("%ThoughtBubbleRight");

            float scaleFactor = 0.3f;

            thoughtLeft.OffsetLeft = -73.6836f * scaleFactor;
            thoughtLeft.OffsetTop = -324.997f * scaleFactor;
            thoughtRight.OffsetLeft = 209.209f * scaleFactor;
            thoughtRight.OffsetTop = -317.103f * scaleFactor;

            Logger.Info($"[FurinaRestSiteRoomPositionPatch] Adjusted thought bubble positions with scale factor {scaleFactor}");
        }
        catch (Exception ex)
        {
            Logger.Error($"[FurinaRestSiteRoomPositionPatch] Failed to adjust thought bubble positions: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(NRestSiteCharacter), "FlipX")]
public static class FurinaRestSiteFlipXPatch
{
    private static readonly Logger Logger = new("Furina", LogType.Generic);

    [HarmonyPostfix]
    public static void Postfix(NRestSiteCharacter __instance)
    {
        if (__instance?.Player?.Character is Furina furina &&
            furina.WorldProceduralVisuals?.RestSite != null)
        {
            try
            {
                var visuals = __instance.GetNodeOrNull<Sprite2D>("Visuals");
                if (visuals != null)
                {
                    Vector2 scale = visuals.Scale;
                    scale.X = -scale.X;
                    visuals.Scale = scale;

                    Vector2 position = visuals.Position;
                    position.X -= 43f;
                    position.Y -= 25f;
                    visuals.Position = position;

                    Logger.Info($"[FurinaRestSiteFlipXPatch] Flipped Sprite2D for {__instance.Name}, new scale={visuals.Scale}, new position={visuals.Position}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[FurinaRestSiteFlipXPatch] Failed to flip Sprite2D: {ex.Message}");
            }
        }
    }
}