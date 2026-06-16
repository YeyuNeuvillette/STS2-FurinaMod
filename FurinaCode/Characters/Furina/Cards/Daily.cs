using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Base;
using MegaCrit.Sts2.Core.Combat;
using Godot;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Daily : TropeCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
    { 
        new BlockVar(10m, ValueProp.Move),
        new BlockVar("BonusBlock", 6m, ValueProp.Move)
    };

    public override bool GainsBlock => true;

    public Daily()
        : base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override bool ShouldGlowGoldInternal => LastCardWasSkill();

    private bool LastCardWasSkill()
    {
        if (CombatState == null || Owner == null)
            return false;
            
        try
        {
            var cardPlaysStarted = CombatManager.Instance.History.CardPlaysStarted;
            if (cardPlaysStarted == null)
                return false;
                
            var lastCardPlay = cardPlaysStarted
                .Where(e => e != null && e.HappenedThisTurn(CombatState) && e.CardPlay?.Card?.Owner == Owner && e.CardPlay?.Card != this)
                .LastOrDefault();

            return lastCardPlay != null && lastCardPlay.CardPlay?.Card?.Type == CardType.Skill;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Daily] Error in LastCardWasSkill: {ex.Message}");
            return false;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.OnPlay(choiceContext, cardPlay);
        
        if (Owner?.Creature == null)
            return;

        var baseBlock = DynamicVars.Block.BaseValue;

        await CreatureCmd.GainBlock(Owner.Creature, baseBlock, ValueProp.Move, cardPlay);

        if (LastCardWasSkill())
        {
            GD.Print($"[Daily] Last card was a skill, adding bonus block");
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars["BonusBlock"].BaseValue, ValueProp.Move, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
    }
}