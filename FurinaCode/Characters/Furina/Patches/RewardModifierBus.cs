using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Rewards;

namespace Furina.Characters.Furina.Patches;

public readonly record struct RewardModifyContext(
    Player Player,
    List<Reward> Rewards,
    AbstractRoom? Room
);

public delegate void RewardModifierDelegate(in RewardModifyContext context);

public static class RewardModifierBus
{
    private static readonly List<RewardModifierDelegate> _modifiers = new();

    public static void Register(RewardModifierDelegate modifier)
    {
        if (modifier != null && !_modifiers.Contains(modifier))
            _modifiers.Add(modifier);
    }

    public static void Unregister(RewardModifierDelegate modifier)
    {
        _modifiers.Remove(modifier);
    }

    internal static void RunAll(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (_modifiers.Count == 0)
            return;

        var context = new RewardModifyContext(player, rewards, room);
        for (int i = 0; i < _modifiers.Count; i++)
        {
            try
            {
                _modifiers[i](in context);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[RewardModifierBus] Modifier threw exception: {ex}");
            }
        }
    }
}