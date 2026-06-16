using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MinionLib.Minion;
using Furina.Characters.Furina.Actions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Furina.Characters.Furina.Powers;

namespace Furina.Characters.Furina.Minions;

public sealed class SurintendanteChevalmarin : MinionModel
{
    public override int MinInitialHp => 3;

    public override int MaxInitialHp => 3;

    protected override string VisualsPath => "res://Furina/scenes/Minions/SurintendanteChevalmarin.tscn";

    public override async Task OnSummon(PlayerChoiceContext choiceContext, Player owner, MinionSummonOptions options)
    {
        if (options.MaxHp is decimal maxHp)
            await CreatureCmd.SetMaxAndCurrentHp(owner.PlayerCombatState!.GetPet<SurintendanteChevalmarin>()!, maxHp);

        await PowerCmd.Apply<PureWaterNote>(choiceContext, owner.PlayerCombatState!.GetPet<SurintendanteChevalmarin>()!, 1m, owner.Creature, options.Source);
        await PowerCmd.Apply<SalonSolitairePower>(choiceContext, owner.PlayerCombatState!.GetPet<SurintendanteChevalmarin>()!, 1m, owner.Creature, options.Source);
    }
}