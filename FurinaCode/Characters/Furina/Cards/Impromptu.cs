using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Base;
using Furina.Characters.Furina;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Impromptu : OusiaPneumaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new DamageVar(5m, ValueProp.Move),
        new BlockVar(3m, ValueProp.Move)
    };

    public override bool GainsBlock => true;

    public Impromptu()
        : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override CardType GetOusiaCardType()
    {
        return CardType.Attack;
    }

    protected override CardType GetPneumaCardType()
    {
        return CardType.Skill;
    }

    protected override int GetOusiaEnergyCost()
    {
        return 0;
    }

    protected override int GetPneumaEnergyCost()
    {
        return 0;
    }

    protected override TargetType GetOusiaTargetType()
    {
        return TargetType.AnyEnemy;
    }

    protected override TargetType GetPneumaTargetType()
    {
        return TargetType.Self;
    }

    protected override async Task OnOusiaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
            return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override async Task OnPneumaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);
    }

    protected override async Task OnChorusEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await OnOusiaEffect(choiceContext, cardPlay);
        await OnPneumaEffect(choiceContext, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}