using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MinionLib.Minion;
using Furina.Characters.Furina.Actions;
using Furina.Characters.Furina.Powers;

namespace Furina.Characters.Furina.Minions;

public sealed class GentilhommeUsher : MinionModel
{
    public override int MinInitialHp => 3;

    public override int MaxInitialHp => 3;

    protected override string VisualsPath => "res://Furina/scenes/Minions/GentilhommeUsher.tscn";

    public override async Task OnSummon(PlayerChoiceContext choiceContext, Player owner, MinionSummonOptions options)
    {
        GD.Print($"[MinionDebug] GentilhommeUsher.OnSummon called, MaxHp={options.MaxHp}");

        var pet = owner.PlayerCombatState!.GetPet<GentilhommeUsher>();
        GD.Print($"[MinionDebug] GentilhommeUsher.OnSummon: GetPet result={pet != null}, name={pet?.Name ?? "null"}");

        if (options.MaxHp is decimal maxHp)
            await CreatureCmd.SetMaxAndCurrentHp(pet!, maxHp);

        GD.Print($"[MinionDebug] GentilhommeUsher.OnSummon: About to apply HydroBarrage");
        await PowerCmd.Apply<HydroBarrage>(choiceContext, pet!, 1m, owner.Creature, options.Source);
        GD.Print($"[MinionDebug] GentilhommeUsher.OnSummon: HydroBarrage applied");

        GD.Print($"[MinionDebug] GentilhommeUsher.OnSummon: About to apply SalonSolitairePower");
        await PowerCmd.Apply<SalonSolitairePower>(choiceContext, pet!, 1m, owner.Creature, options.Source);
        GD.Print($"[MinionDebug] GentilhommeUsher.OnSummon: SalonSolitairePower applied");

        GD.Print($"[MinionDebug] GentilhommeUsher.OnSummon: Powers on pet after summon:");
        foreach (var p in pet!.Powers)
        {
            GD.Print($"[MinionDebug]   Power: {p.Id.Entry}, Type={p.GetType().Name}, Amount={p.Amount}");
        }
    }
}