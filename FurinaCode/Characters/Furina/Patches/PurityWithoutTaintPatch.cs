using HarmonyLib;
using Furina.Characters.Furina.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Furina.Characters.Furina.Patches;

[HarmonyPatch]
internal static class PurityWithoutTaintPatch
{
    [HarmonyPatch(
        typeof(CardPileCmd),
        nameof(CardPileCmd.AddGeneratedCardToCombat),
        typeof(CardModel),
        typeof(PileType),
        typeof(Player),
        typeof(CardPilePosition))]
    [HarmonyPrefix]
    private static bool BlockGeneratedCard(
        CardModel card,
        Player creator,
        ref Task<CardPileAddResult> __result)
    {
        if (!ShouldBlock(card, creator))
            return true;

        __result = Task.FromResult(default(CardPileAddResult));
        return false;
    }

    [HarmonyPatch(
        typeof(CardPileCmd),
        nameof(CardPileCmd.AddGeneratedCardsToCombat),
        typeof(IEnumerable<CardModel>),
        typeof(PileType),
        typeof(Player),
        typeof(CardPilePosition))]
    [HarmonyPrefix]
    private static bool BlockGeneratedCards(
        ref IEnumerable<CardModel> cards,
        Player creator,
        ref Task<IReadOnlyList<CardPileAddResult>> __result)
    {
        var allowedCards = cards.Where(card => !ShouldBlock(card, creator)).ToArray();
        if (allowedCards.Length > 0)
        {
            cards = allowedCards;
            return true;
        }

        __result = Task.FromResult<IReadOnlyList<CardPileAddResult>>([]);
        return false;
    }

    private static bool ShouldBlock(CardModel card, Player creator)
    {
        var owner = card.Owner ?? creator;
        return owner.Creature.GetPower<PurityWithoutTaintPower>() != null;
    }
}
