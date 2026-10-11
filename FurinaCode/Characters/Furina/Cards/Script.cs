using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Base;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Combat;
using STS2RitsuLib.Keywords;
using MegaCrit.Sts2.Core.Rooms;
using Furina.Characters.Furina.Powers;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(TokenCardPool))]
public sealed class Script : BaseCard
{
    private CardPile? _scriptPile;
    private bool _hasSubscribed = false;
    private readonly Dictionary<CardModel, int> _capturedCosts = new(ReferenceEqualityComparer.Instance);

    public override bool CanBeGeneratedInCombat => false;

    public override int MaxUpgradeLevel => 0;

    public Script()
        : base(0, CardType.Skill, CardRarity.Token, TargetType.None, shouldShowInCardLibrary: false)
    {
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword>
    {
        CardKeyword.Retain,
        CardKeyword.Exhaust,
        ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_SCRIPT")
    };

    protected override int CanonicalEnergyCost => 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.PlayerCombatState == null)
            return;

        var scriptPile = TropeSystem.GetPile(Owner);

        if (scriptPile.Cards.Count == 0)
            return;

        var cardsToPlay = scriptPile.Cards.AsEnumerable().Reverse().ToList();
        using (TropeSystem.BeginReplay())
        {
            foreach (var card in cardsToPlay)
            {
                if (CombatManager.Instance.IsOverOrEnding)
                    break;

                await CardCmd.AutoPlay(choiceContext, card, null, AutoPlayType.Default, skipXCapture: true, skipCardPileVisuals: false);
            }
        }
    }

    internal void BindToScriptPile()
    {
        if (Owner?.PlayerCombatState == null)
            return;

        if (_hasSubscribed)
            return;

        var scriptPile = TropeSystem.GetPile(Owner);
        
        if (scriptPile != _scriptPile)
        {
            if (_scriptPile != null)
                _scriptPile.ContentsChanged -= OnScriptPileContentsChanged;
            
            _scriptPile = scriptPile;
            
            _scriptPile.ContentsChanged += OnScriptPileContentsChanged;
            _hasSubscribed = true;
            OnScriptPileContentsChanged();
        }
    }

    private void OnScriptPileContentsChanged()
    {
        if (Owner?.PlayerCombatState == null)
            return;

        var scriptPile = TropeSystem.GetPile(Owner);

        // A card contributes the cost it had when it entered the Script pile.
        // Temporary cost modifiers may expire while the card waits there, but
        // that must not retroactively change the Script card's cost.
        var currentCards = scriptPile.Cards.ToHashSet(ReferenceEqualityComparer.Instance);
        foreach (var removedCard in _capturedCosts.Keys.Where(card => !currentCards.Contains(card)).ToList())
        {
            _capturedCosts.Remove(removedCard);
            // Script pricing is metadata owned by this pile, never a modifier
            // on the card. Drop every trace of it as soon as the card leaves.
            TropeSystem.ForgetCapturedScriptCost(removedCard);
        }

        foreach (var card in scriptPile.Cards)
        {
            if (!_capturedCosts.ContainsKey(card))
            {
                var cost = TropeSystem.TryTakeCapturedScriptCost(card, out var capturedCost)
                    ? capturedCost
                    : card.EnergyCost.GetWithModifiers(CostModifiers.All);
                _capturedCosts.Add(card, cost);
            }
        }

        var totalCost = scriptPile.Cards.Sum(card => _capturedCosts[card]);
        
        var reduction = Owner.Creature.GetPower<RevisionPower>()?.Amount ?? 0m;
        var costLimit = Math.Max(0, 3 - (int)reduction);
        var finalCost = Math.Min(totalCost, costLimit);
        
        EnergyCost.SetCustomBaseCost(finalCost);
    }

    internal static void RefreshAll(Player player)
    {
        if (player.PlayerCombatState == null)
            return;

        foreach (var script in player.PlayerCombatState.AllCards.OfType<Script>())
            script.OnScriptPileContentsChanged();
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (_scriptPile != null)
            _scriptPile.ContentsChanged -= OnScriptPileContentsChanged;
        _scriptPile = null;
        _hasSubscribed = false;
        _capturedCosts.Clear();
        await base.AfterCombatEnd(room);
    }
}
