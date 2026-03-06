using Godot;
using System;

public partial class TestingEnemy : CharacterBody3D
{
    // Stats
    [Export] private float speed = 5f;
    [Export] private float knockBackResist = 10f;
    public int enemyLevel;

    // Position Rotation
    private Vector3 lastDirection = Vector3.Zero;
    [Export] float bodyRotationSpeed = 5f;

    // Combat framework
    [Export] private double stunTimer = 2;
    [Export] private float attackRange = 2f;
    [Export] private float stopChaseDistance = 1.8f;
    [Export] private MeleeComponent meleeComponent;
    [Export] private float jumpForce = 5f;
    private bool isAttacking = false;
    private bool isJumping = false;
    private double attackTimer = 0;
    private double deadTimer = 0;
    public EnemyState currentState = EnemyState.Idle;

    // Child Nodes
    public HealthComponent healthComponent;
    private NavigationAgent3D navigationAgent3D;

    // Player Node
    private Node3D player;

    // Slot Management
    private EnemySlotManager slotManager;
    private int mySlotIndex = -1;

    public override void _Ready()
    {
        healthComponent = GetNode("HealthComponent") as HealthComponent;
        navigationAgent3D = GetNode("NavigationAgent3D") as NavigationAgent3D;
        player = GetTree().GetFirstNodeInGroup("Player") as Node3D;

        if (player != null)
        {
            slotManager = player.GetNodeOrNull<EnemySlotManager>("EnemySlotManager");
        }
    }

    public override void _Process(double delta)
    {

    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 currentVelocity = Velocity;
        UpdateTimers(delta);

        if (currentState != EnemyState.Dead && currentState != EnemyState.Stunned)
        {
            HandleRotation(delta);
        }

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

    public override void _ExitTree()
    {
        if (slotManager != null && mySlotIndex != -1)
        {
            slotManager.ReleaseSlot(mySlotIndex, this);
            mySlotIndex = -1;
        }
    }

    private Vector3 HandleNavigation()
    {
        if (player == null)
        {
            GD.Print("Player is null");
            return Vector3.Zero;
        }

        if (mySlotIndex == -1 && slotManager != null)
        {
            mySlotIndex = slotManager.RequestSlot(this);
        }

        Vector3 targetPos;
        if (mySlotIndex != -1 && slotManager != null)
        {
            targetPos = slotManager.GetSlotPosition(mySlotIndex);
        }
        else
        {
            targetPos = player.GlobalPosition;
        }

        navigationAgent3D.TargetPosition = targetPos;

        var destination = navigationAgent3D.GetNextPathPosition();
        var direction = (destination - GlobalPosition).Normalized();

        return direction;
    }

    private void HandleMovement(ref Vector3 velocity, Vector3 targetVelocity, double delta)
    {
        velocity.X = Mathf.MoveToward(velocity.X, targetVelocity.X, (float)delta * knockBackResist);
        velocity.Z = Mathf.MoveToward(velocity.Z, targetVelocity.Z, (float)delta * knockBackResist);
    }

    private void HandleRotation(double delta)
    {
        if (player == null) return;

        Vector3 direction = (player.GlobalPosition - GlobalPosition).Normalized();

        direction.Y = 0;

        if (direction != Vector3.Zero)
        {
            lastDirection = direction;
        }

        Rotation = new Vector3(
            Rotation.X,
            (float)Mathf.LerpAngle(Rotation.Y, Mathf.Atan2(lastDirection.X, lastDirection.Z), (float)(delta * bodyRotationSpeed)),
            Rotation.Z
        );
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
        currentVelocity.X = 0;
        currentVelocity.Z = 0;

        HandleGravity(ref currentVelocity, delta);

        if (!isJumping && !isAttacking)
        {
            if (CanPerformAttack())
            {
                PerformJump(ref currentVelocity);
            }
            else if (!IsPlayerInAttackRange())
            {
                ChangeState(EnemyState.Chasing);
            }
        }
        else if (isJumping)
        {
            if (IsOnFloor() && currentVelocity.Y <= 0)
            {
                isJumping = false;
                PerformSlamAttack();
            }
        }
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

        if (isJumping)
            return false;

        if (attackTimer > 0f)
            return false;

        if (!IsPlayerInAttackRange())
            return false;

        if (currentState == EnemyState.Stunned)
            return false;

        if (healthComponent.isDead)
            return false;

        if (meleeComponent != null && !meleeComponent.CanAttack())
            return false;

        return true;
    }

    private void PerformSlamAttack()
    {
        if (meleeComponent != null)
        {
            meleeComponent.PerformMeleeAttack();
            attackTimer = meleeComponent.cooldown;
        }
        else
        {
            GD.PrintErr("MeleeComponent is missing on TestingEnemy!");
        }
    }

    private void PerformJump(ref Vector3 currentVelocity)
    {
        isJumping = true;
        isAttacking = true;

        currentVelocity.Y = jumpForce;
    }

    private void HandleChasing(ref Vector3 currentVelocity, double delta)
    {
        float distance = player.GlobalPosition.DistanceSquaredTo(GlobalPosition);

        float attackRangeSq = attackRange * attackRange;
        float stopChaseSq = stopChaseDistance * stopChaseDistance;

        if (distance <= attackRangeSq)
        {
            ChangeState(EnemyState.Attacking);
            return;
        }

        Vector3 direction = HandleNavigation();

        if (distance <= stopChaseSq)
            direction = Vector3.Zero;

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

    public enum EnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Stunned,
        Dead
    }

    public void SetEnemyLevel(int level)
    {
        this.enemyLevel = level;
        GD.Print($"++++++++++++++++++++++++++++++++++++++ Enemy Stats ++++++++++++++++++++++++++++++++++++++");
        GD.Print($"                                 Level: {enemyLevel} ");
    }
}
