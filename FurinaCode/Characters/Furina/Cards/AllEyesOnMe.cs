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
public sealed class AllEyesOnMe : FurinaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new CalculationBaseVar(17m),
        new ExtraDamageVar(17m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(static (card, _) =>
            card is AllEyesOnMe allEyesOnMe && allEyesOnMe.HasEnoughOtherCardsInHand() ? 1m : 0m),
        new EnergyVar(2)
    };

    protected override bool ShouldGlowGoldInternal => HasEnoughOtherCardsInHand();

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

        if (HasEnoughOtherCardsInHand())
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        }

        await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this, cardPlay).Targeting(target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(3m);
        DynamicVars.ExtraDamage.UpgradeValueBy(3m);
        DynamicVars.Energy.UpgradeValueBy(1m);
    }

    private bool HasEnoughOtherCardsInHand() => CountOtherCardsInHand() >= 9;

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
