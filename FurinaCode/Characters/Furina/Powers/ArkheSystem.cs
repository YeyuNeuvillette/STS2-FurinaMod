using Furina.Characters.Furina.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Furina.Characters.Furina.Powers;

public enum ArkheState
{
    None,
    Ousia,
    Pneuma
}

public interface IArkheDualEffectProvider
{
    bool ShouldApplyBothEffects { get; }
    Task OnArkheCardPlayed(PlayerChoiceContext choiceContext, CardModel cardSource);
}

public static class ArkheSystem
{
    public const string OusiaStorageKey = "Ousia";
    public const string PneumaStorageKey = "Pneuma";

    public static ArkheState GetState(Creature? creature, bool includeDualEffect = true)
    {
        if (creature == null || includeDualEffect && GetDualEffectProvider(creature) != null)
            return ArkheState.None;
        if (creature.HasPower<OusiaPower>())
            return ArkheState.Ousia;
        if (creature.HasPower<PneumaPower>())
            return ArkheState.Pneuma;
        return ArkheState.None;
    }

    public static bool IsArkhePower(PowerModel power) => power is OusiaPower or PneumaPower;

    public static string GetStorageKey(ArkheState state) => state switch
    {
        ArkheState.Ousia => OusiaStorageKey,
        ArkheState.Pneuma => PneumaStorageKey,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "None has no minion storage.")
    };

    public static ArkheState GetOpposite(ArkheState state) => state switch
    {
        ArkheState.Ousia => ArkheState.Pneuma,
        ArkheState.Pneuma => ArkheState.Ousia,
        _ => ArkheState.Ousia
    };

    public static Task SwitchState(PlayerChoiceContext choiceContext, Player player, CardModel? source) =>
        SetState(choiceContext, player, GetOpposite(GetState(player.Creature, includeDualEffect: false)), source);

    public static async Task SetState(
        PlayerChoiceContext choiceContext,
        Player player,
        ArkheState targetState,
        CardModel? source = null)
    {
        var creature = player.Creature;
        var currentState = GetState(creature, includeDualEffect: false);
        if (currentState == targetState)
            return;

        if (creature.HasPower<OusiaPower>())
            await PowerCmd.Remove<OusiaPower>(creature);
        if (creature.HasPower<PneumaPower>())
            await PowerCmd.Remove<PneumaPower>(creature);

        if (targetState == ArkheState.Ousia)
            await PowerCmd.Apply<OusiaPower>(choiceContext, creature, 1m, creature, source);
        else if (targetState == ArkheState.Pneuma)
            await PowerCmd.Apply<PneumaPower>(choiceContext, creature, 1m, creature, source);
    }

    public static IArkheDualEffectProvider? GetDualEffectProvider(Creature creature) =>
        creature.Powers.OfType<IArkheDualEffectProvider>().FirstOrDefault(provider => provider.ShouldApplyBothEffects);

    public static void RefreshHand(Creature creature)
    {
        var forceNone = GetDualEffectProvider(creature) != null;
        foreach (var card in creature.Player?.PlayerCombatState?.Hand.Cards.OfType<OusiaPneumaCard>() ?? [])
            card.ForceNoneState = forceNone;
    }

    public static void RefreshCard(Creature creature, CardModel card)
    {
        if (card is OusiaPneumaCard arkheCard)
            arkheCard.ForceNoneState = GetDualEffectProvider(creature) != null;
    }
}
