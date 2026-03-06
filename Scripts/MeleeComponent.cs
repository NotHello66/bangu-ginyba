using Godot;
using System;

public partial class MeleeComponent : Node3D
{
    [Export] PackedScene torusAttackScene;
    [Export] PackedScene clawAttackScene;
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

    public void PerformTorusAttack()
    {
        if (CanAttack())
        {
            timer = 0f;

            attackData ??= new Attack(damage, knockbackForce, GlobalPosition);

            if (torusAttackScene == null)
            {
                GD.PrintErr("Melee attack scene is null");
                return;
            }

            AttackTorus attackNode = (AttackTorus)torusAttackScene.Instantiate();
            GetTree().CurrentScene.AddChild(attackNode);
            attackNode.GlobalPosition = this.GlobalPosition;
            attackNode.Initialize(attackData);
        }
    }

    public void PerformClawAttack()
    {
        if (CanAttack())
        {
            timer = 0f;

            attackData ??= new Attack(damage, knockbackForce, GlobalPosition);

            if (clawAttackScene == null)
            {
                GD.PrintErr("Melee attack scene is null");
                return;
            }

            AttackClaw attackNode = (AttackClaw)clawAttackScene.Instantiate();
            GetTree().CurrentScene.AddChild(attackNode);

            attackNode.GlobalPosition = this.GlobalPosition;
            attackNode.GlobalRotation = this.GlobalRotation;
            
            attackNode.Initialize(attackData);
        }
    }
}