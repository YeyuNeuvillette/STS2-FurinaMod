using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop;

namespace Furina.Characters.Furina.Powers;

[ModInterop("Neuvillette", "Neuvillette.Characters.Neuvillette.Powers.AgilityPowerApi")]
public static class NeuvilletteAgilityInterop
{
    public static bool IsReady => false;
    public static PowerModel GetModel() => null!;
}

public static class AgilityPowerRef
{
    public static bool IsNeuvilletteLoaded => NeuvilletteAgilityInterop.IsReady;

    public static PowerModel GetModel()
    {
        if (IsNeuvilletteLoaded)
            return NeuvilletteAgilityInterop.GetModel();

        return ModelDb.Power<AgilityPower>();
    }

    public static async Task Apply(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (IsNeuvilletteLoaded)
        {
            var mutable = GetModel().ToMutable();
            await PowerCmd.Apply(choiceContext, mutable, target, amount, applier, cardSource);
            return;
        }

        await PowerCmd.Apply<AgilityPower>(choiceContext, target, amount, applier, cardSource);
    }

    public static IHoverTip HoverTip()
    {
        if (IsNeuvilletteLoaded)
            return HoverTipFactory.FromPower(GetModel());

        return HoverTipFactory.FromPower<AgilityPower>();
    }
}