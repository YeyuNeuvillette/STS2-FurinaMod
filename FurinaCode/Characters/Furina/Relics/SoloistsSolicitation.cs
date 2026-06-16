using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Base;
using Furina.Characters.Furina.Powers;
using Furina.Characters.Furina.Cards;

namespace Furina.Characters.Furina.Relics;

[RegisterRelic(typeof(FurinaRelicPool))]
[RegisterCharacterStarterRelic(typeof(Furina))]
public sealed class SoloistsSolicitation : BaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<string> RegisteredKeywordIds => 
        new List<string> { "FURINA_KEYWORD_ARKHE" };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => 
        HoverTipFactory.FromCardWithCardHoverTips<SeatsSacredAndSecular>();

    public override async Task BeforeCombatStart()
    {
        await PowerCmd.Apply<OusiaPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, null);
    }

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != Owner)
            return;

        if (Owner.Creature?.CombatState == null)
            return;

        var card = Owner.Creature.CombatState.CreateCard<SeatsSacredAndSecular>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
    }
}