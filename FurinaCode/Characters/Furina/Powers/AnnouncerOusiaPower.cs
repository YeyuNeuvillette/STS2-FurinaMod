using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Powers;


[RegisterPower]

public sealed class AnnouncerOusiaPower : FurinaPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    protected override bool IsVisibleInternal => false;

    public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner.Creature != base.Owner)
        {
            return false;
        }
        if (card.Type != CardType.Attack)
        {
            return false;
        }
        bool flag;
        switch (card.Pile?.Type)
        {
        case PileType.Hand:
        case PileType.Play:
            flag = true;
            break;
        default:
            flag = false;
            break;
        }
        if (!flag)
        {
            return false;
        }
        modifiedCost = originalCost - 1m;
        return true;
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == base.Owner && cardPlay.Card.Type == CardType.Attack)
        {
            bool flag;
            switch (cardPlay.Card.Pile?.Type)
            {
            case PileType.Hand:
            case PileType.Play:
                flag = true;
                break;
            default:
            flag = false;
                break;
            }
            if (flag)
            {
                await PowerCmd.Decrement(this);
            }
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(base.Owner))
        {
            await PowerCmd.Decrement(this);
        }
    }
}