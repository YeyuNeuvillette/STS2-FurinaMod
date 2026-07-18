using Furina.Scripts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Powers;

public abstract class ArkheInvitationPower : FurinaPower
{
    private static readonly HashSet<(ulong playerId, string state)> SettingUp = [];

    protected abstract string StorageKey { get; }
    protected abstract string OtherStorageKey { get; }
    protected abstract bool IsOusia { get; }

    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.None;

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await base.AfterRemoved(oldOwner);

        var player = oldOwner.Player;
        if (player?.PlayerCombatState?.Pets == null)
            return;

        HpSyncManager.StoreHpPool(player, StorageKey);
        MinionStorage.StoreMinions(player, StorageKey);

        var minions = player.PlayerCombatState.Pets
            .Where(InvitationManager.IsFurinaMinion)
            .Where(pet => pet.IsAlive)
            .ToList();

        GD.Print($"[ArkheInvitationPower] Storing {StorageKey} and removing {minions.Count} active minions");
        foreach (var minion in minions)
        {
            Patches.SkipDeathAnimPatch.RegisterCreatureToSkipDeathAnim(minion);
            InvitationManager.RegisterStateSwitchRemoval(minion);
            await CreatureCmd.Kill(minion);
        }
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        await base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);

        if (!ReferenceEquals(power, this) || power.Amount <= 0m || amount <= 0m)
            return;

        UpdateVisuals();

        var player = Owner?.Player;
        if (player == null || !SettingUp.Add((player.NetId, StorageKey)))
            return;

        try
        {
            if (MinionStorage.HasStoredMinions(player, StorageKey))
                await MinionStorage.RestoreMinions(choiceContext, player, cardSource, StorageKey);

            await InvitationManager.FillMissingInvitations(choiceContext, player, cardSource, StorageKey);
            await ApplyPreviousFormHpPool(player);
        }
        finally
        {
            SettingUp.Remove((player.NetId, StorageKey));
        }
    }

    private async Task ApplyPreviousFormHpPool(MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        if (!HpSyncManager.TryGetStoredHpPool(player, OtherStorageKey, out var hpPool))
            return;

        var minions = player.PlayerCombatState?.Pets
            .Where(pet => pet.IsAlive && InvitationManager.IsFurinaMinion(pet))
            .OrderBy(pet => pet.CombatId)
            .ToList() ?? [];
        var distribution = HpSyncManager.CalculateHpDistribution(minions, hpPool);

        foreach (var minion in minions)
        {
            if (distribution.TryGetValue(minion, out var hp))
                await CreatureCmd.SetCurrentHp(minion, hp);
        }

        GD.Print($"[ArkheInvitationPower] Applied {OtherStorageKey} HP pool {hpPool} to {StorageKey}");
    }

    private void UpdateVisuals()
    {
        var creatureNode = NCombatRoom.Instance?.GetCreatureNode(Owner);
        if (creatureNode?.Visuals is FurinaCreatureVisuals visuals)
            visuals.SetArkheState(IsOusia);
    }
}

[RegisterPower]
public sealed class OusiaPower : ArkheInvitationPower
{
    protected override string StorageKey => InvitationManager.OusiaKey;
    protected override string OtherStorageKey => InvitationManager.PneumaKey;
    protected override bool IsOusia => true;
}

[RegisterPower]
public sealed class PneumaPower : ArkheInvitationPower
{
    protected override string StorageKey => InvitationManager.PneumaKey;
    protected override string OtherStorageKey => InvitationManager.OusiaKey;
    protected override bool IsOusia => false;
}
