using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace Furina.Characters.Furina.Patches;

[Obsolete("Use Furina.Api.Rewards.RewardModifyContext instead.")]
public readonly record struct RewardModifyContext(
    Player Player,
    List<Reward> Rewards,
    AbstractRoom? Room
);

[Obsolete("Use Furina.Api.Rewards.RewardModifierDelegate instead.")]
public delegate void RewardModifierDelegate(in RewardModifyContext context);

[Obsolete("Use Furina.Api.Rewards.RewardModifierBus instead.")]
public static class RewardModifierBus
{
    private static readonly Dictionary<RewardModifierDelegate, global::Furina.Api.Rewards.RewardModifierDelegate> Bridges = new();

    public static void Register(RewardModifierDelegate modifier)
    {
        if (modifier == null || Bridges.ContainsKey(modifier))
            return;

        global::Furina.Api.Rewards.RewardModifierDelegate bridge = (in global::Furina.Api.Rewards.RewardModifyContext context) =>
        {
            var compatContext = new RewardModifyContext(context.Player, context.Rewards, context.Room);
            modifier(in compatContext);
        };

        Bridges[modifier] = bridge;
        global::Furina.Api.Rewards.RewardModifierBus.Register(bridge);
    }

    public static void Unregister(RewardModifierDelegate modifier)
    {
        if (!Bridges.Remove(modifier, out var bridge))
            return;

        global::Furina.Api.Rewards.RewardModifierBus.Unregister(bridge);
    }

    internal static void RunAll(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        global::Furina.Api.Rewards.RewardModifierBus.RunAll(player, rewards, room);
    }
}
