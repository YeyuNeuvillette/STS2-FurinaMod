using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Furina.Characters.Furina.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Furina.Characters.Furina.Powers;

public interface IArkheDualEffectProvider
{
    bool ShouldApplyBothEffects { get; }
    Task OnArkheCardPlayed(PlayerChoiceContext choiceContext, CardModel cardSource);
}

public static class ArkheDualEffectHelper
{
    public static bool HasActiveProvider(Creature creature)
    {
        return creature.Powers.OfType<IArkheDualEffectProvider>().Any(p => p.ShouldApplyBothEffects);
    }

    public static IArkheDualEffectProvider? GetFirstActiveProvider(Creature creature)
    {
        return creature.Powers.OfType<IArkheDualEffectProvider>().FirstOrDefault(p => p.ShouldApplyBothEffects);
    }

    public static void SetForceNoneStateForHand(Creature creature, bool forceNone)
    {
        if (creature.Player == null)
            return;

        foreach (var card in PileType.Hand.GetPile(creature.Player).Cards)
        {
            if (card is OusiaPneumaCard ousiaPneumaCard)
            {
                ousiaPneumaCard.ForceNoneState = forceNone;
                ousiaPneumaCard.UpdateArkheState();
            }
        }
    }

    public static void OnCardDrawn(Creature creature, CardModel card)
    {
        if (card is OusiaPneumaCard ousiaPneumaCard)
        {
            ousiaPneumaCard.ForceNoneState = HasActiveProvider(creature);
            ousiaPneumaCard.UpdateArkheState();
        }
    }

    public static void OnProviderRemoved(Creature oldOwner)
    {
        if (oldOwner.Player == null)
            return;

        bool stillActive = HasActiveProvider(oldOwner);
        SetForceNoneStateForHand(oldOwner, stillActive);
    }
}