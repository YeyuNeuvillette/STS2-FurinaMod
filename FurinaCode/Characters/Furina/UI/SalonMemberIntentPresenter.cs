using Furina.Characters.Furina.Actions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Furina.Characters.Furina.UI;

public partial class SalonMemberIntentPresenter : Node
{
    private const string PresenterNodeName = "FurinaSalonMemberIntentPresenter";
    private const string IntentNodeName = "FurinaSalonMemberIntent";

    private NCreature? _creatureNode;
    private PowerModel? _action;
    private NIntent? _intentNode;
    private int _lastAttackDamage = int.MinValue;
    private bool _lastVisible;

    internal static void TryAttach(NCreature creatureNode)
    {
        if (!InvitationManager.IsSalonMember(creatureNode.Entity)
            || creatureNode.GetNodeOrNull<SalonMemberIntentPresenter>(PresenterNodeName) != null)
            return;

        var presenter = new SalonMemberIntentPresenter
        {
            Name = PresenterNodeName,
            _creatureNode = creatureNode
        };
        creatureNode.AddChildSafely(presenter);
    }

    internal static void TryRefresh(NCreature creatureNode)
    {
        if (!InvitationManager.IsSalonMember(creatureNode.Entity))
            return;

        creatureNode.GetNodeOrNull<SalonMemberIntentPresenter>(PresenterNodeName)
            ?.Bind(ResolveAction(creatureNode.Entity));
    }

    public override void _Process(double delta)
    {
        if (_creatureNode is null || !GodotObject.IsInstanceValid(_creatureNode))
            return;

        var currentAction = ResolveAction(_creatureNode.Entity);
        if (!ReferenceEquals(currentAction, _action) || !IsIntentNodeAttached())
            Bind(currentAction);

        Refresh();
    }

    public override void _ExitTree()
    {
        RemoveIntentNode();
        _action = null;
        _creatureNode = null;
    }

    private static PowerModel? ResolveAction(Creature creature)
    {
        return creature.Monster switch
        {
            Minions.GentilhommeUsher => creature.GetPower<HydroBarrage>(),
            Minions.MademoiselleCrabaletta => creature.GetPower<CrabShield>(),
            Minions.SurintendanteChevalmarin => creature.GetPower<PureWaterNote>(),
            _ => null
        };
    }

    private void Bind(PowerModel? action)
    {
        RemoveIntentNode();
        _action = action;
        _lastAttackDamage = int.MinValue;
        _lastVisible = false;

        if (action is null || _creatureNode is null)
            return;

        AbstractIntent intent = action switch
        {
            HydroBarrage hydroBarrage => new SalonAttackIntent(() => hydroBarrage.IntentDamage),
            CrabShield => new SalonDefendIntent(),
            PureWaterNote => new SalonBuffIntent(),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action.GetType(), "Unsupported salon action.")
        };

        _intentNode = NIntent.Create((float)_creatureNode.GetHashCode() / 100f);
        _intentNode.Name = IntentNodeName;
        _creatureNode.IntentContainer.AddChildSafely(_intentNode);
        _intentNode.UpdateIntent(intent, Array.Empty<Creature>(), _creatureNode.Entity);
        _intentNode.Visible = false;
    }

    private void Refresh()
    {
        if (_creatureNode is null || _action is null || !IsIntentNodeAttached())
            return;

        var shouldShow = _creatureNode.Entity.IsAlive && _action.DisplayAmount > 0;
        if (shouldShow != _lastVisible)
        {
            _intentNode!.Visible = shouldShow;
            _lastVisible = shouldShow;
        }

        if (_action is HydroBarrage hydroBarrage && hydroBarrage.IntentDamage != _lastAttackDamage)
        {
            _lastAttackDamage = hydroBarrage.IntentDamage;
            _intentNode!.UpdateIntent(
                new SalonAttackIntent(() => hydroBarrage.IntentDamage),
                Array.Empty<Creature>(),
                _creatureNode.Entity);
        }
    }

    private bool IsIntentNodeAttached()
    {
        return _intentNode is not null
            && GodotObject.IsInstanceValid(_intentNode)
            && _intentNode.GetParent() == _creatureNode?.IntentContainer;
    }

    private void RemoveIntentNode()
    {
        if (_intentNode is not null && GodotObject.IsInstanceValid(_intentNode))
            _intentNode.QueueFreeSafely();

        _intentNode = null;
    }

    private sealed class SalonAttackIntent(Func<int> damageProvider) : AttackIntent
    {
        public override bool HasIntentTip => false;

        public override int GetTotalDamage(IEnumerable<Creature> targets, Creature owner) =>
            Math.Max(0, damageProvider());

        public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
        {
            var label = new LocString("intents", "FORMAT_DAMAGE_SINGLE");
            label.Add("Damage", GetTotalDamage(targets, owner));
            return label;
        }
    }

    private sealed class SalonDefendIntent : DefendIntent
    {
        public override bool HasIntentTip => false;
    }

    private sealed class SalonBuffIntent : BuffIntent
    {
        public override bool HasIntentTip => false;
    }
}
