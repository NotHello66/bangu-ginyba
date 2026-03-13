using Godot;
using System;

public partial class TestingEnemy : CharacterBody3D
{
    [ExportGroup("Stats")]
    [Export] private float speed = 5f;
    [Export] private float knockBackResist = 10f;
    private int enemyLevel;

    [ExportGroup("Position & Rotation")]
    [Export] float bodyRotationSpeed = 5f;
    [Export] private float WobbleAmplitude = 3.0f;
    [Export] private float WobbleFrequency = 4.0f;
    private float randomPhaseOffset;
    private Vector3 lastDirection = Vector3.Zero;

    [ExportGroup("Combat framework")]
    [Export] private double stunTimer = 2;
    [Export] private float attackRange = 2f;
    [Export] private float stopChaseDistance = 1.8f;
    [Export] private MeleeComponent meleeComponent;
    [Export] private float jumpForce = 5f;
    private bool isAttacking = false;
    private bool isJumping = false;
    private double attackTimer = 0;
    private double deadTimer = 0;
    private EnemyState currentState = EnemyState.Idle;

    // Child Nodes
    public HealthComponent healthComponent;
    private NavigationAgent3D navigationAgent3D;

    // Player Node
    private Node3D player;

    // Slot Management
    private EnemySlotManager slotManager;

    [ExportGroup("Rewards")]
    [Export] float GoldReward = 5f;
    [Signal]
    public delegate void EnemyKilledEventHandler(float goldReward);

    public void SetEnemyLevel(int level)
    {
        this.enemyLevel = level;
        GD.Print($"++++++++++++++++++++++++++++++++++++++ Enemy Stats ++++++++++++++++++++++++++++++++++++++");
        GD.Print($"                                Level: {enemyLevel} ");
    }
    
    private void ChangeState(EnemyState newState)
    {
        currentState = newState;
    }

    public override void _Ready()
    {
        healthComponent = GetNode("HealthComponent") as HealthComponent;
        navigationAgent3D = GetNode("NavigationAgent3D") as NavigationAgent3D;

        player = GetTree().GetFirstNodeInGroup("Player") as Node3D;
        if (player != null)
        {
            slotManager = player.GetNodeOrNull<EnemySlotManager>("EnemySlotManager");
        }

        randomPhaseOffset = (float)GD.RandRange(0.0, Mathf.Pi * 2);
    }

    public override void _Process(double delta){}

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
            EmitSignal(SignalName.EnemyKilled, GoldReward);
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
        if (slotManager != null)
        {
            int currentIndex = slotManager.GetEnemySlotIndex(this);
            if (currentIndex != -1)
            {
                slotManager.ReleaseSlot(currentIndex, this);
            }
        }
    }

    #region #################################################################### Chase State ####################################################################
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

        if (direction != Vector3.Zero)
        {
            Vector3 rightVector = Vector3.Up.Cross(direction).Normalized();

            float time = Time.GetTicksMsec() / 1000.0f;
            float wobbleMultiplier = Mathf.Sin((time * WobbleFrequency) + randomPhaseOffset) * WobbleAmplitude;

            targetVelocity += rightVector * wobbleMultiplier;
        }

        HandleGravity(ref currentVelocity, delta);
        HandleMovement(ref currentVelocity, targetVelocity, delta);
    }

    private Vector3 HandleNavigation()
    {
        if (player == null)
        {
            GD.Print("Player is null");
            return Vector3.Zero;
        }

        Vector3 targetPos = player.GlobalPosition; 

        if (slotManager != null)
        {
            int currentIndex = slotManager.GetEnemySlotIndex(this);

            if (currentIndex == -1)
            {
                currentIndex = slotManager.RequestSlot(this);
            }

            if (currentIndex != -1)
            {
                targetPos = slotManager.GetSlotPosition(currentIndex);
            }
        }

        navigationAgent3D.TargetPosition = navigationAgent3D.TargetPosition.Lerp(targetPos, 0.1f); ;

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
    #endregion

    #region #################################################################### Dead State ####################################################################
    private void HandleDead(ref Vector3 currentVelocity, double delta)
    {
        // Drop Some Loot
        HandleDeath(ref currentVelocity, delta);
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
    #endregion

    #region #################################################################### Attack State ####################################################################
    private void HandleAttacking(ref Vector3 currentVelocity, double delta)
    {
        if (meleeComponent == null)
        {
            GD.PrintErr("MeleeComponent is missing on TestingEnemy!");
            return;
        }

        currentVelocity.X = 0;
        currentVelocity.Z = 0;

        HandleGravity(ref currentVelocity, delta);

        if (!isJumping && !isAttacking)
        {
            if (CanPerformAttack())
            {
                if (enemyLevel < 5)
                {
                    PerformJump(ref currentVelocity);
                }
                else
                {
                    PerformStrikeAttack();
                    attackTimer = meleeComponent.cooldown;
                }
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
                attackTimer = meleeComponent.cooldown;
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
        meleeComponent.PerformTorusAttack();
    }

    private void PerformStrikeAttack()
    {
        meleeComponent.PerformClawAttack();
    }
    #endregion

    private void HandleGravity(ref Vector3 velocity, double delta)
    {
        if (!IsOnFloor())
            velocity += GetGravity() * (float)delta;
    }

    private void HandleStunned(ref Vector3 currentVelocity, double delta)
    {
        //currentVelocity = Vector3.Zero;
        stunTimer -= delta;
        // spining stars above head (worst case change collor)

        if (stunTimer <= 0)
            ChangeState(EnemyState.Chasing);
    }

    private void PerformJump(ref Vector3 currentVelocity)
    {
        isJumping = true;
        currentVelocity.Y = jumpForce;
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
}
