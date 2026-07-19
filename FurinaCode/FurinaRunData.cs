using System.Collections.Generic;
using STS2RitsuLib;
using STS2RitsuLib.RunData;

namespace Furina;

public sealed class SufferingHolySonData
{
    public int BossMaxHpReduction { get; set; }
}

public sealed class CardCostReductionData
{
    public Dictionary<string, int> ReducedCards { get; set; } = new();
}

public static class FurinaRunData
{
    public static PlayerRunSavedData<SufferingHolySonData> SufferingHolySon { get; private set; } = null!;
    public static PlayerRunSavedData<CardCostReductionData> CardCostReductions { get; private set; } = null!;

    public static void Register()
    {
        var store = RitsuLibFramework.GetRunSavedDataStore(MainFile.ModId);

        SufferingHolySon = store.RegisterPerPlayer<SufferingHolySonData>(
            key: "SufferingHolySon",
            defaultFactory: () => new SufferingHolySonData());

        CardCostReductions = store.RegisterPerPlayer<CardCostReductionData>(
            key: "CardCostReductions",
            defaultFactory: () => new CardCostReductionData());
    }
}
