using Godot;
using System;

public partial class MeleeComponent : Node3D
{
    [Export] PackedScene meleeAttackScene; 
    [Export] float damage = 10f;
    [Export] float knockbackForce = 5f;
    [Export] public float cooldown = 1.5f;

    float timer;
    private Attack attackData;

    public override void _Ready()
    {
        attackData = new Attack(damage, knockbackForce, GlobalPosition);
        timer = cooldown;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (timer < cooldown)
        {
            timer += (float)delta;
        }
    }

    public bool CanAttack()
    {
        return timer >= cooldown;
    }

    public void PerformMeleeAttack()
    {
        if (CanAttack())
        {
            timer = 0f;

            attackData ??= new Attack(damage, knockbackForce, GlobalPosition);

            if (meleeAttackScene == null)
            {
                GD.PrintErr("Melee attack scene is null");
                return;
            }

            AttackTorus attackNode = (AttackTorus)meleeAttackScene.Instantiate();
            GetTree().CurrentScene.AddChild(attackNode);
            attackNode.GlobalPosition = this.GlobalPosition;
            attackNode.Initialize(attackData);
        }
    }
}