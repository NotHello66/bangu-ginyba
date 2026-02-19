using Godot;
using System;

public partial class RangedComponent : Node3D
{
    [Export] PackedScene projectile;
    [Export] bool isAOE;
    [Export] float damage;
    [Export] float knockbackForce;
    [Export] float projectileSpeed;
    [Export] float AoeRadius = 0f;

    private Attack attack;

    // Called when the node enters the scene tree for the first time
    public override void _Ready()
    {
        attack = new Attack(damage, knockbackForce, GlobalPosition);
    }

    public void Fire(Node3D target)
    {
        attack ??= new Attack(damage, knockbackForce, GlobalPosition);
        if (projectile == null)
        {
            GD.PrintErr("projectile is null");
        }
        if (attack == null)
        {
            GD.PrintErr("attack is null");
        }
        if (target != null)
        {
            ProjectileComponent projComp = (ProjectileComponent)projectile.Instantiate();
            GetTree().CurrentScene.AddChild(projComp);
            projComp.GlobalTransform = this.GlobalTransform;
            projComp.Initialize(target, projectileSpeed, attack, isAOE, AoeRadius);

        }
    }
}
