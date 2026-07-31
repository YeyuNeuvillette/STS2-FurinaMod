using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Furina.Characters.Furina.Patches;

internal static class WaterAndJusticeMarker
{
    private const string SavedReductionKey = "FurinaWaterAndJusticeReduction";
    private static readonly ConditionalWeakTable<CardModel, ReductionState> Reductions = new();

    public static void AddReduction(CardModel card, int amount)
    {
        if (amount <= 0)
            return;

        var state = Reductions.GetValue(card, _ => new ReductionState());
        state.Amount += amount;
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

        Reductions.Remove(card);
        Reductions.Add(card, new ReductionState { Amount = amount });
        card.InvokeEnergyCostChanged();
    }

    public static bool TryGetOwnReduction(CardModel card, out int amount)
    {
        if (Reductions.TryGetValue(card, out var state) && state.Amount > 0)
        {
            amount = state.Amount;
            return true;
        }

        amount = 0;
        return false;
    }

    private static bool TryReadSavedReduction(SerializableCard save, out int amount)
    {
        amount = save.Props?.ints?
            .FirstOrDefault(prop => prop.name == SavedReductionKey)
            .value ?? 0;
        return amount > 0;
    }

    private static void WriteSavedReduction(SerializableCard save, int amount)
    {
        if (amount <= 0)
            return;

        save.Props ??= new SavedProperties();
        save.Props.ints ??= new List<SavedProperties.SavedProperty<int>>();

        var savedProperty = new SavedProperties.SavedProperty<int>(SavedReductionKey, amount);
        var existingIndex = save.Props.ints.FindIndex(prop => prop.name == SavedReductionKey);
        if (existingIndex >= 0)
            save.Props.ints[existingIndex] = savedProperty;
        else
            save.Props.ints.Add(savedProperty);
    }

    private sealed class ReductionState
    {
        public int Amount { get; set; }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.ToSerializable))]
    private static class CardToSerializablePatch
    {
        private static void Postfix(CardModel __instance, ref SerializableCard __result)
        {
            if (TryGetOwnReduction(__instance, out var amount))
                WriteSavedReduction(__result, amount);
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.FromSerializable))]
    private static class CardFromSerializablePatch
    {
        private static void Postfix(SerializableCard save, CardModel __result)
        {
            if (TryReadSavedReduction(save, out var amount))
                RestoreReduction(__result, amount);
        }
    }
}
