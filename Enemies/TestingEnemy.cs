using Godot;
using System;

public partial class TestingEnemy : CharacterBody3D
{
    private float speed = 1f;
    [Export] private float knockBackResist = 10f;
    HealthComponent healthComponent;
    private double deadTimer = 0;
    public override void _Ready()
    {
        healthComponent = GetNode("HealthComponent") as HealthComponent;
    }
	public override void _PhysicsProcess(double delta)
	{

        Vector3 velocity = Velocity;
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }
        velocity.X = Mathf.MoveToward(Velocity.X, 0, (float) delta * knockBackResist);
        velocity.Z = Mathf.MoveToward(Velocity.Z, 0, (float)delta * knockBackResist);

        if (healthComponent.isDead && deadTimer < healthComponent.deathDespawnTimer)
        {
            deadTimer += delta;
            Vector3 fallDirection = Velocity;
            fallDirection = fallDirection.Normalized();
            Vector3 fallAxis = Vector3.Up.Cross(fallDirection);
            fallAxis = fallAxis.Normalized();
            if (fallAxis.LengthSquared() > 0.0001f)
            {
                RotateObjectLocal(fallAxis, GetGravity().Length() * (float)delta);
            }
        }
        else if(healthComponent.isDead) QueueFree();


        Velocity = velocity;
        MoveAndSlide();
	}
}
