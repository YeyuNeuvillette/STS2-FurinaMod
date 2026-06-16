using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Furina;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Modulation : OusiaPneumaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new DamageVar(9m, ValueProp.Move),
        new PowerVar<VulnerablePower>(2m)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            var tips = new List<IHoverTip>();

            if (CurrentArkheState == ArkheState.Ousia)
            {
                tips.Add(HoverTipFactory.FromPower<VulnerablePower>(2));
            }
            else if (CurrentArkheState == ArkheState.None)
            {
                tips.Add(HoverTipFactory.FromPower<VulnerablePower>(2));
            }

            return tips;
        }
    }

    public Modulation()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override async Task OnOusiaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
            return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        await PowerCmd.Apply<VulnerablePower>(choiceContext, CombatState.HittableEnemies, DynamicVars.Vulnerable.BaseValue, Owner?.Creature, this);
    }

    protected override async Task OnPneumaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
            return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this)
            .TargetingAllOpponents(CombatState)
            .WithHitCount(2)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["VulnerablePower"].UpgradeValueBy(1m);
    }

    protected override async Task OnNoArkheEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
            return;

        await PowerCmd.Apply<VulnerablePower>(choiceContext, CombatState.HittableEnemies, DynamicVars.Vulnerable.BaseValue, Owner?.Creature, this);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this)
            .TargetingAllOpponents(CombatState)
            .WithHitCount(2)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }
}