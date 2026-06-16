using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Furina.Characters.Furina.Minions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MinionLib.Action;
using Furina.RitsuAdapters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Powers;

[RegisterPower]
public sealed class EveryoneOutPower : FurinaPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private static readonly HashSet<System.Type> SalonMemberTypes =
    [
        typeof(GentilhommeUsher),
        typeof(MademoiselleCrabaletta),
        typeof(SurintendanteChevalmarin)
    ];

    public override async Task AfterSideTurnStartLate(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player || Owner?.Player == null)
            return;

        await AddExtraUsesToSalonMembers(Amount);
    }

    internal async Task AddExtraUsesToSalonMembers(decimal extraAmount)
    {
        var salonPets = Owner!.Player!.PlayerCombatState?.Pets
            .Where(p => p.Monster != null && SalonMemberTypes.Contains(p.Monster.GetType()))
            .ToList();

        if (salonPets == null)
            return;

        foreach (var pet in salonPets)
        {
            var action = pet.GetPower<ModActionTemplate>();
            if (action == null) continue;

            if (!IsOnActiveTurn(action)) continue;

            if (action.DynamicVars.TryGetValue("Uses", out var usesVar))
            {
                usesVar.BaseValue += extraAmount;
                action.InvokeDisplayAmountChanged();
            }
        }
    }

    private static bool IsOnActiveTurn(ModActionTemplate action)
    {
        if (action.DynamicVars.TryGetValue("TurnCounter", out var turnCounterVar))
        {
            return turnCounterVar.BaseValue == 0m;
        }
        return true;
    }
}