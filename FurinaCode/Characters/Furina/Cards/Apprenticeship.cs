using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Base;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Apprenticeship : TropeCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new DamageVar(7m, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("ConsecutiveAttacks").WithMultiplier((CardModel card, Creature? _) => CountConsecutiveAttacks(card))
    };

    public Apprenticeship()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.OnPlay(choiceContext, cardPlay);

        if (CombatState == null)
            return;

        var target = cardPlay.Target;
        if (target == null)
            return;

        int consecutiveAttacks = CountConsecutiveAttacks(this);
        int totalHits = 1 + consecutiveAttacks;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(totalHits)
            .FromCard(this)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }

    private static int CountConsecutiveAttacks(CardModel card)
    {
        if (card.CombatState == null || card.Owner == null)
            return 0;

        var cardPlaysThisTurn = CombatManager.Instance.History.CardPlaysStarted
            .Where(e => e.HappenedThisTurn(card.CombatState) && e.CardPlay.Card.Owner == card.Owner)
            .ToList();

        int count = 0;
        for (int i = cardPlaysThisTurn.Count - 1; i >= 0; i--)
        {
            var playedCard = cardPlaysThisTurn[i].CardPlay.Card;
            if (playedCard == card)
                continue;

            if (playedCard.Type == CardType.Attack)
                count++;
            else
                break;
        }

        return count;
    }
}