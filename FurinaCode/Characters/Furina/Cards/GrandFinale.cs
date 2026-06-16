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

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class GrandFinale : FurinaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
    { 
        new DamageVar(18m, ValueProp.Move),
        new BlockVar(9m, ValueProp.Move)
    };

    public GrandFinale()
        : base(3, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
            return;

        var target = cardPlay.Target;
        if (target == null)
            return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);

        var drawAmount = CurrentUpgradeLevel > 0 ? 4m : 3m;
        await CardPileCmd.Draw(choiceContext, drawAmount, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6m);
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}