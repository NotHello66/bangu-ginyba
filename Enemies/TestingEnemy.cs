using Godot;
using System;

public partial class TestingEnemy : CharacterBody3D
{
    // Stats
    [Export] private float speed = 5f;
    [Export] private float knockBackResist = 10f;

    // Position Rotation
    private Vector3 lastDirection = Vector3.Zero;
    [Export] float bodyRotationSpeed = 5f;

    // Dead timer
    private double deadTimer = 0;

    // Child Nodes
    private HealthComponent healthComponent;
    private NavigationAgent3D navigationAgent3D;

    // Player Node
    private Node3D player;

    public override void _Ready()
    {
        healthComponent = GetNode("HealthComponent") as HealthComponent;
        navigationAgent3D = GetNode("NavigationAgent3D") as NavigationAgent3D;
        player = GetTree().GetFirstNodeInGroup("Player") as Node3D;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("debug_moveTestEnemyToRandomPos"))
        {
            Vector3 randomPosition = Vector3.Zero;
            Random r = new Random();
            randomPosition.X = r.Next(-5, 5);
            randomPosition.Z = r.Next(-5, 5);
            navigationAgent3D.SetTargetPosition(randomPosition);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Navigation Agent 3D
        if (player != null)
        {
            navigationAgent3D.TargetPosition = player.GlobalPosition;
        }
        else
        {
            GD.Print("Player is null");
        }
        var destination = navigationAgent3D.GetNextPathPosition();
        var localDestination = destination - GlobalPosition;
        var direction = localDestination.Normalized();

        Vector3 targetVelocity = direction * speed;

        Vector3 currentVelocity = Velocity;

        // Rotation 
        if (direction != Vector3.Zero)
        {
            lastDirection = direction;
        }
        Rotation = new Vector3(Rotation.X, (float)Mathf.LerpAngle(Rotation.Y, Mathf.Atan2(lastDirection.X, lastDirection.Z), delta * bodyRotationSpeed), Rotation.Z);

        // Gravity
        if (!IsOnFloor())
        {
            currentVelocity += GetGravity() * (float)delta;
        }

        // Knockback
        currentVelocity.X = Mathf.MoveToward(currentVelocity.X, targetVelocity.X, (float)delta * knockBackResist);
        currentVelocity.Z = Mathf.MoveToward(currentVelocity.Z, targetVelocity.Z, (float)delta * knockBackResist);

        // Dying
        if (healthComponent.isDead)
        {
            currentVelocity.X = Mathf.MoveToward(Velocity.X, 0, (float)delta * knockBackResist);
            currentVelocity.Z = Mathf.MoveToward(Velocity.Z, 0, (float)delta * knockBackResist);

            if (deadTimer < healthComponent.deathDespawnTimer)
            {
                deadTimer += delta;
                Vector3 fallDirection = Velocity.Normalized();
                fallDirection = fallDirection.Normalized();
                Vector3 fallAxis = Vector3.Up.Cross(fallDirection).Normalized();
                if (fallAxis.LengthSquared() > 0.0001f)
                {
                    RotateObjectLocal(fallAxis, GetGravity().Length() * (float)delta);
                }
            }
            else
            {
                QueueFree();
            }
        }

        Velocity = currentVelocity;
        MoveAndSlide();
	}
}