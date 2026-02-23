using Godot;
using System;

public partial class Attack : Node
{
	public float damage;
	public float knockbackForce;
	public Vector3 attackOrigin;
	public Attack(float damage, float knockbackForce, Vector3 attackOrigin)
	{
		this.damage = damage;
		this.knockbackForce = knockbackForce;
		this.attackOrigin = attackOrigin;
	}

}
