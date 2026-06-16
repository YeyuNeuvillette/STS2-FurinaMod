using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Furina.Characters.Furina.Minions;
using Furina.Characters.Furina.Actions;
using Furina.Characters.Furina.Powers;
using MinionLib.Commands;
using MinionLib.Minion;
using MegaCrit.Sts2.Core.Combat;
using MinionLib.Powers;
using STS2RitsuLib;
using STS2RitsuLib.RunData;
using STS2RitsuLib.Scaffolding.Content;
using Furina.RitsuAdapters;

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

    private static readonly HashSet<Type> FurinaMinionTypes =
    [
        typeof(GentilhommeUsher),
        typeof(MademoiselleCrabaletta),
        typeof(SurintendanteChevalmarin),
        typeof(SingerOfManyWaters)
    ];

    public static bool IsFurinaMinion(Creature creature)
    {
        return creature.Monster != null && FurinaMinionTypes.Contains(creature.Monster.GetType());
    }

    private static readonly Dictionary<(ulong netId, string key), List<MinionState>> _storedMinions = new();
    private static readonly HashSet<(ulong netId, string key)> _isRestoring = [];
    private static readonly HashSet<ulong> _salonSolitaireUsed = [];
    private static int _playerTurnCount;

    public static void ClearAll()
    {
        _storedMinions.Clear();
        _isRestoring.Clear();
        _salonSolitaireUsed.Clear();
        _playerTurnCount = 0;
    }

    public static void ResetSalonSolitaire()
    {
        _salonSolitaireUsed.Clear();
    }

    public static void IncrementTurnCount()
    {
        _playerTurnCount++;
    }

    public static bool IsSalonSolitaireUsed(Player player)
    {
        return _salonSolitaireUsed.Contains(player.NetId);
    }

    public static void MarkSalonSolitaireUsed(Player player)
    {
        _salonSolitaireUsed.Add(player.NetId);
    }

    public static void StoreMinions(Player player, string storageKey)
    {
        var dictKey = (player.NetId, storageKey);
        _storedMinions.Remove(dictKey);
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

            if (!FurinaMinionTypes.Contains(pet.Monster.GetType()))
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

        _storedMinions[dictKey] = minions;
        GD.Print($"[MinionStorage] Stored {minions.Count} minions for key={storageKey}");
    }

    public static bool HasStoredMinions(Player player, string storageKey)
    {
        var dictKey = (player.NetId, storageKey);
        return _storedMinions.TryGetValue(dictKey, out var list) && list.Count > 0;
    }

    public static async Task RestoreMinions(PlayerChoiceContext choiceContext, Player player, CardModel? source, string storageKey)
    {
        var dictKey = (player.NetId, storageKey);

        if (_isRestoring.Contains(dictKey))
        {
            GD.Print("[MinionStorage] Already restoring minions, skipping");
            return;
        }

        if (!_storedMinions.TryGetValue(dictKey, out var minionsToRestore) || minionsToRestore.Count == 0)
        {
            GD.Print($"[MinionStorage] No stored minions found for key={storageKey}");
            return;
        }

        _storedMinions.Remove(dictKey);
        _isRestoring.Add(dictKey);
        GD.Print($"[MinionStorage] RestoreMinions called for key={storageKey}, count={minionsToRestore.Count}");

        try
        {
            foreach (var minionState in minionsToRestore)
            {
                GD.Print($"[MinionStorage] Restoring minion: Type={minionState.MinionType.Name}, MaxHp={minionState.MaxHp}, CurrentHp={minionState.CurrentHp}, Powers={minionState.Powers.Count}");
                foreach (var ps in minionState.Powers)
                {
                    GD.Print($"[MinionStorage]   Stored power: {ps.PowerType.Name}, Amount={ps.Amount}, DynamicVars={ps.DynamicVarValues.Count}");
                    foreach (var dv in ps.DynamicVarValues)
                        GD.Print($"[MinionStorage]     DynamicVar: {dv.Key}={dv.Value}");
                }

                Creature? minion = null;

                if (minionState.MinionType == typeof(GentilhommeUsher))
                    minion = await MinionCmd.AddMinion<GentilhommeUsher>(choiceContext, player, new MinionSummonOptions(MaxHp: minionState.MaxHp, Source: source, Position: MinionPosition.Front));
                else if (minionState.MinionType == typeof(MademoiselleCrabaletta))
                    minion = await MinionCmd.AddMinion<MademoiselleCrabaletta>(choiceContext, player, new MinionSummonOptions(MaxHp: minionState.MaxHp, Source: source, Position: MinionPosition.Front));
                else if (minionState.MinionType == typeof(SurintendanteChevalmarin))
                    minion = await MinionCmd.AddMinion<SurintendanteChevalmarin>(choiceContext, player, new MinionSummonOptions(MaxHp: minionState.MaxHp, Source: source, Position: MinionPosition.Front));
                else if (minionState.MinionType == typeof(SingerOfManyWaters))
                    minion = await MinionCmd.AddMinion<SingerOfManyWaters>(choiceContext, player, new MinionSummonOptions(MaxHp: minionState.MaxHp, Source: source, Position: MinionPosition.Front));

                GD.Print($"[MinionStorage] AddMinion result: {(minion != null ? $"success, Name={minion.Name}, CombatId={minion.CombatId}" : "FAILED")}");

                if (minion != null)
                {
                    GD.Print($"[MinionStorage] Minion after summon - IsAlive={minion.IsAlive}, MaxHp={minion.MaxHp}, CurrentHp={minion.CurrentHp}");
                    GD.Print($"[MinionStorage] Minion powers after summon (before restore):");
                    foreach (var p in minion.Powers)
                        GD.Print($"[MinionStorage]   {p.Id.Entry}, Type={p.GetType().Name}, Amount={p.Amount}");

                    await CreatureCmd.SetMaxHp(minion, minionState.MaxHp);
                    await CreatureCmd.SetCurrentHp(minion, minionState.CurrentHp);

                    var onSummonPowerTypes = minion.Monster switch
                    {
                        GentilhommeUsher => new HashSet<ModelId> { ModelDb.GetId(typeof(HydroBarrage)), ModelDb.GetId(typeof(SalonSolitairePower)) },
                        MademoiselleCrabaletta => new HashSet<ModelId> { ModelDb.GetId(typeof(CrabShield)), ModelDb.GetId(typeof(SalonSolitairePower)) },
                        SurintendanteChevalmarin => new HashSet<ModelId> { ModelDb.GetId(typeof(PureWaterNote)), ModelDb.GetId(typeof(SalonSolitairePower)) },
                        SingerOfManyWaters => new HashSet<ModelId> { ModelDb.GetId(typeof(JoyfulSinging)) },
                        _ => new HashSet<ModelId>()
                    };

                    GD.Print($"[MinionStorage] onSummonPowerTypes for {minion.Monster?.GetType().Name ?? "null"}: {string.Join(", ", onSummonPowerTypes.Select(id => id.Entry))}");

                    foreach (var powerState in minionState.Powers)
                    {
                        var powerModelId = ModelDb.GetId(powerState.PowerType);
                        GD.Print($"[MinionStorage] Processing power: {powerState.PowerType.Name} (ModelId={powerModelId.Entry}), Amount={powerState.Amount}");

                        if (powerState.PowerType == typeof(MinionGuardianPower))
                        {
                            GD.Print($"[MinionStorage]   Skipping MinionGuardianPower (managed by SalonSolitaireGameAction)");
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
                            else
                            {
                                GD.Print($"[MinionStorage]   WARNING: onSummonPowerType {powerState.PowerType.Name} not found on minion! Will apply via non-generic path.");
                            }
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
                                GD.Print($"[MinionStorage]   PowerCmd.Apply completed successfully");
                            }
                        }
                        catch (Exception ex)
                        {
                            GD.PrintErr($"[MinionStorage] Error applying power {powerState.PowerType.Name}: {ex.Message}\n{ex.StackTrace}");
                        }
                    }

                    GD.Print($"[MinionStorage] Minion powers after restore (before DynamicVars):");
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

                    GD.Print($"[MinionStorage] === FINAL minion state after full restore ===");
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
                else
                {
                    GD.PrintErr($"[MinionStorage] Failed to summon minion: {minionState.MinionType.Name}");
                }
            }
        }
        finally
        {
            _isRestoring.Remove(dictKey);
            GD.Print($"[MinionStorage] RestoreMinions completed for key={storageKey}");
        }
    }
}