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

        GD.Print($"[Transition] Checking powers: has OusiaPower={creature.HasPower<OusiaPower>()}, has PneumaPower={creature.HasPower<PneumaPower>()}");
        
        if (creature.HasPower<OusiaPower>())
        {
            GD.Print("[Transition] Removing OusiaPower, applying PneumaPower");
            await PowerCmd.Remove<OusiaPower>(creature);
            await PowerCmd.Apply<PneumaPower>(new ThrowingPlayerChoiceContext(), creature, 1m, creature, null);
            GD.Print("[Transition] PneumaPower applied");
        }
        else if (creature.HasPower<PneumaPower>())
        {
            GD.Print("[Transition] Removing PneumaPower, applying OusiaPower");
            await PowerCmd.Remove<PneumaPower>(creature);
            await PowerCmd.Apply<OusiaPower>(new ThrowingPlayerChoiceContext(), creature, 1m, creature, null);
            GD.Print("[Transition] OusiaPower applied");
        }
        else
        {
            GD.Print("[Transition] No Arkhe power found, applying OusiaPower");
            await PowerCmd.Apply<OusiaPower>(new ThrowingPlayerChoiceContext(), creature, 1m, creature, null);
            GD.Print("[Transition] OusiaPower applied");
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
    }
}