using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Combat.HandSize;

namespace Furina.Characters.Furina.Patches;

[HarmonyPatch(typeof(NEndTurnButton), nameof(NEndTurnButton.Initialize))]
internal static class HandCountDisplayPatch
{
    private const string CounterNodeName = "FurinaHandCountDisplay";

    private static readonly FieldInfo? PlayerHandField =
        AccessTools.Field(typeof(NEndTurnButton), "_playerHand");

    [HarmonyPostfix]
    private static void AddHandCounter(NEndTurnButton __instance, CombatState __0)
    {
        if (__instance.GetNodeOrNull<Control>(CounterNodeName) != null)
            return;

        try
        {
            if (PlayerHandField == null)
            {
                MainFile.Logger.Warn("Failed to create Furina hand counter: NEndTurnButton._playerHand was not found.");
                return;
            }

            var playerHand = PlayerHandField?.GetValue(__instance) as CardPile;
            if (playerHand == null)
            {
                MainFile.Logger.Warn("Failed to create Furina hand counter: the local player hand was not initialized.");
                return;
            }

            var localPlayer = __0.Players.FirstOrDefault(player =>
                ReferenceEquals(player.PlayerCombatState?.Hand, playerHand));
            if (localPlayer?.Character is not global::Furina.Characters.Furina.Furina)
                return;

            __instance.AddChild(new HandCountDisplay(playerHand, localPlayer)
            {
                Name = CounterNodeName,
            });
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"Failed to create Furina hand counter: {ex.Message}");
        }
    }
}

internal sealed partial class HandCountDisplay : Label
{
    private readonly CardPile _hand;
    private readonly Player _player;

    internal HandCountDisplay(CardPile hand, Player player)
    {
        _hand = hand;
        _player = player;

        MouseFilter = MouseFilterEnum.Ignore;
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;
        // Keep this in the combat UI's normal canvas layer. A positive Z index
        // makes it draw above modal backstops and popup menus.
        ZIndex = 0;

        AnchorLeft = 0.5f;
        AnchorTop = 0f;
        AnchorRight = 0.5f;
        AnchorBottom = 0f;
        OffsetLeft = -60f;
        OffsetTop = -38f;
        OffsetRight = 60f;
        OffsetBottom = -8f;

        AddThemeColorOverride("font_color", new Color("f4fbff"));
        AddThemeColorOverride("font_outline_color", new Color("16304a"));
        AddThemeConstantOverride("outline_size", 5);
        AddThemeFontSizeOverride("font_size", 20);
    }

    public override void _Ready()
    {
        _hand.ContentsChanged += Refresh;
        Refresh();
    }

    public override void _ExitTree()
    {
        _hand.ContentsChanged -= Refresh;
    }

    private void Refresh()
    {
        var maxHandSize = MaxHandSizeCalculator.Calculate(_player);
        Text = $"{_hand.Cards.Count} / {maxHandSize}";
    }
}
