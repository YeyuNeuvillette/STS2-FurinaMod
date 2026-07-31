using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Base;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Combat;
using STS2RitsuLib.Keywords;
using MegaCrit.Sts2.Core.Rooms;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(TokenCardPool))]
public sealed class Script : BaseCard
{
    private CardPile? _scriptPile;
    private bool _hasSubscribed = false;

    public override bool CanBeGeneratedInCombat => false;

    public override int MaxUpgradeLevel => 0;

    public Script()
        : base(0, CardType.Skill, CardRarity.Token, TargetType.None, shouldShowInCardLibrary: false)
    {
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword>
    {
        CardKeyword.Retain,
        ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_SCRIPT")
    };

    protected override int CanonicalEnergyCost => 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.PlayerCombatState == null)
            return;

        var scriptPile = TropeSystem.GetPile(Owner);

        if (scriptPile.Cards.Count == 0)
            return;

        var cardsToPlay = scriptPile.Cards.AsEnumerable().Reverse().ToList();
        using (TropeSystem.BeginReplay())
        {
            foreach (var card in cardsToPlay)
            {
                if (CombatManager.Instance.IsOverOrEnding)
                    break;

                await CardCmd.AutoPlay(choiceContext, card, null, AutoPlayType.Default, skipXCapture: true, skipCardPileVisuals: false);
            }
        }
    }

    internal void BindToScriptPile()
    {
        if (Owner?.PlayerCombatState == null)
            return;

        if (_hasSubscribed)
            return;

        var scriptPile = TropeSystem.GetPile(Owner);
        
        if (scriptPile != _scriptPile)
        {
            if (_scriptPile != null)
                _scriptPile.ContentsChanged -= OnScriptPileContentsChanged;
            
            _scriptPile = scriptPile;
            
            _scriptPile.ContentsChanged += OnScriptPileContentsChanged;
            _hasSubscribed = true;
            OnScriptPileContentsChanged();
        }
    }

    private void OnScriptPileContentsChanged()
    {
        if (Owner?.PlayerCombatState == null)
            return;

        var scriptPile = TropeSystem.GetPile(Owner);
        
        var totalCost = scriptPile.Cards
            .Select(card => card.EnergyCost.GetWithModifiers(CostModifiers.All))
            .Sum();
        
        var finalCost = Math.Min(totalCost, 3);
        
        EnergyCost.SetCustomBaseCost(finalCost);
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (_scriptPile != null)
            _scriptPile.ContentsChanged -= OnScriptPileContentsChanged;
        _scriptPile = null;
        _hasSubscribed = false;
        await base.AfterCombatEnd(room);
    }
}
