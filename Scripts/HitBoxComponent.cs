using Godot;
using System;

public partial class HitBoxComponent : Node3D
{
	private HealthComponent healthComponent;
	private CharacterBody3D parent;
	public override void _Ready()
	{
		parent = GetParent<CharacterBody3D>();
        healthComponent = GetParent().GetNode<HealthComponent>("HealthComponent");
    }
    public void Damage(Attack attack)
	{
		healthComponent.Damage(attack);
		Vector3 direction = (GlobalPosition - attack.AttackOrigin).Normalized();
		parent.Velocity += direction * attack.Knockback;
	}
}
