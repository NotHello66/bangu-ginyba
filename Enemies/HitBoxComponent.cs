using Godot;
using System;

public partial class HitBoxComponent : Node3D
{
	[Export] HealthComponent healthComponent;
	CharacterBody3D parent;
	public override void _Ready()
	{
		parent = GetParent() as CharacterBody3D;
	}
	public void Damage(Attack attack)
	{
		healthComponent.Damage(attack);
		Vector3 direction = (GlobalPosition - attack.attackOrigin).Normalized();
		parent.Velocity += direction * attack.knockbackForce;
	}
}
