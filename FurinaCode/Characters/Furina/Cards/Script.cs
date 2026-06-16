using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.CardPiles;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Base;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Combat;
using STS2RitsuLib.Keywords;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(TokenCardPool))]
public sealed class Script : BaseCard
{
    private CardPile? _scriptPile;
    private bool _hasSubscribed = false;

    public override bool CanBeGeneratedInCombat => false;

    public override int MaxUpgradeLevel => 0;

    public Script()
        : base(0, CardType.Skill, CardRarity.Token, TargetType.None, shouldShowInCardLibrary: false)
    {
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword>
    {
        CardKeyword.Retain,
        ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_SCRIPT")
    };

    protected override int CanonicalEnergyCost
    {
        get
        {
            GD.Print($"[Script] CanonicalEnergyCost getter called");
            
            if (Owner?.PlayerCombatState == null)
            {
                GD.Print($"[Script] Owner or PlayerCombatState is null, returning 0");
                return 0;
            }

            var scriptPileType = GetScriptPileType();
            var scriptPile = scriptPileType.GetPile(Owner);

            GD.Print($"[Script] Script pile: {scriptPile.Type}, Cards count: {scriptPile.Cards.Count}");

            if (scriptPile.Cards.Count == 0)
            {
                GD.Print($"[Script] Script pile is empty, returning 0");
                return 0;
            }

            var totalCost = scriptPile.Cards
                .Select(card => card.EnergyCost.GetWithModifiers(CostModifiers.All))
                .Sum();
            
            var finalCost = Math.Min(totalCost, 3);
            GD.Print($"[Script] Final energy cost: {finalCost} (calculated: {totalCost}, capped at 3)");
            return finalCost;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        GD.Print($"[Script] OnPlay called, CombatState: {CombatState != null}, Owner: {Owner}");
        
        if (Owner?.PlayerCombatState == null)
            return;

        var scriptPileType = GetScriptPileType();
        var scriptPile = scriptPileType.GetPile(Owner);

        GD.Print($"[Script] Script pile cards count: {scriptPile.Cards.Count}");

        if (scriptPile.Cards.Count == 0)
            return;

        var cardsToPlay = scriptPile.Cards.ToList();
        GD.Print($"[Script] Playing {cardsToPlay.Count} cards from script pile");

        GD.Print($"[Script] Before PlayFromScriptPile, _isPlayingFromScriptPile: {TropeCard._isPlayingFromScriptPile}");
        using (TropeCard.PlayFromScriptPile())
        {
            GD.Print($"[Script] Inside PlayFromScriptPile, _isPlayingFromScriptPile: {TropeCard._isPlayingFromScriptPile}");
            
            for (int i = 0; i < cardsToPlay.Count; i++)
            {
                var card = cardsToPlay[i];
                GD.Print($"[Script] Playing card {i + 1}/{cardsToPlay.Count}: {card.Id.Entry}, CombatState: {card.CombatState != null}");
                
                if (CombatManager.Instance.IsOverOrEnding)
                {
                    GD.Print($"[Script] Combat ending, stopping playback");
                    break;
                }
                    
                await CardCmd.AutoPlay(choiceContext, card, null, AutoPlayType.Default, skipXCapture: true, skipCardPileVisuals: false);
                GD.Print($"[Script] Finished playing card {i + 1}/{cardsToPlay.Count}, _isPlayingFromScriptPile: {TropeCard._isPlayingFromScriptPile}");
            }
            
            GD.Print($"[Script] Before disposing PlayFromScriptPile, _isPlayingFromScriptPile: {TropeCard._isPlayingFromScriptPile}");
        }
        GD.Print($"[Script] After PlayFromScriptPile, _isPlayingFromScriptPile: {TropeCard._isPlayingFromScriptPile}");
        
        GD.Print($"[Script] OnPlay completed");
    }

    internal void EnsureScriptPileSubscription()
    {
        GD.Print($"[Script] EnsureScriptPileSubscription called, _hasSubscribed: {_hasSubscribed}");
        
        if (Owner?.PlayerCombatState == null)
        {
            GD.Print($"[Script] Owner or PlayerCombatState is null");
            return;
        }

        if (_hasSubscribed)
        {
            GD.Print($"[Script] Already subscribed, skipping");
            return;
        }

        var scriptPileType = GetScriptPileType();
        var scriptPile = scriptPileType.GetPile(Owner);
        
        GD.Print($"[Script] Script pile: {scriptPile.Type}, Cards count: {scriptPile.Cards.Count}");
        
        if (scriptPile != _scriptPile)
        {
            GD.Print($"[Script] Subscribing to script pile ContentsChanged");
            
            if (_scriptPile != null)
            {
                _scriptPile.ContentsChanged -= OnScriptPileContentsChanged;
            }
            
            _scriptPile = scriptPile;
            
            if (_scriptPile != null)
            {
                _scriptPile.ContentsChanged += OnScriptPileContentsChanged;
                _hasSubscribed = true;
                GD.Print($"[Script] Subscription successful, triggering initial energy cost update");
                InvokeEnergyCostChanged();
            }
        }
    }

    private void OnScriptPileContentsChanged()
    {
        GD.Print($"[Script] OnScriptPileContentsChanged called");
        
        if (Owner?.PlayerCombatState == null)
            return;
            
        var scriptPileType = GetScriptPileType();
        var scriptPile = scriptPileType.GetPile(Owner);
        
        var totalCost = scriptPile.Cards
            .Select(card => card.EnergyCost.GetWithModifiers(CostModifiers.All))
            .Sum();
        
        var finalCost = Math.Min(totalCost, 3);
        
        GD.Print($"[Script] Calculated total cost: {totalCost}, setting to: {finalCost}");
        
        try
        {
            EnergyCost.SetCustomBaseCost(finalCost);
            GD.Print($"[Script] Energy cost set successfully");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Script] Error setting energy cost: {ex.Message}");
        }
    }

    private static PileType GetScriptPileType()
    {
        if (ModCardPileRegistry.TryGet("FURINA_CARDPILE_SCRIPT", out var definition))
        {
            return definition.PileType;
        }
        return PileType.Discard;
    }
}