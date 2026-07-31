using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Furina.Characters.Furina;
using Furina.Characters.Furina.Actions;
using Furina.Characters.Furina.Minions;
using Furina.Characters.Furina.Powers;
using Furina.RitsuAdapters;
using MinionLib.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace Furina;

public static class MinionStorage
{
    private sealed class MinionState
    {
        public Type MinionType { get; set; } = null!;
        public decimal MaxHp { get; set; }
        public decimal CurrentHp { get; set; }
        public List<PowerState> Powers { get; set; } = new();
        public int StoredTurnCount { get; set; }
    }

    private sealed class PowerState
    {
        public Type PowerType { get; set; } = null!;
        public decimal Amount { get; set; }
        public Dictionary<string, decimal> DynamicVarValues { get; set; } = new();
    }

    private static readonly Dictionary<(ulong netId, string key), List<MinionState>> StoredMinions = new();
    private static readonly HashSet<(ulong netId, string key)> RestoringKeys = [];
    private static readonly HashSet<ulong> SalonSolitaireUsed = [];
    private static int _playerTurnCount;

    public static void ClearAll()
    {
        StoredMinions.Clear();
        RestoringKeys.Clear();
        SalonSolitaireUsed.Clear();
        _playerTurnCount = 0;
        HpSyncManager.ClearAll();
    }

    public static void ResetSalonSolitaire()
    {
        SalonSolitaireUsed.Clear();
    }

    public static void IncrementTurnCount()
    {
        _playerTurnCount++;
    }

    public static bool IsSalonSolitaireUsed(MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        return SalonSolitaireUsed.Contains(player.NetId);
    }

    public static void MarkSalonSolitaireUsed(MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        SalonSolitaireUsed.Add(player.NetId);
    }

    public static void StoreMinions(MegaCrit.Sts2.Core.Entities.Players.Player player, string storageKey)
    {
        var dictKey = (player.NetId, storageKey);
        StoredMinions.Remove(dictKey);
        GD.Print($"[MinionStorage] StoreMinions called for key={storageKey}");

        if (player.PlayerCombatState?.Pets == null)
        {
            GD.Print("[MinionStorage] No pets found");
            return;
        }

        GD.Print($"[MinionStorage] Found {player.PlayerCombatState.Pets.Count} pets");
        var minions = new List<MinionState>();

        foreach (var pet in player.PlayerCombatState.Pets.ToList())
        {
            if (pet.Monster == null || !pet.IsAlive)
                continue;

            if (!InvitationManager.IsFurinaMinion(pet))
            {
                GD.Print($"[MinionStorage] Skipping non-Furina pet: {pet.Name} ({pet.Monster.GetType().Name})");
                continue;
            }

            var minionState = new MinionState
            {
                MinionType = pet.Monster.GetType(),
                MaxHp = pet.MaxHp,
                CurrentHp = pet.CurrentHp,
                StoredTurnCount = _playerTurnCount
            };

            foreach (var power in pet.Powers.ToList())
            {
                var powerState = new PowerState
                {
                    PowerType = power.GetType(),
                    Amount = power.Amount
                };

                if (power is ModPowerTemplate modPower)
                {
                    foreach (var dynamicVar in modPower.DynamicVars)
                        powerState.DynamicVarValues[dynamicVar.Key] = dynamicVar.Value.BaseValue;
                }
                else if (power is ModActionTemplate modAction)
                {
                    foreach (var dynamicVar in modAction.DynamicVars)
                        powerState.DynamicVarValues[dynamicVar.Key] = dynamicVar.Value.BaseValue;
                }
                else if (power is MinionLib.Action.ActionModel actionModel)
                {
                    foreach (var dynamicVar in actionModel.DynamicVars)
                        powerState.DynamicVarValues[dynamicVar.Key] = dynamicVar.Value.BaseValue;
                }

                minionState.Powers.Add(powerState);
            }

            minions.Add(minionState);
        }

        StoredMinions[dictKey] = minions;
        GD.Print($"[MinionStorage] Stored {minions.Count} minions for key={storageKey}");
    }

