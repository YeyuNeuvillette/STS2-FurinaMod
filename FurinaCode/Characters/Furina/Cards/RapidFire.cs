using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Furina.Characters.Furina.Actions;
using Furina.Characters.Furina.Minions;
using Furina.Characters.Furina.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class RapidFire : InviteCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<RapidFirePower>(1m)];

    public RapidFire()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override System.Type SelectMinionType(PlayerChoiceContext choiceContext) =>
        typeof(GentilhommeUsher);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await base.OnPlay(choiceContext, cardPlay);

        await PowerCmd.Apply<RapidFirePower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["RapidFirePower"].BaseValue,
            Owner.Creature,
            this);

        var usher = Owner.PlayerCombatState?.Pets
            .FirstOrDefault(pet => pet.Monster is GentilhommeUsher);
        usher?.GetPower<HydroBarrage>()?.AddUses(1m);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
