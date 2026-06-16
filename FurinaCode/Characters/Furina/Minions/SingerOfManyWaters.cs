using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MinionLib.Minion;
using Furina.Characters.Furina.Actions;
using Furina.Characters.Furina.Powers;
using MinionLib.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Furina.Characters.Furina.Minions;

public sealed class SingerOfManyWaters : MinionModel
{
    public override int MinInitialHp => 3;

    public override int MaxInitialHp => 3;

    protected override string VisualsPath => "res://Furina/scenes/Minions/SingerOfManyWaters.tscn";

    public override async Task OnSummon(PlayerChoiceContext choiceContext, Player owner, MinionSummonOptions options)
    {
        if (options.MaxHp is decimal maxHp)
            await CreatureCmd.SetMaxAndCurrentHp(owner.PlayerCombatState!.GetPet<SingerOfManyWaters>()!, maxHp);

        await PowerCmd.Apply<JoyfulSinging>(choiceContext, owner.PlayerCombatState!.GetPet<SingerOfManyWaters>()!, 1m, owner.Creature, options.Source);
        await PowerCmd.Apply<MinionGuardianPower>(choiceContext, owner.PlayerCombatState!.GetPet<SingerOfManyWaters>()!, 1m, owner.Creature, options.Source);
    }
}