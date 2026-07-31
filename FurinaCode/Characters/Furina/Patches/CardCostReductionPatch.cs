using System.Reflection;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace Furina.Characters.Furina.Patches;

[HarmonyPatch]
public static class CardCostReductionPatch
{
    private static readonly FieldInfo? CardField = AccessTools.Field(typeof(CardEnergyCost), "_card");

    private static MethodBase? TargetMethod()
    {
        return AccessTools.Method(typeof(CardEnergyCost), "GetWithModifiers");
    }

    private static void Postfix(CardEnergyCost __instance, ref int __result)
    {
        if (__result <= 0)
            return;

        var card = CardField?.GetValue(__instance) as CardModel;
        if (card == null || card.IsCanonical)
            return;

        var owner = card.Owner;
        if (owner == null)
            return;

        if (owner.RunState is not RunState runState)
            return;

        var reduction = WaterAndJusticeMarker.GetReduction(card);
        if (reduction <= 0
            && FurinaRunData.CardCostReductions.TryGet(runState, owner.NetId, out var data)
            && data.ReducedCards.TryGetValue(card.Id.Entry, out var legacyReduction))
        {
            var deckCard = card.DeckVersion ?? card;
            if (owner.Deck.Cards.Count(deckCardInDeck => deckCardInDeck.Id.Entry == deckCard.Id.Entry) == 1)
                reduction = legacyReduction;
        }

        if (reduction > 0)
        {
            __result = System.Math.Max(0, __result - reduction);
        }
    }
}
