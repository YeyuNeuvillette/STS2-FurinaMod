using Furina.Characters.Furina.Actions;
using Furina.Characters.Furina.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MinionLib.Minion;

namespace Furina.Characters.Furina.Minions;

public abstract class FurinaMinion : MinionModel
{
    public override int MinInitialHp => 3;
    public override int MaxInitialHp => 3;

    protected static async Task SetSummonedHp<T>(Player owner, MinionSummonOptions options)
        where T : MinionModel
    {
        if (options.MaxHp is decimal maxHp)
            await CreatureCmd.SetMaxAndCurrentHp(owner.PlayerCombatState!.GetPet<T>()!, maxHp);
    }
}

public sealed class GentilhommeUsher : FurinaMinion
{
    protected override string VisualsPath => "res://Furina/scenes/Minions/GentilhommeUsher.tscn";

    public override async Task OnSummon(PlayerChoiceContext choiceContext, Player owner, MinionSummonOptions options)
    {
        await SetSummonedHp<GentilhommeUsher>(owner, options);
        var pet = owner.PlayerCombatState!.GetPet<GentilhommeUsher>()!;
        await PowerCmd.Apply<HydroBarrage>(choiceContext, pet, 1m, owner.Creature, options.Source);
        await PowerCmd.Apply<SalonSolitairePower>(choiceContext, pet, 1m, owner.Creature, options.Source);
    }
}

public sealed class MademoiselleCrabaletta : FurinaMinion
{
    protected override string VisualsPath => "res://Furina/scenes/Minions/MademoiselleCrabaletta.tscn";

    public override async Task OnSummon(PlayerChoiceContext choiceContext, Player owner, MinionSummonOptions options)
    {
        await SetSummonedHp<MademoiselleCrabaletta>(owner, options);
        var pet = owner.PlayerCombatState!.GetPet<MademoiselleCrabaletta>()!;
        await PowerCmd.Apply<CrabShield>(choiceContext, pet, 1m, owner.Creature, options.Source);
        await PowerCmd.Apply<SalonSolitairePower>(choiceContext, pet, 1m, owner.Creature, options.Source);
    }
}

public sealed class SurintendanteChevalmarin : FurinaMinion
{
    protected override string VisualsPath => "res://Furina/scenes/Minions/SurintendanteChevalmarin.tscn";

    public override async Task OnSummon(PlayerChoiceContext choiceContext, Player owner, MinionSummonOptions options)
    {
        await SetSummonedHp<SurintendanteChevalmarin>(owner, options);
        var pet = owner.PlayerCombatState!.GetPet<SurintendanteChevalmarin>()!;
        await PowerCmd.Apply<PureWaterNote>(choiceContext, pet, 1m, owner.Creature, options.Source);
        await PowerCmd.Apply<SalonSolitairePower>(choiceContext, pet, 1m, owner.Creature, options.Source);
    }
}

public sealed class SingerOfManyWaters : FurinaMinion
{
    protected override string VisualsPath => "res://Furina/scenes/Minions/SingerOfManyWaters.tscn";

    public override async Task OnSummon(PlayerChoiceContext choiceContext, Player owner, MinionSummonOptions options)
    {
        await SetSummonedHp<SingerOfManyWaters>(owner, options);
        var pet = owner.PlayerCombatState!.GetPet<SingerOfManyWaters>()!;
        await PowerCmd.Apply<JoyfulSinging>(choiceContext, pet, 1m, owner.Creature, options.Source);
    }
}
