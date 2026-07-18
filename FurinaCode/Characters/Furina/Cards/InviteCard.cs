using Furina.Characters.Base;
using Furina.Characters.Furina.Minions;
using Furina.Characters.Furina.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Monsters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Furina.Characters.Furina.Cards;

public abstract class InviteCard(
    int energyCost,
    CardType type,
    CardRarity rarity,
    TargetType target,
    bool shouldShowInCardLibrary = true)
    : FurinaCard(energyCost, type, rarity, target, shouldShowInCardLibrary)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_INVITE")
    ];

    protected abstract Type SelectMinionType(PlayerChoiceContext choiceContext);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        if (Owner.Creature.HasPower<OusiaPower>())
            await InvitationManager.InviteOusia(choiceContext, Owner, this, SelectMinionType(choiceContext));
        else if (Owner.Creature.HasPower<PneumaPower>())
            await InvitationManager.InvitePneuma(choiceContext, Owner, this);
    }

    protected Task HandleOusiaState(PlayerChoiceContext choiceContext) =>
        HandleOusiaState(choiceContext, SelectMinionType(choiceContext));

    protected Task HandleOusiaState(PlayerChoiceContext choiceContext, Type minionType) =>
        Owner == null
            ? Task.CompletedTask
            : InvitationManager.InviteOusia(choiceContext, Owner, this, minionType);

    protected Task HandlePneumaState(PlayerChoiceContext choiceContext) =>
        Owner == null
            ? Task.CompletedTask
            : InvitationManager.InvitePneuma(choiceContext, Owner, this);
}

public abstract class FixedInvitationCard<TMinion> : InviteCard, KnowledgeDemon.IChoosable
{
    public override bool CanBeGeneratedInCombat => false;
    public override int MaxUpgradeLevel => 0;

    protected FixedInvitationCard()
        : base(-1, CardType.Skill, CardRarity.Status, TargetType.None, shouldShowInCardLibrary: false)
    {
    }

    protected override Type SelectMinionType(PlayerChoiceContext choiceContext) => typeof(TMinion);

    public Task OnChosen() => Invite(new ThrowingPlayerChoiceContext());

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Invite(choiceContext);

    protected virtual Task Invite(PlayerChoiceContext choiceContext)
    {
        return Owner == null
            ? Task.CompletedTask
            : InvitationManager.InviteOusia(choiceContext, Owner, this, typeof(TMinion));
    }
}

[RegisterCard(typeof(TokenCardPool))]
public sealed class InviteGentilhommeUsher : FixedInvitationCard<GentilhommeUsher>
{
}

[RegisterCard(typeof(TokenCardPool))]
public sealed class InviteMademoiselleCrabaletta : FixedInvitationCard<MademoiselleCrabaletta>
{
}

[RegisterCard(typeof(TokenCardPool))]
public sealed class InviteSurintendanteChevalmarin : FixedInvitationCard<SurintendanteChevalmarin>
{
}

[RegisterCard(typeof(FurinaCardPool))]
public sealed class InviteSingerOfManyWaters : FixedInvitationCard<SingerOfManyWaters>
{
    protected override Task Invite(PlayerChoiceContext choiceContext)
    {
        return Owner == null
            ? Task.CompletedTask
            : InvitationManager.InvitePneuma(choiceContext, Owner, this);
    }
}
