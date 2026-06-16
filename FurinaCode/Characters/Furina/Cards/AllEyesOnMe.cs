using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class AllEyesOnMe : FurinaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new DamageVar(15m, ValueProp.Move),
        new EnergyVar(2)
    };

    protected override bool ShouldGlowGoldInternal => CountOtherCardsInHand() >= 9;

    public AllEyesOnMe()
        : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
            return;

        var target = cardPlay.Target;
        if (target == null)
            return;

        var damage = DynamicVars.Damage.BaseValue;

        if (CountOtherCardsInHand() >= 9)
        {
            damage *= 2;
            await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        }

        await DamageCmd.Attack(damage).FromCard(this).Targeting(target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }

    private int CountOtherCardsInHand()
    {
        var hand = Owner?.PlayerCombatState?.Hand;
        if (hand == null)
            return 0;

        var count = hand.Cards.Count;
        if (hand.Cards.Contains(this))
            count--;

        return count;
    }
}