using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using Furina.Characters.Base;
using Furina.Characters.Furina.Powers;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Transition : BaseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
    { 
        new BlockVar(11m, ValueProp.Move)
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword> 
    { 
        ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_ARKHE")
    };

    public override bool GainsBlock => true;

    public Transition()
        : base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        var creature = Owner.Creature;
        var baseBlock = DynamicVars.Block.BaseValue;

        GD.Print($"[Transition] Gaining {baseBlock} block");
        await CreatureCmd.GainBlock(creature, baseBlock, ValueProp.Move, cardPlay);

        await InvitationManager.SwitchArkhe(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
    }
}
