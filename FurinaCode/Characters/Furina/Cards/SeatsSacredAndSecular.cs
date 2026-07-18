using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using Furina.Characters.Base;
using Furina.Characters.Furina.Powers;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(TokenCardPool))]
public sealed class SeatsSacredAndSecular : BaseCard
{
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Minion };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword> 
    { 
        CardKeyword.Ethereal, 
        CardKeyword.Exhaust,
        ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_ARKHE")
    };

    public SeatsSacredAndSecular()
        : base(0, CardType.Skill, CardRarity.Token, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await InvitationManager.SwitchArkhe(choiceContext, Owner, this);
    }
}
