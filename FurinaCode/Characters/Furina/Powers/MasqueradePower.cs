using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
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

    private int CardsDrawnThisTurn
    {
        get
        {
            if (Owner == null)
                return 0;

            return CombatManager.Instance.History.Entries
                .OfType<CardDrawnEntry>()
                .Count(e => e.HappenedThisTurn(CombatState) && e.Actor == Owner);
        }
    }

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner?.Player)
            return count;

        return count + 2m;
    }

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);

        UpdateCardsDrawnDisplay();
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player || Owner == null)
            return;

        UpdateCardsDrawnDisplay();
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        UpdateCardsDrawnDisplay();
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || Owner == null)
            return;

        if (CardsDrawnThisTurn < 10)
        {
            await CreatureCmd.Kill(Owner);
            return;
        }

        await PowerCmd.ModifyAmount(choiceContext, this, -1m, Owner, null);
    }

    private void UpdateCardsDrawnDisplay()
    {
        DynamicVars["CardsDrawn"].BaseValue = CardsDrawnThisTurn;
        InvokeDisplayAmountChanged();
    }
}