    public static bool HasStoredMinions(MegaCrit.Sts2.Core.Entities.Players.Player player, string storageKey)
    {
        var dictKey = (player.NetId, storageKey);
        return StoredMinions.TryGetValue(dictKey, out var list) && list.Count > 0;
    }

    public static void ReduceStoredInvitations(MegaCrit.Sts2.Core.Entities.Players.Player player, string storageKey, int amount)
    {
        if (amount <= 0 || !StoredMinions.TryGetValue((player.NetId, storageKey), out var minions))
            return;

        if (storageKey == ArkheSystem.PneumaStorageKey)
        {
            var singer = minions.FirstOrDefault(m => m.MinionType == typeof(SingerOfManyWaters));
            if (singer == null)
                return;

            var contribution = singer.MaxHp < 3m ? 0 : 1 + (int)((singer.MaxHp - 3m) / 3m);
            if (amount >= contribution)
            {
                minions.Remove(singer);
                return;
            }

            singer.MaxHp -= amount * 3m;
            singer.CurrentHp = Math.Min(singer.CurrentHp, singer.MaxHp);
        }
        else
        {
            minions.Clear();
        }
    }

    public static async Task RestoreMinions(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Entities.Players.Player player,
        CardModel? source,
        string storageKey)
    {
        var dictKey = (player.NetId, storageKey);

        if (RestoringKeys.Contains(dictKey))
        {
            GD.Print("[MinionStorage] Already restoring minions, skipping");
            return;
        }

        if (!StoredMinions.TryGetValue(dictKey, out var minionsToRestore) || minionsToRestore.Count == 0)
        {
            GD.Print($"[MinionStorage] No stored minions found for key={storageKey}");
            return;
        }

        StoredMinions.Remove(dictKey);
        RestoringKeys.Add(dictKey);
        GD.Print($"[MinionStorage] RestoreMinions called for key={storageKey}, count={minionsToRestore.Count}");

        try
        {
            var restoredMinions = new List<Creature>();
            var minionStateMap = new Dictionary<uint, MinionState>();
            var shouldReapplyEveryoneOutAfterRestore = false;

            foreach (var minionState in minionsToRestore)
            {
                GD.Print($"[MinionStorage] Restoring minion: Type={minionState.MinionType.Name}, MaxHp={minionState.MaxHp}, CurrentHp={minionState.CurrentHp}, Powers={minionState.Powers.Count}");
                foreach (var ps in minionState.Powers)
                {
                    GD.Print($"[MinionStorage]   Stored power: {ps.PowerType.Name}, Amount={ps.Amount}, DynamicVars={ps.DynamicVarValues.Count}");
                    foreach (var dv in ps.DynamicVarValues)
                        GD.Print($"[MinionStorage]     DynamicVar: {dv.Key}={dv.Value}");
                }

                var minion = await InvitationManager.SummonMinion(
                    choiceContext, player, source, minionState.MinionType, minionState.MaxHp);

                GD.Print($"[MinionStorage] AddMinion result: {(minion != null ? $"success, Name={minion.Name}, CombatId={minion.CombatId}" : "FAILED")}");

                if (minion != null)
                {
                    GD.Print($"[MinionStorage] Minion after summon - IsAlive={minion.IsAlive}, MaxHp={minion.MaxHp}, CurrentHp={minion.CurrentHp}");
                    GD.Print("[MinionStorage] Minion powers after summon (before restore):");
                    foreach (var p in minion.Powers)
                        GD.Print($"[MinionStorage]   {p.Id.Entry}, Type={p.GetType().Name}, Amount={p.Amount}");

                    await CreatureCmd.SetMaxHp(minion, minionState.MaxHp);

                    restoredMinions.Add(minion);
                    if (minion.CombatId.HasValue)
                        minionStateMap[minion.CombatId.Value] = minionState;
                }
                else
                {
                    GD.PrintErr($"[MinionStorage] Failed to summon minion: {minionState.MinionType.Name}");
                }
            }

            var hpDistribution = HpSyncManager.CalculateHpDistribution(player, restoredMinions, storageKey);

            foreach (var minion in restoredMinions)
            {
                if (!minion.CombatId.HasValue || !minionStateMap.TryGetValue(minion.CombatId.Value, out var minionState))
                    continue;

                var currentHp = hpDistribution.TryGetValue(minion, out var hp)
                    ? hp
                    : minionState.CurrentHp;
                currentHp = Math.Max(0m, Math.Min(currentHp, minion.MaxHp));

                GD.Print($"[MinionStorage] Setting CurrentHp for {minion.Name}: {currentHp} (stored was {minionState.CurrentHp})");
                await CreatureCmd.SetCurrentHp(minion, currentHp);

                var onSummonPowerTypes = InvitationManager.GetOnSummonPowerIds(minion.Monster!.GetType());

                GD.Print($"[MinionStorage] onSummonPowerTypes for {minion.Monster?.GetType().Name ?? "null"}: {string.Join(", ", onSummonPowerTypes.Select(id => id.Entry))}");

                foreach (var powerState in minionState.Powers)
                {
                    var powerModelId = ModelDb.GetId(powerState.PowerType);
                    GD.Print($"[MinionStorage] Processing power: {powerState.PowerType.Name} (ModelId={powerModelId.Entry}), Amount={powerState.Amount}");

                    if (powerState.PowerType == typeof(MinionGuardianPower))
                    {
                        GD.Print("[MinionStorage]   Skipping MinionGuardianPower (managed by SalonSolitaireGameAction)");
                        continue;
                    }

                    if (onSummonPowerTypes.Contains(powerModelId))
                    {
                        var existingPower = minion.GetPower(powerModelId);
                        GD.Print($"[MinionStorage]   Power is in onSummonPowerTypes. existingPower={existingPower != null}, existingAmount={existingPower?.Amount}");

                        if (existingPower != null)
                        {
                            var diff = powerState.Amount - existingPower.Amount;
                            GD.Print($"[MinionStorage]   Modifying amount: diff={diff}");
                            if (diff != 0m)
                            {
                                await PowerCmd.ModifyAmount(choiceContext, existingPower, diff, player.Creature, source);
                            }
                            continue;
                        }

                        GD.Print($"[MinionStorage]   WARNING: onSummonPowerType {powerState.PowerType.Name} not found on minion! Will apply via non-generic path.");
                    }

                    try
                    {
                        var powerModel = ModelDb.GetById<PowerModel>(ModelDb.GetId(powerState.PowerType)).ToMutable();
                        GD.Print($"[MinionStorage]   Applying via PowerCmd.Apply (non-generic): powerModel={powerModel?.GetType().Name}, InstanceType={powerModel?.InstanceType}");

                        if (powerModel != null)
                        {
                            var existingForStacking = PowerCmd.FindExistingInstanceForStacking(powerModel, minion, player.Creature);
                            GD.Print($"[MinionStorage]   FindExistingInstanceForStacking: {(existingForStacking != null ? $"found {existingForStacking.Id.Entry} Amount={existingForStacking.Amount}" : "null (will create new)")}");

                            await PowerCmd.Apply(choiceContext, powerModel, minion, powerState.Amount, player.Creature, source, silent: true);
                            GD.Print("[MinionStorage]   PowerCmd.Apply completed successfully");
                        }
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr($"[MinionStorage] Error applying power {powerState.PowerType.Name}: {ex.Message}\n{ex.StackTrace}");
                    }
                }

                GD.Print("[MinionStorage] Minion powers after restore (before DynamicVars):");
                foreach (var p in minion.Powers)
                    GD.Print($"[MinionStorage]   {p.Id.Entry}, Type={p.GetType().Name}, Amount={p.Amount}");

                foreach (var powerState in minionState.Powers)
                {
                    if (powerState.DynamicVarValues.Count == 0)
                        continue;

                    if (powerState.PowerType == typeof(MinionGuardianPower))
                        continue;

                    var existingPower = minion.GetPower(ModelDb.GetId(powerState.PowerType));
                    if (existingPower == null)
                    {
                        GD.Print($"[MinionStorage] WARNING: Cannot restore DynamicVars for {powerState.PowerType.Name} - power not found on minion");
                        continue;
                    }

                    GD.Print($"[MinionStorage] Restoring DynamicVars for {powerState.PowerType.Name}:");

                    if (existingPower is ModPowerTemplate modPower)
                    {
                        foreach (var varPair in powerState.DynamicVarValues)
                        {
                            GD.Print($"[MinionStorage]   ModPowerTemplate: {varPair.Key} -> {varPair.Value} (hasKey={modPower.DynamicVars.ContainsKey(varPair.Key)})");
                            if (modPower.DynamicVars.ContainsKey(varPair.Key))
                                modPower.DynamicVars[varPair.Key].BaseValue = varPair.Value;
                        }
                    }
                    else if (existingPower is ModActionTemplate modAction)
                    {
                        foreach (var varPair in powerState.DynamicVarValues)
                        {
                            GD.Print($"[MinionStorage]   ModActionTemplate: {varPair.Key} -> {varPair.Value} (hasKey={modAction.DynamicVars.ContainsKey(varPair.Key)})");
                            if (modAction.DynamicVars.ContainsKey(varPair.Key))
                                modAction.DynamicVars[varPair.Key].BaseValue = varPair.Value;
                        }
                        modAction.InvokeDisplayAmountChanged();
                    }
                    else if (existingPower is MinionLib.Action.ActionModel actionModel)
                    {
                        foreach (var varPair in powerState.DynamicVarValues)
                        {
                            GD.Print($"[MinionStorage]   ActionModel: {varPair.Key} -> {varPair.Value} (hasKey={actionModel.DynamicVars.ContainsKey(varPair.Key)})");
                            if (actionModel.DynamicVars.ContainsKey(varPair.Key))
                                actionModel.DynamicVars[varPair.Key].BaseValue = varPair.Value;
                        }
                    }
                }

                if (minionState.StoredTurnCount < _playerTurnCount && minion.CombatState != null)
                {
                    var turnsMissed = _playerTurnCount - minionState.StoredTurnCount;
                    shouldReapplyEveryoneOutAfterRestore = true;
                    GD.Print($"[MinionStorage] Turn changed since storage (stored={minionState.StoredTurnCount}, current={_playerTurnCount}, missed={turnsMissed}), refreshing action powers");

                    foreach (var power in minion.Powers.ToList())
                    {
                        if (power is ModActionTemplate actionPower)
                        {
                            for (int i = 0; i < turnsMissed; i++)
                            {
                                await actionPower.AfterSideTurnStart(CombatSide.Player, new List<Creature> { minion }, minion.CombatState);
                            }
                        }
                    }
                }

                GD.Print("[MinionStorage] === FINAL minion state after full restore ===");
                GD.Print($"[MinionStorage] Name={minion.Name}, IsAlive={minion.IsAlive}, CombatId={minion.CombatId}, MaxHp={minion.MaxHp}, CurrentHp={minion.CurrentHp}");
                GD.Print($"[MinionStorage] Side={minion.Side}, PetOwner={minion.PetOwner?.Creature.Name ?? "null"}");
                foreach (var p in minion.Powers)
                {
                    GD.Print($"[MinionStorage]   Power: {p.Id.Entry}, Type={p.GetType().Name}, Amount={p.Amount}");
                    if (p is MinionLib.Action.ActionModel am)
                    {
                        GD.Print($"[MinionStorage]     ActionModel: TargetType={am.TargetType}");
                        foreach (var dv in am.DynamicVars)
                            GD.Print($"[MinionStorage]     DynamicVar: {dv.Key}={dv.Value.BaseValue}");
                    }
                    else if (p is ModActionTemplate mat)
                    {
                        foreach (var dv in mat.DynamicVars)
                            GD.Print($"[MinionStorage]     DynamicVar: {dv.Key}={dv.Value.BaseValue}");
                    }
                }
            }

            if (shouldReapplyEveryoneOutAfterRestore)
            {
                var everyoneOutPower = player.Creature.GetPower<EveryoneOutPower>();
                if (everyoneOutPower != null)
                {
                    GD.Print($"[MinionStorage] Reapplying EveryoneOut extra uses after restore: {everyoneOutPower.Amount}");
                    await everyoneOutPower.AddExtraUsesToSalonMembers(everyoneOutPower.Amount);
                }
            }
        }
        finally
        {
            RestoringKeys.Remove(dictKey);
            GD.Print($"[MinionStorage] RestoreMinions completed for key={storageKey}");
        }
    }
}
