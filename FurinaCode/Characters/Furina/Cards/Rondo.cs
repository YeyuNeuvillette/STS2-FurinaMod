using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Combat.AttackHits;
using Furina.Characters.Base;
using Furina.Characters.Furina;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Rondo : BaseCard, IAttackHitHookListener
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new DamageVar(5m, ValueProp.Move) };

    public Rondo()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    public async Task BeforeAttackHit(AttackHitContext context)
    {
        if (!ReferenceEquals(context.CardSource, this))
            return;

        var enemies = context.CombatState.Enemies;
        if (enemies.Count == 0)
            return;

        if (context.HitIndex == 0)
        {
            context.Targets = new List<Creature> { enemies.First() };
        }
        else if (context.HitIndex == 1)
        {
            context.Targets = new List<Creature> { enemies.Last() };
        }
    }

    public Task AfterAttackHit(AttackHitContext context)
    {
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
            return;

        var enemies = CombatState.Enemies;
        if (enemies.Count == 0)
            return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState)
            .WithHitCount(2)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}