using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Utils;

namespace Furina.Characters.Furina.Patches;

internal static class WaterAndJusticeMarker
{
    private const string LegacySavedReductionKey = "FurinaWaterAndJusticeReduction";
    private static readonly SavedAttachedState<CardModel, int> Reductions =
        new("FurinaWaterAndJusticeReduction", defaultValueFactory: () => 0);

    public static void AddReduction(CardModel card, int amount)
    {
        if (amount <= 0)
            return;

        Reductions.Update(card, current => current + amount);
        card.InvokeEnergyCostChanged();
    }

    public static int GetReduction(CardModel card)
    {
        var deckCard = card.DeckVersion ?? card;
        if (TryGetOwnReduction(deckCard, out var amount))
            return amount;

        return ReferenceEquals(deckCard, card) || !TryGetOwnReduction(card, out amount)
            ? 0
            : amount;
    }

    private static void RestoreReduction(CardModel card, int amount)
    {
        if (amount <= 0)
            return;

        Reductions[card] = amount;
        card.InvokeEnergyCostChanged();
    }

    public static bool TryGetOwnReduction(CardModel card, out int amount)
    {
        if (Reductions.TryGetValue(card, out var reduction) && reduction > 0)
        {
            amount = reduction;
            return true;
        }

        amount = 0;
        return false;
    }

    private static bool TryReadSavedReduction(SerializableCard save, out int amount)
    {
        amount = save.Props?.ints?
            .FirstOrDefault(prop => prop.name == LegacySavedReductionKey)
            .value ?? 0;
        return amount > 0;
    }

    // Compatibility for saves created before SavedAttachedState was adopted. The legacy
    // property is read once and is never written again, so future replay serialization only
    // sees the registered RitsuLib property name.
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.FromSerializable))]
    private static class CardFromSerializablePatch
    {
        private static void Postfix(SerializableCard save, CardModel __result)
        {
            if (TryReadSavedReduction(save, out var amount))
            {
                var current = Reductions.GetValueOrDefault(__result, 0);
                RestoreReduction(__result, System.Math.Max(current, amount));
            }
        }
    }
}
