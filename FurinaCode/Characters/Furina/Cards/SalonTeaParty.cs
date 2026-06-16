using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Furina.Characters.Furina.Minions;
using Furina.Characters.Furina.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class SalonTeaParty : InviteCard
{
    protected override bool HasEnergyCostX => true;

    protected override Type SelectMinionType(PlayerChoiceContext choiceContext)
    {
        var minions = new[] { typeof(GentilhommeUsher), typeof(MademoiselleCrabaletta), typeof(SurintendanteChevalmarin) };
        return Owner.RunState.Rng.CombatCardSelection.NextItem(minions)!;
    }

    public SalonTeaParty()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        int x = ResolveEnergyXValue();
        if (CurrentUpgradeLevel > 0)
            x++;

        var ousiaPower = Owner.Creature.GetPower<OusiaPower>();
        var pneumaPower = Owner.Creature.GetPower<PneumaPower>();

        for (int i = 0; i < x; i++)
        {
            if (ousiaPower != null && ousiaPower.Amount > 0)
            {
                await HandleOusiaState(choiceContext);
            }
            else if (pneumaPower != null && pneumaPower.Amount > 0)
            {
                await HandlePneumaState(choiceContext);
            }
        }
    }
}