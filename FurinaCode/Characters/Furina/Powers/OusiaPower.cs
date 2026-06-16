using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Scripts;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using static Godot.Node;

namespace Furina.Characters.Furina.Powers;

[RegisterPower]
public sealed class OusiaPower : FurinaPower
{
    private const string StorageKey = "Ousia";

    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.None;

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await base.AfterRemoved(oldOwner);
        GD.Print("[OusiaPower] AfterRemoved called");
        
        var player = oldOwner?.Player;
        if (player != null && player.PlayerCombatState?.Pets != null)
        {
            GD.Print($"[OusiaPower] Player found, pets count: {player.PlayerCombatState.Pets.Count}");
            
            MinionStorage.StoreMinions(player, StorageKey);
            GD.Print($"[OusiaPower] Minions stored, has stored minions: {MinionStorage.HasStoredMinions(player, StorageKey)}");

            var pets = player.PlayerCombatState.Pets.ToList();
            var furinaPets = pets.Where(p => MinionStorage.IsFurinaMinion(p)).ToList();
            GD.Print($"[OusiaPower] Killing {furinaPets.Count} Furina pets (out of {pets.Count} total) without death animation");
            foreach (var pet in furinaPets)
            {
                if (pet.IsAlive)
                {
                    GD.Print($"[OusiaPower] Killing pet: {pet.Name}");
                    try
                    {
                        Patches.SkipDeathAnimPatch.RegisterCreatureToSkipDeathAnim(pet);
                        await CreatureCmd.Kill(pet);
                        GD.Print($"[OusiaPower] Pet killed successfully: {pet.Name}");
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr($"[OusiaPower] Error killing pet {pet.Name}: {ex.Message}");
                    }
                }
            }
        }
        else
        {
            GD.Print("[OusiaPower] Player or pets not found");
        }
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);
        
        GD.Print($"[OusiaPower] AfterPowerAmountChanged: power.Amount={power.Amount}, amount={amount}");
        
        if (power.Amount > 0 && amount > 0)
        {
            UpdateVisuals(true);
            
            var player = Owner?.Player;
            if (player != null)
            {
                GD.Print($"[OusiaPower] Checking for stored minions");
                GD.Print($"[OusiaPower] Has stored minions: {MinionStorage.HasStoredMinions(player, StorageKey)}");
                if (MinionStorage.HasStoredMinions(player, StorageKey))
                {
                    GD.Print("[OusiaPower] Restoring minions");
                    await MinionStorage.RestoreMinions(choiceContext, player, cardSource, StorageKey);
                    GD.Print("[OusiaPower] Minions restored");
                }
            }
        }
    }

    private void UpdateVisuals(bool isOusia)
    {
        var creatureNode = NCombatRoom.Instance?.GetCreatureNode(Owner);
        if (creatureNode?.Visuals is FurinaCreatureVisuals visuals)
        {
            visuals.SetArkheState(isOusia);
        }
    }
}