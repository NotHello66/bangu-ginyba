using Godot;
using System;

public partial class RangedComponent : Node3D
{
	[Export] PackedScene projectile;
	[Export] public bool isAOE = false;
	[Export] public float damage = 10f;
	[Export] private float knockbackForce = 5f;
	[Export] private float projectileSpeed = 10f;
	[Export] private float aoeRadius = 0f;
	[Export] public float cooldown = 1.5f;
	[Export] private float turnSpeed = 7f;

	float timer;

	public override void _Ready()
	{
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

	public void Fire(Node3D target)
	{
		if (!CanAttack())
			return;

		if (projectile == null)
		{
			GD.PrintErr("projectile is null");
			return;
		}

		timer = 0f;

		ProjectileComponent proj = projectile.Instantiate<ProjectileComponent>();
		GetTree().CurrentScene.AddChild(proj);

		proj.GlobalPosition = GlobalPosition + (GlobalTransform.Basis.Z * 1.5f);

		Attack attackData = new Attack(damage, knockbackForce, GlobalPosition);
		proj.Initialize(target, GetParent<Node3D>(), projectileSpeed, attackData, isAOE, aoeRadius, turnSpeed);
	}
}
