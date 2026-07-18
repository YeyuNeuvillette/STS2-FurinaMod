using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Furina.Characters.Furina;

/// <summary>
/// Transfers the total current HP between Arkhe forms. Damage is never shared
/// while a form remains active; redistribution only happens during a switch.
/// </summary>
public static class HpSyncManager
{
    private static readonly Dictionary<(ulong playerId, string state), decimal> StoredHpPools = [];

    public static void ClearAll() => StoredHpPools.Clear();

    public static void StoreHpPool(Player player, string state)
    {
        var hpPool = player.PlayerCombatState?.Pets
            .Where(pet => pet.IsAlive && InvitationManager.IsFurinaMinion(pet))
            .Sum(pet => pet.CurrentHp) ?? 0m;

        StoredHpPools[(player.NetId, state)] = hpPool;
        GD.Print($"[HpSyncManager] Stored {state} HP pool: {hpPool}");
    }

    public static bool TryGetStoredHpPool(Player player, string state, out decimal hpPool) =>
        StoredHpPools.TryGetValue((player.NetId, state), out hpPool);

    public static IReadOnlyDictionary<Creature, decimal> CalculateHpDistribution(
        Player player,
        IReadOnlyCollection<Creature> minions,
        string state)
    {
        var hpPool = StoredHpPools.GetValueOrDefault((player.NetId, state));
        return CalculateHpDistribution(minions, hpPool);
    }

    public static IReadOnlyDictionary<Creature, decimal> CalculateHpDistribution(
        IReadOnlyCollection<Creature> minions,
        decimal hpPool)
    {
        var ordered = minions.OrderBy(minion => minion.CombatId).ToList();
        var result = ordered.ToDictionary(minion => minion, _ => 0m);
        var remaining = Math.Clamp(Math.Floor(hpPool), 0m, ordered.Sum(minion => minion.MaxHp));
        var eligible = ordered.ToList();

        while (remaining > 0m && eligible.Count > 0)
        {
            var share = Math.Max(1m, Math.Floor(remaining / eligible.Count));
            var progressed = false;

            foreach (var minion in eligible.ToList())
            {
                if (remaining <= 0m)
                    break;

                var capacity = minion.MaxHp - result[minion];
                if (capacity <= 0m)
                {
                    eligible.Remove(minion);
                    continue;
                }

                var amount = Math.Min(remaining, Math.Min(share, capacity));
                result[minion] += amount;
                remaining -= amount;
                progressed |= amount > 0m;

                if (result[minion] >= minion.MaxHp)
                    eligible.Remove(minion);
            }

            if (!progressed)
                break;
        }

        GD.Print($"[HpSyncManager] Distributed HP pool {hpPool}: {string.Join(", ", result.Select(pair => $"{pair.Key.Name}={pair.Value}"))}");
        return result;
    }
}
