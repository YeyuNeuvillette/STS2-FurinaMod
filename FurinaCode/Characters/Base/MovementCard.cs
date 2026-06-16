using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Furina.Extensions;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Helpers;

namespace Furina.Characters.Base;

public abstract class MovementCard(
    int energyCost,
    CardType type,
    CardRarity rarity,
    TargetType targetType,
    bool shouldShowInCardLibrary = true)
    : ModCardTemplate(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
{
    private static readonly HashSet<string> _unlockedCardIdsThisCombat = new();

    public static void ResetCombatState()
    {
        _unlockedCardIdsThisCombat.Clear();
    }

    private string ArtFileName => $"{Id.Entry.ToCardArtFileName()}.png";
    private string LegacyArtFileName => $"{Id.Entry.ToLegacyCompactFileName()}.png";
    private string LegacyPrefixedArtFileName => $"{MainFile.ModId.ToLowerInvariant()}_{Id.Entry.ToLegacyCompactFileName()}.png";

    protected abstract int[] TargetSequence { get; }
    
    private int _currentIndex = 0;
    private bool _hasPlayedEffect = false;

    private bool IsUnlocked => _unlockedCardIdsThisCombat.Contains(Id.Entry);

    protected override bool IsPlayable => IsUnlocked && base.IsPlayable;

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        _unlockedCardIdsThisCombat.Remove(Id.Entry);
        _currentIndex = 0;
        _hasPlayedEffect = false;
        await base.AfterCombatEnd(room);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword>
    {
        ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_MOVEMENT")
    };

    protected override void AddExtraArgsToDescription(LocString description)
    {
        try
        {
            base.AddExtraArgsToDescription(description);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[MovementCard] base.AddExtraArgsToDescription error: {ex.Message}");
        }
        
        try
        {
            UpdateSequenceProgress();
            description.Add("SequenceProgress", BuildSequenceString());
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[MovementCard] SequenceProgress error: {ex.Message}");
            description.Add("SequenceProgress", $"[gold]乐章[/gold]：［{string.Join("-", TargetSequence.Select(_ => $"[color=#808080]?[/color]"))}］\n");
        }
    }

    public override string CustomPortraitPath =>
        ResolveExistingPath(
            ArtFileName.BigCardsImagePath(),
            ArtFileName.CardsImagePath(),
            LegacyPrefixedArtFileName.BigCardsImagePath(),
            LegacyPrefixedArtFileName.CardsImagePath(),
            LegacyArtFileName.BigCardsImagePath(),
            LegacyArtFileName.CardsImagePath(),
            "card.png".BigCardsImagePath(),
            "card.png".CardsImagePath());

    public override string PortraitPath =>
        ResolveExistingPath(
            ArtFileName.CardsImagePath(),
            ArtFileName.BigCardsImagePath(),
            LegacyPrefixedArtFileName.CardsImagePath(),
            LegacyPrefixedArtFileName.BigCardsImagePath(),
            LegacyArtFileName.CardsImagePath(),
            LegacyArtFileName.BigCardsImagePath(),
            "card.png".CardsImagePath(),
            "card.png".BigCardsImagePath());

    public override string? CustomBetaPortraitPath =>
        ResolveExistingPath(
            ArtFileName.CardBetaImagePath(),
            LegacyPrefixedArtFileName.CardBetaImagePath(),
            LegacyArtFileName.CardBetaImagePath(),
            PortraitPath);

    private void UpdateSequenceProgress()
    {
        if (IsUnlocked || CombatState == null || Owner == null)
            return;

        try
        {
            var cardPlays = CombatManager.Instance.History.CardPlaysStarted
                .Where(e => e != null && e.HappenedThisTurn(CombatState) && e.CardPlay?.Card?.Owner == Owner && e.CardPlay?.Card != this)
                .ToList();

            int newIndex = 0;
            foreach (var cardPlay in cardPlays)
            {
                var playedCardCost = cardPlay.CardPlay.Resources.EnergySpent;
                
                if (playedCardCost == TargetSequence[newIndex])
                {
                    newIndex++;
                    if (newIndex >= TargetSequence.Length)
                    {
                        UnlockCard();
                        return;
                    }
                }
                else
                {
                    newIndex = 0;
                }
            }

            if (newIndex != _currentIndex)
            {
                _currentIndex = newIndex;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[MovementCard] Error updating sequence progress: {ex.Message}");
        }
    }

    private void UnlockCard()
    {
        _unlockedCardIdsThisCombat.Add(Id.Entry);
        GD.Print($"[MovementCard] {Id.Entry} unlocked!");
        
        PlayUnlockEffect();
        ForceUpdateCardVisuals();
    }

    private void ForceUpdateCardVisuals()
    {
        try
        {
            var combatRoom = NCombatRoom.Instance;
            if (combatRoom?.Ui?.Hand != null)
            {
                var cardHolder = combatRoom.Ui.Hand.ActiveHolders.FirstOrDefault(h => h.CardModel == this);
                if (cardHolder?.CardNode != null)
                {
                    cardHolder.CardNode.UpdateVisuals(cardHolder.CardNode.DisplayingPile, CardPreviewMode.Normal);
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[MovementCard] Error forcing card visual update: {ex.Message}");
        }
    }

    private void PlayUnlockEffect()
    {
        if (_hasPlayedEffect)
            return;
        
        _hasPlayedEffect = true;

        try
        {
            var combatRoom = NCombatRoom.Instance;
            if (combatRoom?.Ui?.Hand != null)
            {
                var cardHolder = combatRoom.Ui.Hand.ActiveHolders.FirstOrDefault(h => h.CardModel == this);
                if (cardHolder is NHandCardHolder handCardHolder && handCardHolder.CardNode != null)
                {
                    var vfx = NCardTransformShineVfx.Create(handCardHolder.CardNode, this, null);
                    if (vfx != null)
                    {
                        TaskHelper.RunSafely(vfx.PlayAnimation(shortVersion: false));
                    }
                    handCardHolder.CardNode.Reload();
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[MovementCard] Error playing unlock effect: {ex.Message}");
        }
    }

    private string BuildSequenceString()
    {
        if (IsUnlocked)
            return string.Empty;
            
        var parts = new List<string>();
        for (int i = 0; i < TargetSequence.Length; i++)
        {
            if (i < _currentIndex)
            {
                parts.Add($"[gold]{TargetSequence[i]}[/gold]");
            }
            else
            {
                parts.Add($"[color=#808080]{TargetSequence[i]}[/color]");
            }
        }
        return $"[gold]乐章[/gold]：［{string.Join("-", parts)}］\n";
    }

    private static string ResolveExistingPath(params string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (ResourceLoader.Exists(candidate))
                return candidate;
        }
        return candidates[^1];
    }
}