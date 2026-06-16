using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Powers;

[RegisterPower]
public sealed class MasqueradePower : FurinaPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("CardsDrawn", 0m)];

    private int _cardsDrawnThisTurn;

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner?.Player)
            return count;

        return count + 2m;
    }

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);

        if (Owner?.Player == null)
            return;

        _cardsDrawnThisTurn = PileType.Hand.GetPile(Owner.Player).Cards.Count();
        DynamicVars["CardsDrawn"].BaseValue = _cardsDrawnThisTurn;
        InvokeDisplayAmountChanged();
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player || Owner == null)
            return;

        _cardsDrawnThisTurn = 0;
        DynamicVars["CardsDrawn"].BaseValue = 0;
        InvokeDisplayAmountChanged();
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (Owner?.Player == null)
            return;

        _cardsDrawnThisTurn++;
        DynamicVars["CardsDrawn"].BaseValue = _cardsDrawnThisTurn;
        InvokeDisplayAmountChanged();
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || Owner == null)
            return;

        if (_cardsDrawnThisTurn < 10)
        {
            await CreatureCmd.Kill(Owner);
            return;
        }

        await PowerCmd.ModifyAmount(choiceContext, this, -1m, Owner, null);
    }
}