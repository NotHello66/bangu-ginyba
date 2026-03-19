using Godot;
using System;

public partial class HitBoxComponent : Node3D
{
	private HealthComponent healthComponent;
	private Node3D parent;

    public override void _Ready()
	{
		parent = GetParent<Node3D>();
        healthComponent = GetParent().GetNode<HealthComponent>("HealthComponent");
    }
    public void Damage(Attack attack)
	{
		healthComponent.Damage(attack);
        if (parent is CharacterBody3D characterBody)
        {
            Vector3 direction = (GlobalPosition - attack.AttackOrigin).Normalized();
            characterBody.Velocity += direction * attack.Knockback;
        }
    }
}
