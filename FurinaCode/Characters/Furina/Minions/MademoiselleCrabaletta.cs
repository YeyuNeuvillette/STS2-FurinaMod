using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MinionLib.Minion;
using Furina.Characters.Furina.Actions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Furina.Characters.Furina.Powers;

namespace Furina.Characters.Furina.Minions;

public sealed class MademoiselleCrabaletta : MinionModel
{
    public override int MinInitialHp => 3;

    public override int MaxInitialHp => 3;

    protected override string VisualsPath => "res://Furina/scenes/Minions/MademoiselleCrabaletta.tscn";

    public override async Task OnSummon(PlayerChoiceContext choiceContext, Player owner, MinionSummonOptions options)
    {
        if (options.MaxHp is decimal maxHp)
            await CreatureCmd.SetMaxAndCurrentHp(owner.PlayerCombatState!.GetPet<MademoiselleCrabaletta>()!, maxHp);

        await PowerCmd.Apply<CrabShield>(choiceContext, owner.PlayerCombatState!.GetPet<MademoiselleCrabaletta>()!, 1m, owner.Creature, options.Source);
        await PowerCmd.Apply<SalonSolitairePower>(choiceContext, owner.PlayerCombatState!.GetPet<MademoiselleCrabaletta>()!, 1m, owner.Creature, options.Source);
    }
}