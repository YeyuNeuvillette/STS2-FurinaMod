using System.Threading;
using System.Runtime.CompilerServices;
using Furina.Characters.Furina.Cards;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.CardPiles;
using ScriptCard = Furina.Characters.Furina.Cards.Script;

namespace Furina;

public static class TropeSystem
{
    private const string PileId = "FURINA_CARDPILE_SCRIPT";
    private static readonly AsyncLocal<int> ReplayDepth = new();
    private static ConditionalWeakTable<CardModel, ScriptCostSnapshot> _pendingScriptCosts = new();
    private static PileType? _pileType;

    public static bool IsReplaying => ReplayDepth.Value > 0;

    public static PileType PileType
    {
        get
        {
            if (_pileType.HasValue)
                return _pileType.Value;

            _pileType = ModCardPileRegistry.TryGet(PileId, out var definition)
                ? definition.PileType
                : MegaCrit.Sts2.Core.Entities.Cards.PileType.Discard;
            return _pileType.Value;
        }
    }

    public static void Register()
    {
        var spec = new ModCardPileSpec
        {
            Scope = ModCardPileScope.CombatOnly,
            Style = ModCardPileUiStyle.BottomLeft,
            Anchor = ModCardPileAnchor.Default,
            IconPath = "res://Furina/images/ui/script_pile_registration.png",
            VisibleWhen = context => context.Player?.Piles
                .SelectMany(pile => pile.Cards)
                .Any(card => card is ScriptCard) == true
        };

        ModCardPileRegistry.For(MainFile.ModId).RegisterOwned("SCRIPT", spec);
    }

    public static void ResetCombat()
    {
        ReplayDepth.Value = 0;
        _pendingScriptCosts = new ConditionalWeakTable<CardModel, ScriptCostSnapshot>();
    }

    public static void CaptureCostForScript(CardModel card, int cost)
    {
        _pendingScriptCosts.Remove(card);
        _pendingScriptCosts.Add(card, new ScriptCostSnapshot(Math.Max(0, cost)));
    }

    public static bool TryTakeCapturedScriptCost(CardModel card, out int cost)
    {
        if (_pendingScriptCosts.TryGetValue(card, out var snapshot))
        {
            cost = snapshot.Cost;
            _pendingScriptCosts.Remove(card);
            return true;
        }

        cost = 0;
        return false;
    }

    public static void ForgetCapturedScriptCost(CardModel card)
    {
        _pendingScriptCosts.Remove(card);
    }

    public static CardPile GetPile(Player player) => PileType.GetPile(player);

    public static async Task EnsureScriptCard(Player player)
    {
        if (player.PlayerCombatState == null || player.Creature.CombatState == null)
            return;

        var hasScript = player.PlayerCombatState.Hand.Cards.Any(card => card is ScriptCard)
            || player.PlayerCombatState.DrawPile.Cards.Any(card => card is ScriptCard)
            || player.PlayerCombatState.DiscardPile.Cards.Any(card => card is ScriptCard);
        if (hasScript)
            return;

        var script = player.Creature.CombatState.CreateCard<ScriptCard>(player);
        await CardPileCmd.AddGeneratedCardToCombat(script, MegaCrit.Sts2.Core.Entities.Cards.PileType.Hand, player);
        script.BindToScriptPile();
        GD.Print($"[TropeSystem] Created Script for player {player.NetId}");
    }

    public static IDisposable BeginReplay()
    {
        ReplayDepth.Value++;
        return new ReplayScope();
    }

    private sealed class ReplayScope : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            ReplayDepth.Value = Math.Max(0, ReplayDepth.Value - 1);
        }
    }

    private sealed record ScriptCostSnapshot(int Cost);
}
