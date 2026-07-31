using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Furina.Powers;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class DodgeAndWeave : OusiaPneumaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<DodgeAndWeaveOusiaPower>(1m),
        new PowerVar<DodgeAndWeavePneumaPower>(1m),
        new PowerVar<DodgeAndWeaveChorusPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        base.AdditionalHoverTips.Concat(
            [
                HoverTipFactory.FromPower<VigorPower>(),
                AgilityPowerRef.HoverTip()
            ]
        );

    public DodgeAndWeave()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnOusiaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<DodgeAndWeaveOusiaPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["DodgeAndWeaveOusiaPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override async Task OnPneumaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<DodgeAndWeavePneumaPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["DodgeAndWeavePneumaPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override async Task OnNoArkheEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<DodgeAndWeaveChorusPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["DodgeAndWeaveChorusPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DodgeAndWeaveOusiaPower"].UpgradeValueBy(1m);
        DynamicVars["DodgeAndWeavePneumaPower"].UpgradeValueBy(1m);
        DynamicVars["DodgeAndWeaveChorusPower"].UpgradeValueBy(1m);
    }
}
