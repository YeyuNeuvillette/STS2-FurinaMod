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
    private string ArtFileName => $"{Id.Entry.ToCardArtFileName()}.png";
    private string LegacyArtFileName => $"{Id.Entry.ToLegacyCompactFileName()}.png";
    private string LegacyPrefixedArtFileName => $"{MainFile.ModId.ToLowerInvariant()}_{Id.Entry.ToLegacyCompactFileName()}.png";

    protected abstract IReadOnlyList<int> TargetSequence { get; }
    
    private int _currentIndex = 0;
    private bool _hasPlayedEffect = false;

    private bool IsUnlocked => MovementSystem.IsUnlocked(this);

    protected override bool IsPlayable
    {
        get
        {
            UpdateSequenceProgress();
            return IsUnlocked && base.IsPlayable;
        }
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
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
            if (!IsCanonical)
                UpdateSequenceProgress();
            description.Add("SequenceProgress", BuildSequenceString());
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[MovementCard] SequenceProgress error: {ex.Message}");
            description.Add("SequenceProgress", BuildStaticSequenceString());
        }
    }

    public override string CustomPortraitPath =>
        AssetPathResolver.ResolveExisting(
            ArtFileName.BigCardsImagePath(),
            ArtFileName.CardsImagePath(),
            LegacyPrefixedArtFileName.BigCardsImagePath(),
            LegacyPrefixedArtFileName.CardsImagePath(),
            LegacyArtFileName.BigCardsImagePath(),
            LegacyArtFileName.CardsImagePath(),
            "card.png".BigCardsImagePath(),
            "card.png".CardsImagePath());

    public override string PortraitPath =>
        AssetPathResolver.ResolveExisting(
            ArtFileName.CardsImagePath(),
            ArtFileName.BigCardsImagePath(),
            LegacyPrefixedArtFileName.CardsImagePath(),
            LegacyPrefixedArtFileName.BigCardsImagePath(),
            LegacyArtFileName.CardsImagePath(),
            LegacyArtFileName.BigCardsImagePath(),
            "card.png".CardsImagePath(),
            "card.png".BigCardsImagePath());

    public override string? CustomBetaPortraitPath =>
        AssetPathResolver.ResolveExisting(
            ArtFileName.CardBetaImagePath(),
            LegacyPrefixedArtFileName.CardBetaImagePath(),
            LegacyArtFileName.CardBetaImagePath(),
            PortraitPath);

    private void UpdateSequenceProgress()
    {
        if (IsCanonical || IsUnlocked || CombatState == null || Owner == null)
            return;

        try
        {
            var newIndex = MovementSystem.CalculateProgress(this, TargetSequence);
            if (newIndex >= TargetSequence.Count)
            {
                UnlockCard();
                return;
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
        if (!MovementSystem.Unlock(this))
            return;
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
        if (!IsCanonical && IsUnlocked)
            return string.Empty;
            
        var parts = new List<string>();
        for (int i = 0; i < TargetSequence.Count; i++)
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

    private string BuildStaticSequenceString() =>
        $"[gold]乐章[/gold]：［{string.Join("-", TargetSequence.Select(value => $"[color=#808080]{value}[/color]"))}］\n";
}

internal static class MovementSystem
{
    private static readonly HashSet<(ulong playerId, string cardId)> UnlockedCards = [];

    public static void ResetCombat() => UnlockedCards.Clear();

    public static bool IsUnlocked(MovementCard card) =>
        card.Owner != null && UnlockedCards.Contains((card.Owner.NetId, card.Id.Entry));

    public static bool Unlock(MovementCard card)
    {
        return card.Owner != null && UnlockedCards.Add((card.Owner.NetId, card.Id.Entry));
    }

    public static int CalculateProgress(MovementCard card, IReadOnlyList<int> targetSequence)
    {
        if (card.CombatState == null || card.Owner == null || targetSequence.Count == 0)
            return 0;

        var costs = CombatManager.Instance.History.CardPlaysStarted
            .Where(entry => entry != null
                && entry.HappenedThisTurn(card.CombatState)
                && entry.CardPlay?.Card?.Owner == card.Owner
                && entry.CardPlay.Card != card)
            .Select(entry => (int)entry.CardPlay.Resources.EnergySpent)
            .ToList();

        for (var start = 0; start < costs.Count; start++)
        {
            var matched = 0;
            while (start + matched < costs.Count
                && matched < targetSequence.Count
                && costs[start + matched] == targetSequence[matched])
            {
                matched++;
            }

            if (matched == targetSequence.Count)
                return matched;
        }

        var maxSuffix = Math.Min(costs.Count, targetSequence.Count - 1);
        for (var length = maxSuffix; length > 0; length--)
        {
            if (costs.TakeLast(length).SequenceEqual(targetSequence.Take(length)))
                return length;
        }

        return 0;
    }
}
