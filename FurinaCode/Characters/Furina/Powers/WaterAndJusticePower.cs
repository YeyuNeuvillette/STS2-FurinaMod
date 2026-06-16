using System.Linq;
using System.Threading.Tasks;
using Furina.Characters.Base;
using Furina.Characters.Furina.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Powers;

[RegisterPower]
public sealed class WaterAndJusticePower : FurinaPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        var player = Owner.Player;
        if (player == null)
            return;

        var targetEntry = ModelDb.GetEntry(typeof(WaterAndJustice));
        var cardInDeck = player.Deck.Cards
            .FirstOrDefault(c => c.Id.Entry == targetEntry);

        if (cardInDeck != null)
        {
            await CardPileCmd.RemoveFromDeck(cardInDeck);
        }
    }
}