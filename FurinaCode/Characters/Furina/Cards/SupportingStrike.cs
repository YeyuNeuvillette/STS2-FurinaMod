using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Base;
using Furina.Characters.Furina.Minions;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class SupportingStrike : InviteCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
    { 
        new DamageVar(4m, ValueProp.Move)
    };
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };

    public SupportingStrike()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override Type SelectMinionType(PlayerChoiceContext choiceContext)
    {
        var minions = new[] { typeof(GentilhommeUsher), typeof(MademoiselleCrabaletta), typeof(SurintendanteChevalmarin) };
        return Owner.RunState.Rng.CombatCardSelection.NextItem(minions)!;
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

        await base.OnPlay(choiceContext, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}