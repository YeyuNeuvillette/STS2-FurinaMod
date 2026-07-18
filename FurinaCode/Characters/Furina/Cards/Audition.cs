using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Furina.Characters.Base;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Audition : FurinaCard
{
    private const string _drawKey = "Draw";

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
    { 
        new DamageVar(7m, ValueProp.Move),
        new IntVar(_drawKey, 1m)
    };

    public Audition()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
            return;

        var target = cardPlay.Target;
        if (target == null)
            return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        var drawCount = (int)DynamicVars[_drawKey].BaseValue;
        var drawnCards = await CardPileCmd.Draw(choiceContext, drawCount, Owner);
        
        foreach (var card in drawnCards)
        {
            card.EnergyCost.AddThisTurn(-1);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[_drawKey].UpgradeValueBy(1m);
    }
}