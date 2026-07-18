using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using Furina.Characters.Furina;
using Furina.Characters.Furina.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class BestLeadingActor : FurinaCard
{
    private bool _isPneumaActive = true;

    public bool IsPneumaActive => _isPneumaActive;
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new PowerVar<BestLeadingActorPower>(1m)
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new List<CardKeyword> { CardKeyword.Retain };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_OUSIA_PNEUMA"))];

    public BestLeadingActor()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        description.Add("CurrentArkhe", _isPneumaActive ? "[color=#FFFF00]芒性[/color]" : "[purple]荒性[/purple]");
    }

    public void PlayTransformVfx()
    {
        try
        {
            var combatRoom = NCombatRoom.Instance;
            if (combatRoom == null || combatRoom.Ui == null || combatRoom.Ui.Hand == null)
                return;

            var cardHolder = combatRoom.Ui.Hand.ActiveHolders.FirstOrDefault(h => h.CardModel == this);
            if (cardHolder is NHandCardHolder handCardHolder && handCardHolder.CardNode != null)
            {
                var vfx = NCardTransformShineVfx.Create(handCardHolder.CardNode, this, null);
                if (vfx != null)
                {
                    TaskHelper.RunSafely(vfx.PlayAnimation(shortVersion: true));
                }
                handCardHolder.CardNode.Reload();
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[BestLeadingActor] PlayTransformVfx error: {ex.Message}");
        }
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || Pile == null || Pile.Type != PileType.Hand)
            return;

        _isPneumaActive = !_isPneumaActive;
        PlayTransformVfx();
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        var power = await PowerCmd.Apply<BestLeadingActorPower>(choiceContext, Owner.Creature, DynamicVars["BestLeadingActorPower"].BaseValue, Owner.Creature, this);
        power?.SetPneumaState(_isPneumaActive);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
