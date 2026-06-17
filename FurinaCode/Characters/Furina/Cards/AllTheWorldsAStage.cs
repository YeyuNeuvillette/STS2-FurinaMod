using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.CardPiles;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Base;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class AllTheWorldsAStage : FurinaCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword> { CardKeyword.Exhaust };

    public AllTheWorldsAStage()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.PlayerCombatState == null || CombatState == null)
            return;

        var cardCount = CurrentUpgradeLevel > 0 ? 4 : 3;

        var otherPools = ModelDb.AllCharacterCardPools
            .Where(p => p.Title != Furina.CharacterId)
            .ToList();

        var eligibleCards = new List<(CardPoolModel Pool, CardModel Card)>();
        foreach (var pool in otherPools)
        {
            var filtered = CardFactory.FilterForCombat(
                pool.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint));
            foreach (var card in filtered)
            {
                eligibleCards.Add((pool, card));
            }
        }

        var selectedCards = new List<CardModel>();
        var usedPools = new HashSet<CardPoolModel>();
        var rng = Owner.RunState.Rng.CombatCardGeneration;

        var shuffled = new List<(CardPoolModel Pool, CardModel Card)>(eligibleCards);
        rng.Shuffle(shuffled);

        foreach (var entry in shuffled)
        {
            if (selectedCards.Count >= cardCount)
                break;

            if (usedPools.Contains(entry.Pool))
                continue;

            usedPools.Add(entry.Pool);
            selectedCards.Add(entry.Card);
        }

        if (selectedCards.Count < cardCount)
        {
            var remaining = shuffled
                .Where(x => !selectedCards.Contains(x.Card))
                .ToList();
            rng.Shuffle(remaining);
            selectedCards.AddRange(remaining.Take(cardCount - selectedCards.Count).Select(x => x.Card));
        }

        var scriptPileType = ScriptPileRegistration.GetScriptPileType();

        foreach (var canonicalCard in selectedCards)
        {
            var card = CombatState.CreateCard(canonicalCard, Owner);
            await CardPileCmd.AddGeneratedCardToCombat(card, scriptPileType, Owner);
        }

        await EnsureScriptCardExists(choiceContext);
    }

    protected override void OnUpgrade()
    {
    }

    private async Task EnsureScriptCardExists(PlayerChoiceContext choiceContext)
    {
        if (Owner?.PlayerCombatState == null)
            return;

        var hand = Owner.PlayerCombatState.Hand;
        var drawPile = Owner.PlayerCombatState.DrawPile;
        var discardPile = Owner.PlayerCombatState.DiscardPile;

        var hasScript = hand.Cards.Any(c => c is Script)
                        || drawPile.Cards.Any(c => c is Script)
                        || discardPile.Cards.Any(c => c is Script);

        if (!hasScript)
        {
            var combatState = Owner.Creature.CombatState;
            if (combatState == null)
                return;

            var scriptCard = combatState.CreateCard<Script>(Owner);
            await CardPileCmd.AddGeneratedCardToCombat(scriptCard, PileType.Hand, Owner);
            ScriptPileRegistration.OnScriptCardCreated();

            if (scriptCard is Script script)
            {
                script.EnsureScriptPileSubscription();
            }
        }
    }
}