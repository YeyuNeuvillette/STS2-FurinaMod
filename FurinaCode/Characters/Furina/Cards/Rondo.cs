using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Base;
using Furina.Characters.Furina;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Rondo : BaseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new DamageVar(5m, ValueProp.Move) };

    public Rondo()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
            return;

        var enemies = CombatState.Enemies;
        if (enemies.Count == 0)
            return;

        Creature leftEnemy = enemies.First();
        Creature rightEnemy = enemies.Last();

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(leftEnemy)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(rightEnemy)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}