using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Furina.Characters.Furina.Actions;
using Furina.Characters.Furina.Minions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MinionLib.Commands;
using MinionLib.Minion;
using MegaCrit.Sts2.Core.Entities.Powers;
using Furina.Characters.Furina.Powers;

namespace Furina.Characters.Furina;

public static class InvitationManager
{
    public const string OusiaKey = "Ousia";
    public const string PneumaKey = "Pneuma";

    private static readonly Type[] SalonMemberTypes =
    [
        typeof(GentilhommeUsher),
        typeof(MademoiselleCrabaletta),
        typeof(SurintendanteChevalmarin)
    ];

    private static readonly HashSet<Type> FurinaMinionTypes =
    [
        .. SalonMemberTypes,
        typeof(SingerOfManyWaters)
    ];

    private static readonly Dictionary<ulong, int> TotalInvitations = new();
    private static readonly Dictionary<(ulong playerId, string state), int> RealizedInvitations = new();
    private static readonly HashSet<uint?> StateSwitchRemovals = [];
    private static readonly HashSet<(ulong playerId, string state)> FillingStates = [];

    public static void ClearAll()
    {
        TotalInvitations.Clear();
        RealizedInvitations.Clear();
        StateSwitchRemovals.Clear();
        FillingStates.Clear();
    }

    public static void RegisterStateSwitchRemoval(Creature creature)
    {
        StateSwitchRemovals.Add(creature.CombatId);
    }

    public static bool IsFurinaMinion(Creature creature) =>
        creature.Monster != null && FurinaMinionTypes.Contains(creature.Monster.GetType());

    public static bool IsSalonMember(Creature creature) =>
        creature.Monster != null && SalonMemberTypes.Contains(creature.Monster.GetType());

    public static IReadOnlyList<Type> GetSalonMemberTypes() => SalonMemberTypes;

    public static Type SelectRandomSalonMember(Player player) =>
        player.RunState.Rng.CombatCardSelection.NextItem(SalonMemberTypes)!;

    public static async Task SwitchArkhe(
        PlayerChoiceContext choiceContext,
        Player player,
        CardModel? source)
    {
        var creature = player.Creature;
        if (creature.HasPower<OusiaPower>())
        {
            await PowerCmd.Remove<OusiaPower>(creature);
            await PowerCmd.Apply<PneumaPower>(choiceContext, creature, 1m, creature, source);
        }
        else
        {
            if (creature.HasPower<PneumaPower>())
                await PowerCmd.Remove<PneumaPower>(creature);
            await PowerCmd.Apply<OusiaPower>(choiceContext, creature, 1m, creature, source);
        }
    }

    public static IReadOnlySet<ModelId> GetOnSummonPowerIds(Type minionType)
    {
        if (minionType == typeof(GentilhommeUsher))
            return new HashSet<ModelId> { ModelDb.GetId(typeof(HydroBarrage)), ModelDb.GetId(typeof(SalonSolitairePower)) };
        if (minionType == typeof(MademoiselleCrabaletta))
            return new HashSet<ModelId> { ModelDb.GetId(typeof(CrabShield)), ModelDb.GetId(typeof(SalonSolitairePower)) };
        if (minionType == typeof(SurintendanteChevalmarin))
            return new HashSet<ModelId> { ModelDb.GetId(typeof(PureWaterNote)), ModelDb.GetId(typeof(SalonSolitairePower)) };
        if (minionType == typeof(SingerOfManyWaters))
            return new HashSet<ModelId> { ModelDb.GetId(typeof(JoyfulSinging)) };
        return new HashSet<ModelId>();
    }

    public static async Task<Creature?> SummonMinion(
        PlayerChoiceContext choiceContext,
        Player player,
        CardModel? source,
        Type minionType,
        decimal maxHp)
    {
        var options = new MinionSummonOptions(MaxHp: maxHp, Source: source, Position: MinionPosition.Front);
        if (minionType == typeof(GentilhommeUsher))
            return await MinionCmd.AddMinion<GentilhommeUsher>(choiceContext, player, options);
        if (minionType == typeof(MademoiselleCrabaletta))
            return await MinionCmd.AddMinion<MademoiselleCrabaletta>(choiceContext, player, options);
        if (minionType == typeof(SurintendanteChevalmarin))
            return await MinionCmd.AddMinion<SurintendanteChevalmarin>(choiceContext, player, options);
        if (minionType == typeof(SingerOfManyWaters))
            return await MinionCmd.AddMinion<SingerOfManyWaters>(choiceContext, player, options);

        throw new ArgumentOutOfRangeException(nameof(minionType), minionType, "Not a Furina minion type.");
    }

    public static async Task InviteOusia(PlayerChoiceContext choiceContext, Player player, CardModel? source, Type? minionType = null, bool isNewInvitation = true)
    {
        minionType ??= SelectRandomSalonMember(player);
        var existing = player.PlayerCombatState?.Pets.FirstOrDefault(p => p.Monster?.GetType() == minionType);

        if (existing == null)
            await SummonSalonMember(choiceContext, player, source, minionType);
        else
            await EnhanceSalonMember(choiceContext, player, source, existing, minionType);

        RecordInvitation(player, OusiaKey, isNewInvitation);
    }

