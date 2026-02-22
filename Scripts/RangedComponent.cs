using Godot;
using System;

public partial class RangedComponent : Node3D
{
    [Export] PackedScene projectile;
    [Export] public bool isAOE;
    [Export] float damage;
    [Export] float knockbackForce;
    [Export] float projectileSpeed;
    [Export] float AoeRadius = 0f;
    [Export] float cooldown = 0f;
    float timer;

    private Attack attack;

    // Called when the node enters the scene tree for the first time
    public override void _Ready()
    {
        attack = new Attack(damage, knockbackForce, GlobalPosition);
        timer = 0f;
    }
    public override void _PhysicsProcess(double delta)
    {
        timer += (float)delta;
    }

    public void Fire(Node3D target)
    {
        if(timer >= cooldown) {
            timer = 0f;
            attack ??= new Attack(damage, knockbackForce, GlobalPosition);
            if (projectile == null)
            {
                GD.PrintErr("projectile is null");
            }
            if (attack == null)
            {
                GD.PrintErr("attack is null");
            }
            //if (target == null) GD.Print("baaaa");
            if (target != null)
            {
                ProjectileComponent projComp = (ProjectileComponent)projectile.Instantiate();
                GetTree().CurrentScene.AddChild(projComp);
                projComp.GlobalTransform = this.GlobalTransform;
                projComp.Initialize(target, projectileSpeed, attack, isAOE, AoeRadius);
            }
        }
    }
}
