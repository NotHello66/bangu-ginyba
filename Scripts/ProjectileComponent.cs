using Godot;
using System;

public partial class ProjectileComponent : Node3D
{
	Node3D target;
	float speed;
	Attack attack;
	bool isAoe;
	float aoeRadius;
	public float turnspeed = 5f;

	public void Initialize(Node3D target, float speed, Attack attack, bool isAoe, float AoeRadius = 0f)
	{
		this.target = target;
		this.speed = speed;
		this.attack = attack;
		this.isAoe = isAoe;
		this.aoeRadius = AoeRadius;

	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public override void _PhysicsProcess(double delta)
	{
		if(target == null || !IsInstanceValid(target))
		{
			QueueFree();
		}
		Vector3 toTarget;
		try
		{
			toTarget = (target.GlobalPosition - GlobalPosition).Normalized();
		}
		catch (ObjectDisposedException)
		{
			QueueFree();
			return;
		}

		// Smooth rotation
		Quaternion currentRotation = GlobalTransform.Basis.GetRotationQuaternion();
		Quaternion targetRotation = Basis.LookingAt(toTarget, Vector3.Up).GetRotationQuaternion();
		Quaternion newRotation = currentRotation.Slerp(targetRotation, turnspeed * (float)delta);

		// Update GlobalTransform with new rotation, keep current position
		GlobalTransform = new Transform3D(new Basis(newRotation), GlobalPosition);

		// Move forward along local -Z in world space
		Vector3 forward = -GlobalTransform.Basis.Z;
		GlobalPosition += forward * speed * (float)delta;


		if (GlobalPosition.DistanceTo(target.GlobalPosition) < 1f)
		{
			if (isAoe)
			{
				foreach (Node enemyNode in GetTree().GetNodesInGroup("Enemy"))
				{
					if (enemyNode is Node3D enemy)
					{
						if (enemy.GlobalPosition.DistanceTo(GlobalPosition) <= aoeRadius)
						{
							HitBoxComponent hitbox = enemy.GetNodeOrNull<HitBoxComponent>("HitBoxComponent");
							if (hitbox != null)
							{
								hitbox.Damage(attack);
								QueueFree();
							}
						}
					}
				}
			}
			else
			{
				if (GlobalPosition.DistanceTo(target.GlobalPosition) < 1f)
				{
					HitBoxComponent hitbox = target.GetNodeOrNull<HitBoxComponent>("HitBoxComponent");
					if (hitbox != null)
					{
						hitbox.Damage(attack);
						QueueFree();
					}
				}
			}
		}
		
	}
}