    public static async Task InvitePneuma(PlayerChoiceContext choiceContext, Player player, CardModel? source, bool isNewInvitation = true)
    {
        var singer = player.PlayerCombatState?.GetPet<SingerOfManyWaters>();
        if (singer == null)
        {
            await SummonMinion(choiceContext, player, source, typeof(SingerOfManyWaters), 3m);
        }
        else
        {
            await IncreaseHp(singer, 3m);
        }

        RecordInvitation(player, PneumaKey, isNewInvitation);
    }

    public static async Task FillMissingInvitations(PlayerChoiceContext choiceContext, Player player, CardModel? source, string state)
    {
        var key = (player.NetId, state);
        if (!FillingStates.Add(key))
            return;

        try
        {
            var total = TotalInvitations.GetValueOrDefault(player.NetId);
            var realized = RealizedInvitations.GetValueOrDefault(key);
            var missing = Math.Max(0, total - realized);

            for (var i = 0; i < missing; i++)
            {
                if (state == OusiaKey)
                    await InviteOusia(choiceContext, player, source, isNewInvitation: false);
                else
                    await InvitePneuma(choiceContext, player, source, isNewInvitation: false);
            }
        }
        finally
        {
            FillingStates.Remove(key);
        }
    }

    public static bool OnMinionDeath(Creature creature)
    {
        if (StateSwitchRemovals.Remove(creature.CombatId))
            return false;

        var player = creature.PetOwner;
        if (player == null || creature.Monster == null)
            return false;

        var type = creature.Monster.GetType();
        var state = type == typeof(SingerOfManyWaters) ? PneumaKey : OusiaKey;
        if (state == OusiaKey && !SalonMemberTypes.Contains(type))
            return false;

        var contribution = GetContribution(creature, state);
        if (contribution <= 0)
            return false;

        var total = TotalInvitations.GetValueOrDefault(player.NetId);
        TotalInvitations[player.NetId] = Math.Max(0, total - contribution);

        var activeKey = (player.NetId, state);
        RealizedInvitations[activeKey] = Math.Max(0, RealizedInvitations.GetValueOrDefault(activeKey) - contribution);

        var otherState = state == OusiaKey ? PneumaKey : OusiaKey;
        var otherKey = (player.NetId, otherState);
        RealizedInvitations[otherKey] = Math.Max(0, RealizedInvitations.GetValueOrDefault(otherKey) - contribution);
        MinionStorage.ReduceStoredInvitations(player, otherState, contribution);
        return true;
    }

    private static void RecordInvitation(Player player, string state, bool isNewInvitation)
    {
        if (isNewInvitation)
            TotalInvitations[player.NetId] = TotalInvitations.GetValueOrDefault(player.NetId) + 1;
        var key = (player.NetId, state);
        RealizedInvitations[key] = RealizedInvitations.GetValueOrDefault(key) + 1;
    }

    private static int GetContribution(Creature creature, string state)
    {
        if (state == PneumaKey)
            return creature.MaxHp < 3m ? 0 : 1 + (int)((creature.MaxHp - 3m) / 3m);

        return Math.Max(1, (int)(creature.MaxHp / 3m));
    }

    private static async Task SummonSalonMember(PlayerChoiceContext choiceContext, Player player, CardModel? source, Type minionType)
    {
        if (!SalonMemberTypes.Contains(minionType))
            throw new ArgumentOutOfRangeException(nameof(minionType), minionType, "Not a salon member type.");

        await SummonMinion(choiceContext, player, source, minionType, 3m);
    }

    private static async Task EnhanceSalonMember(PlayerChoiceContext choiceContext, Player player, CardModel? source, Creature minion, Type minionType)
    {
        await IncreaseHp(minion, 3m);

        if (minionType == typeof(GentilhommeUsher))
        {
            await PowerCmd.Apply<HydroBarrageEnhancement>(choiceContext, minion, 1m, player.Creature, source);
            minion.GetPower<HydroBarrage>()?.UpdateDamage();
        }
        else if (minionType == typeof(MademoiselleCrabaletta))
        {
            await PowerCmd.Apply<CrabShieldEnhancement>(choiceContext, minion, 1m, player.Creature, source);
            minion.GetPower<CrabShield>()?.UpdateBlock();
        }
        else if (minionType == typeof(SurintendanteChevalmarin))
        {
            await PowerCmd.Apply<PureWaterNoteEnhancement>(choiceContext, minion, 1m, player.Creature, source);
            minion.GetPower<PureWaterNote>()?.UpdateDraw();
        }
    }

    private static async Task IncreaseHp(Creature creature, decimal amount)
    {
        var oldMaxHp = creature.MaxHp;
        var newMaxHp = oldMaxHp + amount;
        var newCurrentHp = Math.Min(creature.CurrentHp + amount, newMaxHp);
        await CreatureCmd.SetMaxHp(creature, newMaxHp);
        await CreatureCmd.SetCurrentHp(creature, newCurrentHp);

    }
}
