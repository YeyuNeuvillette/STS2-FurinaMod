using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Encore : FurinaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new DamageVar(4m, DamageProps.card),
        new CardsVar(1),
        new EnergyVar(1)
    };

    protected override bool ShouldGlowGoldInternal => HasPlayedEnoughOtherCards();

    public Encore()
        : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
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

        if (HasPlayedEnoughOtherCards())
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
            await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars.Cards.UpgradeValueBy(1m);
    }

    private bool HasPlayedEnoughOtherCards()
    {
        if (CombatState == null)
            return false;

        var cardsPlayedThisTurn = CombatManager.Instance.History.CardPlaysStarted
            .Count(e => e.HappenedThisTurn(CombatState) && e.CardPlay.Card.Owner == Owner && e.CardPlay.Card != this);

        return cardsPlayedThisTurn >= 5;
    }
}