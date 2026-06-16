using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Furina.Characters.Base;
using Furina.Characters.Furina.Powers;
using STS2RitsuLib.Keywords;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.TestSupport;

namespace Furina.Characters.Furina.Cards;

public abstract class OusiaPneumaCard(
    int energyCost,
    CardType type,
    CardRarity rarity,
    TargetType targetType,
    bool shouldShowInCardLibrary = true)
    : FurinaCard(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
{
    private ArkheState _currentArkheState = ArkheState.None;
    private bool _forceNoneState;

    public enum ArkheState
    {
        None,
        Ousia,
        Pneuma
    }

    public ArkheState CurrentArkheState
    {
        get => _currentArkheState;
        private set
        {
            if (_currentArkheState != value)
            {
                var oldState = _currentArkheState;
                _currentArkheState = value;
                OnArkheStateChanged(oldState, value);
            }
        }
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword>
    {
        ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_OUSIA_PNEUMA")
    };

    public override string PortraitPath
    {
        get
        {
            try
            {
                if (base.Id == null || string.IsNullOrEmpty(base.Id.Entry) || Pool == null || string.IsNullOrEmpty(Pool.Title))
                    return base.PortraitPath;

                var suffix = GetArkheStateSuffix();
                if (!string.IsNullOrEmpty(suffix))
                {
                    var arkhePath = ImageHelper.GetImagePath($"atlases/card_atlas.sprites/{Pool.Title.ToLowerInvariant()}/{base.Id.Entry.ToLowerInvariant()}{suffix}.tres");
                    if (Godot.FileAccess.FileExists(arkhePath))
                    {
                        return arkhePath;
                    }
                }
            }
            catch (System.Exception ex)
            {
                GD.PrintErr($"[OusiaPneumaCard] PortraitPath error: {ex.Message}");
            }
            return base.PortraitPath;
        }
    }

    public override CardType Type
    {
        get
        {
            try
            {
                return GetCardTypeForArkheState(CurrentArkheState);
            }
            catch (System.Exception ex)
            {
                GD.PrintErr($"[OusiaPneumaCard] Type error: {ex.Message}");
                return base.Type;
            }
        }
    }

    public override CardRarity Rarity
    {
        get
        {
            try
            {
                return GetCardRarityForArkheState(CurrentArkheState);
            }
            catch (System.Exception ex)
            {
                GD.PrintErr($"[OusiaPneumaCard] Rarity error: {ex.Message}");
                return base.Rarity;
            }
        }
    }

    public override TargetType TargetType
    {
        get
        {
            try
            {
                return GetTargetTypeForArkheState(CurrentArkheState);
            }
            catch (System.Exception ex)
            {
                GD.PrintErr($"[OusiaPneumaCard] TargetType error: {ex.Message}");
                return base.TargetType;
            }
        }
    }

    protected override int CanonicalEnergyCost
    {
        get
        {
            try
            {
                return GetEnergyCostForArkheState(CurrentArkheState);
            }
            catch (System.Exception ex)
            {
                GD.PrintErr($"[OusiaPneumaCard] CanonicalEnergyCost error: {ex.Message}");
                return base.CanonicalEnergyCost;
            }
        }
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        
        try
        {
            description.Add("ArkheState", CurrentArkheState.ToString());
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[OusiaPneumaCard] AddExtraArgsToDescription error: {ex.Message}");
        }
    }

    private string GetArkheStateSuffix()
    {
        try
        {
            return CurrentArkheState switch
            {
                ArkheState.Ousia => HasOusiaResources() ? "_ousia" : "",
                ArkheState.Pneuma => HasPneumaResources() ? "_pneuma" : "",
                _ => ""
            };
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[OusiaPneumaCard] GetArkheStateSuffix error: {ex.Message}");
            return "";
        }
    }

    private bool HasOusiaResources()
    {
        try
        {
            if (base.Id == null || string.IsNullOrEmpty(base.Id.Entry))
                return false;
            
            if (Pool == null || string.IsNullOrEmpty(Pool.Title))
                return false;

            var hasTitle = LocString.Exists("cards", $"{base.Id.Entry}_ousia.title");
            var hasDescription = LocString.Exists("cards", $"{base.Id.Entry}_ousia.description");
            var portraitPath = ImageHelper.GetImagePath($"atlases/card_atlas.sprites/{Pool.Title.ToLowerInvariant()}/{base.Id.Entry.ToLowerInvariant()}_ousia.tres");
            var hasPortrait = Godot.FileAccess.FileExists(portraitPath);
            
            return hasTitle || hasDescription || hasPortrait;
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[OusiaPneumaCard] HasOusiaResources error: {ex.Message}");
            return false;
        }
    }

    private bool HasPneumaResources()
    {
        try
        {
            if (base.Id == null || string.IsNullOrEmpty(base.Id.Entry))
                return false;
            
            if (Pool == null || string.IsNullOrEmpty(Pool.Title))
                return false;

            var hasTitle = LocString.Exists("cards", $"{base.Id.Entry}_pneuma.title");
            var hasDescription = LocString.Exists("cards", $"{base.Id.Entry}_pneuma.description");
            var portraitPath = ImageHelper.GetImagePath($"atlases/card_atlas.sprites/{Pool.Title.ToLowerInvariant()}/{base.Id.Entry.ToLowerInvariant()}_pneuma.tres");
            var hasPortrait = Godot.FileAccess.FileExists(portraitPath);
            
            return hasTitle || hasDescription || hasPortrait;
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[OusiaPneumaCard] HasPneumaResources error: {ex.Message}");
            return false;
        }
    }

    public CardType GetCardTypeForArkheState(ArkheState state)
    {
        return state switch
        {
            ArkheState.Ousia => GetOusiaCardType(),
            ArkheState.Pneuma => GetPneumaCardType(),
            _ => base.Type
        };
    }

    public CardRarity GetCardRarityForArkheState(ArkheState state)
    {
        return state switch
        {
            ArkheState.Ousia => GetOusiaCardRarity(),
            ArkheState.Pneuma => GetPneumaCardRarity(),
            _ => base.Rarity
        };
    }

    public int GetEnergyCostForArkheState(ArkheState state)
    {
        return state switch
        {
            ArkheState.Ousia => GetOusiaEnergyCost(),
            ArkheState.Pneuma => GetPneumaEnergyCost(),
            _ => base.CanonicalEnergyCost
        };
    }

    public TargetType GetTargetTypeForArkheState(ArkheState state)
    {
        return state switch
        {
            ArkheState.Ousia => GetOusiaTargetType(),
            ArkheState.Pneuma => GetPneumaTargetType(),
            _ => base.TargetType
        };
    }

    public bool ForceNoneState
    {
        get => _forceNoneState;
        set
        {
            if (_forceNoneState != value)
            {
                _forceNoneState = value;
                UpdateArkheState();
            }
        }
    }

    public void UpdateArkheState()
    {
        try
        {
            if (Owner?.Creature == null)
            {
                CurrentArkheState = ArkheState.None;
                return;
            }

            if (_forceNoneState)
            {
                CurrentArkheState = ArkheState.None;
                return;
            }

            var creature = Owner.Creature;

            if (creature.HasPower<OusiaPower>())
            {
                CurrentArkheState = ArkheState.Ousia;
            }
            else if (creature.HasPower<PneumaPower>())
            {
                CurrentArkheState = ArkheState.Pneuma;
            }
            else
            {
                CurrentArkheState = ArkheState.None;
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[OusiaPneumaCard] UpdateArkheState error: {ex.Message}");
            CurrentArkheState = ArkheState.None;
        }
    }

    protected virtual void OnArkheStateChanged(ArkheState oldState, ArkheState newState)
    {
        try
        {
            GD.Print($"[OusiaPneumaCard] Arkhe state changed from {oldState} to {newState} for card {base.Id?.Entry ?? "UNKNOWN"}");
            
            RefreshCardVisuals();
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[OusiaPneumaCard] OnArkheStateChanged error: {ex.Message}");
        }
    }

    protected virtual void RefreshCardVisuals()
    {
        try
        {
            GD.Print($"[OusiaPneumaCard] Refreshing card visuals");
            
            var combatRoom = NCombatRoom.Instance;
            if (combatRoom == null)
                return;

            if (combatRoom.Ui == null || combatRoom.Ui.Hand == null)
                return;

            var cardHolder = combatRoom.Ui.Hand.ActiveHolders.FirstOrDefault(h => h.CardModel == this);
            if (cardHolder is NHandCardHolder handCardHolder && handCardHolder.CardNode != null)
            {
                var vfx = NCardTransformShineVfx.Create(handCardHolder.CardNode, this, null);
                if (vfx != null)
                {
                    TaskHelper.RunSafely(vfx.PlayAnimation(shortVersion: true));
                }
                handCardHolder.CardNode.Reload();
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[OusiaPneumaCard] RefreshCardVisuals error: {ex.Message}");
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        var creature = Owner.Creature;
        var dualEffectProvider = ArkheDualEffectHelper.GetFirstActiveProvider(creature);

        if (dualEffectProvider != null)
        {
            await OnChorusEffect(choiceContext, cardPlay);
            await dualEffectProvider.OnArkheCardPlayed(choiceContext, this);
            return;
        }

        switch (CurrentArkheState)
        {
            case ArkheState.Ousia:
                await OnOusiaEffect(choiceContext, cardPlay);
                break;
            case ArkheState.Pneuma:
                await OnPneumaEffect(choiceContext, cardPlay);
                break;
            default:
                await OnNoArkheEffect(choiceContext, cardPlay);
                break;
        }
    }

    public override void AfterCreated()
    {
        base.AfterCreated();
        UpdateArkheState();
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);
        
        if (power is OusiaPower || power is PneumaPower)
        {
            UpdateArkheState();
        }
    }

    protected abstract Task OnOusiaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay);

    protected abstract Task OnPneumaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay);

    protected virtual Task OnNoArkheEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnChorusEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return OnNoArkheEffect(choiceContext, cardPlay);
    }

    protected virtual CardType GetOusiaCardType() => base.Type;

    protected virtual CardType GetPneumaCardType() => base.Type;

    protected virtual CardRarity GetOusiaCardRarity() => base.Rarity;

    protected virtual CardRarity GetPneumaCardRarity() => base.Rarity;

    protected virtual int GetOusiaEnergyCost() => base.CanonicalEnergyCost;

    protected virtual int GetPneumaEnergyCost() => base.CanonicalEnergyCost;

    protected virtual TargetType GetOusiaTargetType() => base.TargetType;

    protected virtual TargetType GetPneumaTargetType() => base.TargetType;
}