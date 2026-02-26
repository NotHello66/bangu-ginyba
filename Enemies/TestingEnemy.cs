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

    // Combat framework
    [Export] private double stunTimer = 2;
    [Export] private float attackRange = 2f;
    [Export] private float stopChaseDistance = 1.8f;
    [Export] private MeleeComponent meleeComponent;
    private bool isAttacking = false;
    private double attackTimer = 0;
    private double deadTimer = 0;
    private EnemyState currentState = EnemyState.Idle;

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

    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 currentVelocity = Velocity;
        UpdateTimers(delta);

        if (healthComponent.isDead && currentState != EnemyState.Dead)
        {
            ChangeState(EnemyState.Dead);
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdle(ref currentVelocity, delta);
                break;

            case EnemyState.Chasing:
                HandleChasing(ref currentVelocity, delta);
                break;

            case EnemyState.Attacking:
                HandleAttacking(ref currentVelocity, delta);
                break;

            case EnemyState.Stunned:
                HandleStunned(ref currentVelocity, delta);
                break;

            case EnemyState.Dead:
                HandleDead(ref currentVelocity, delta);
                break;
        }

        Velocity = currentVelocity;
        MoveAndSlide();
    }

    private Vector3 HandleNavigation()
    {
        if (player == null)
        {
            GD.Print("Player is null");
            return Vector3.Zero;
        }

        navigationAgent3D.TargetPosition = player.GlobalPosition;

        var destination = navigationAgent3D.GetNextPathPosition();
        var direction = (destination - GlobalPosition).Normalized();

        return direction;
    }

    private void HandleMovement(ref Vector3 velocity, Vector3 targetVelocity, double delta)
    {
        velocity.X = Mathf.MoveToward(velocity.X, targetVelocity.X, (float)delta * knockBackResist);
        velocity.Z = Mathf.MoveToward(velocity.Z, targetVelocity.Z, (float)delta * knockBackResist);
    }

    private void HandleRotation(Vector3 direction, double delta)
    {
        if (direction != Vector3.Zero)
            lastDirection = direction;

        Rotation = new Vector3(Rotation.X, (float)Mathf.LerpAngle(Rotation.Y, Mathf.Atan2(lastDirection.X, lastDirection.Z), (float)(delta * bodyRotationSpeed)), Rotation.Z);
    }

    private void HandleGravity(ref Vector3 velocity, double delta)
    {
        if (!IsOnFloor())
            velocity += GetGravity() * (float)delta;
    }

    private void HandleDeath(ref Vector3 velocity, double delta)
    {
        velocity.X = Mathf.MoveToward(velocity.X, 0, (float)delta * knockBackResist);
        velocity.Z = Mathf.MoveToward(velocity.Z, 0, (float)delta * knockBackResist);

        if (deadTimer < healthComponent.deathDespawnTimer)
        {
            deadTimer += delta;
            Vector3 fallDirection = Velocity.Normalized();
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

    private void ChangeState(EnemyState newState)
    {
        currentState = newState;
    }

    private void HandleDead(ref Vector3 currentVelocity, double delta)
    {
        // Drop Some Loot
        HandleDeath(ref currentVelocity, delta);
    }

    private void HandleStunned(ref Vector3 currentVelocity, double delta)
    {
        //currentVelocity = Vector3.Zero;
        stunTimer -= delta;
        // spining stars above head (worst case change collor)

        if (stunTimer <= 0)
            ChangeState(EnemyState.Chasing);
    }

    private void HandleAttacking(ref Vector3 currentVelocity, double delta)
    {
        currentVelocity = Vector3.Zero;

        //FacePlayer(delta); //Rotate towards player

        if (!IsPlayerInAttackRange())
            ChangeState(EnemyState.Chasing);

        if (CanPerformAttack())
            PerformAttack();
    }

    private bool IsPlayerInAttackRange()
    {
        if (player == null)
            return false;

        return player.GlobalPosition.DistanceSquaredTo(GlobalPosition) <= attackRange * attackRange;
    }

    private bool CanPerformAttack()
    {
        if (isAttacking)
            return false;

        if (attackTimer > 0f)
            return false;

        if (!IsPlayerInAttackRange())
            return false;

        if (currentState == EnemyState.Stunned)
            return false;

        if (healthComponent.isDead)
            return false;

        // Check if the component itself is off cooldown
        if (meleeComponent != null && !meleeComponent.CanAttack())
            return false;

        return true;
    }

    private void PerformAttack()
    {
        isAttacking = true;

        if (meleeComponent != null)
        {
            // Sync the enemy's state timer with the component's cooldown
            attackTimer = meleeComponent.cooldown;
            meleeComponent.PerformMeleeAttack();
        }
        else
        {
            GD.PrintErr("MeleeComponent is missing on TestingEnemy!");
        }

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
            ChangeState(EnemyState.Attacking);
            return;
        }

        Vector3 direction = HandleNavigation();

        // Stop moving when close enough
        if (distance <= stopChaseSq)
            direction = Vector3.Zero;

        HandleRotation(direction, delta);

        Vector3 targetVelocity = direction * speed;

        HandleGravity(ref currentVelocity, delta);
        HandleMovement(ref currentVelocity, targetVelocity, delta);
    }

    private void HandleIdle(ref Vector3 currentVelocity, double delta)
    {
        currentVelocity = Vector3.Zero;

        //if (CanSeePlayer()) // || !player.isDead)
        ChangeState(EnemyState.Chasing);
    }

    private void UpdateTimers(double delta)
    {
        if (attackTimer > 0)
            attackTimer -= delta;

        if (attackTimer <= 0)
            isAttacking = false;
    }

    private enum EnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Stunned,
        Dead
    }
}
