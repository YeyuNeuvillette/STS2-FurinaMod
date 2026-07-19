using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace Furina.Api.Rewards;

public readonly record struct RewardModifyContext(
    Player Player,
    List<Reward> Rewards,
    AbstractRoom? Room
);

public delegate void RewardModifierDelegate(in RewardModifyContext context);

public static class RewardModifierBus
{
    private static readonly List<RewardModifierDelegate> Modifiers = new();

    public static int RegisteredCount => Modifiers.Count;

    public static IDisposable Subscribe(RewardModifierDelegate modifier)
    {
        Register(modifier);
        return new RewardModifierSubscription(modifier);
    }

    public static void Register(RewardModifierDelegate modifier)
    {
        if (modifier != null && !Modifiers.Contains(modifier))
            Modifiers.Add(modifier);
    }

    public static void Unregister(RewardModifierDelegate modifier)
    {
        Modifiers.Remove(modifier);
    }

    internal static void RunAll(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (Modifiers.Count == 0)
            return;

        var context = new RewardModifyContext(player, rewards, room);
        var snapshot = Modifiers.ToArray();
        foreach (var modifier in snapshot)
        {
            try
            {
                modifier(in context);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[Furina.RewardModifierBus] Modifier threw exception: {ex}");
            }
        }
    }

    private sealed class RewardModifierSubscription(RewardModifierDelegate modifier) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            Unregister(modifier);
        }
    }
}
