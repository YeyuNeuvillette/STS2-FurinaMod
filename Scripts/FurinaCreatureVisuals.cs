using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Furina.Scripts;

internal partial class FurinaCreatureVisuals : NCreatureVisuals
{
    private Sprite2D? _ousiaSprite;
    private Sprite2D? _pneumaSprite;

    public override void _Ready()
    {
        base._Ready();
        
        var visuals = GetNode<Node2D>("%Visuals");
        _ousiaSprite = visuals.GetNode<Sprite2D>("Furina");
        _pneumaSprite = visuals.GetNode<Sprite2D>("FurinaPneuma");
    }

    public void SetArkheState(bool isOusia)
    {
        if (_ousiaSprite == null || _pneumaSprite == null)
            return;
            
        _ousiaSprite.Visible = isOusia;
        _pneumaSprite.Visible = !isOusia;
    }
}