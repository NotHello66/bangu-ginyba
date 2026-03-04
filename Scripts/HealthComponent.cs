using Godot;
using System;

public partial class HealthComponent : Node3D
{
	[Export] float MaxHP;
	float HP;
	public bool isDead = false;
	public double deathDespawnTimer = 2;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		HP = MaxHP;
	}

	public void Damage(Attack attack)
	{
		HP -= attack.damage;
		if(HP <= 0)
			isDead = true;
		GD.Print($"| { GetParent().Name} | HP: {HP} | isDead: {isDead} |");
	}
}
