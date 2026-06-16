using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.CardPiles;
using STS2RitsuLib.Keywords;

namespace Furina.Characters.Base;

public abstract class TropeCard : BaseCard
{
    private static PileType? _scriptPileType;
    internal static bool _isPlayingFromScriptPile;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword>
    {
        ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_TROPE"),
        ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_SCRIPT")
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        base.AdditionalHoverTips.Concat(
            HoverTipFactory.FromCardWithCardHoverTips<Furina.Cards.Script>());

    protected TropeCard(
        int energyCost,
        CardType type,
        CardRarity rarity,
        TargetType targetType,
        bool shouldShowInCardLibrary = true)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override PileType GetResultPileTypeForCardPlay()
    {
        var result = PileType.None;
        
        if (IsDupe || Type == CardType.Power)
        {
            result = PileType.None;
        }
        else if (_isPlayingFromScriptPile)
        {
            result = PileType.Discard;
        }
        else
        {
            result = ScriptPileType;
        }
        
        GD.Print($"[TropeCard] GetResultPileTypeForCardPlay for {Id.Entry}: {result}, _isPlayingFromScriptPile: {_isPlayingFromScriptPile}");
        return result;
    }

    private async Task CheckAndGenerateScript(PlayerChoiceContext choiceContext)
    {
        if (Owner?.PlayerCombatState == null)
            return;

        var hand = Owner.PlayerCombatState.Hand;
        var drawPile = Owner.PlayerCombatState.DrawPile;
        var discardPile = Owner.PlayerCombatState.DiscardPile;

        var hasScriptInHand = hand.Cards.Any(c => c is Furina.Cards.Script);
        var hasScriptInDraw = drawPile.Cards.Any(c => c is Furina.Cards.Script);
        var hasScriptInDiscard = discardPile.Cards.Any(c => c is Furina.Cards.Script);

        if (!hasScriptInHand && !hasScriptInDraw && !hasScriptInDiscard)
        {
            var combatState = Owner.Creature.CombatState;
            if (combatState == null)
                return;
                
            var scriptCard = combatState.CreateCard<Furina.Cards.Script>(Owner);
            GD.Print($"[TropeCard] Generated Script card: {scriptCard.Id.Entry}, CombatState: {scriptCard.CombatState != null}, Owner: {scriptCard.Owner}");
            await CardPileCmd.AddGeneratedCardToCombat(scriptCard, PileType.Hand, Owner);
            GD.Print($"[TropeCard] Script card added to hand, Pile: {scriptCard.Pile?.Type}, CombatState: {scriptCard.CombatState != null}");
            ScriptPileRegistration.OnScriptCardCreated();
            
            if (scriptCard is Furina.Cards.Script script)
            {
                GD.Print($"[TropeCard] Triggering script energy cost update");
                script.EnsureScriptPileSubscription();
            }
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        GD.Print($"[TropeCard] OnPlay called for {Id.Entry}, _isPlayingFromScriptPile: {_isPlayingFromScriptPile}");
        
        if (!_isPlayingFromScriptPile)
        {
            GD.Print($"[TropeCard] Generating script for {Id.Entry}");
            await CheckAndGenerateScript(choiceContext);
        }
        else
        {
            GD.Print($"[TropeCard] Skipping script generation for {Id.Entry} (playing from script pile)");
        }
    }

    private static PileType ScriptPileType
    {
        get
        {
            if (!_scriptPileType.HasValue)
            {
                if (ModCardPileRegistry.TryGet("FURINA_CARDPILE_SCRIPT", out var definition))
                {
                    _scriptPileType = definition.PileType;
                }
                else
                {
                    _scriptPileType = PileType.Discard;
                }
            }
            return _scriptPileType.Value;
        }
    }

    public static IDisposable PlayFromScriptPile()
    {
        _isPlayingFromScriptPile = true;
        return new ScriptPilePlayScope();
    }

    private class ScriptPilePlayScope : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _isPlayingFromScriptPile = false;
            }
        }
    }
}