using Godot;

public partial class FriendlyUnit : CharacterBody3D
{
    [ExportGroup("Stats")]
    [Export] private float speed = 5f;

    [Export] private float knockBackResist = 10f;
    private int friendlyLevel;

    [ExportGroup("Position & Rotation")]
    [Export] private float bodyRotationSpeed = 5f;

    [Export] private float WobbleAmplitude = 3.0f;
    [Export] private float WobbleFrequency = 4.0f;
    [Export] float followDistance = 4f;
    private float followBuffer = 1.5f;
    private float randomPhaseOffset;
    private Vector3 lastDirection = Vector3.Zero;

    [ExportGroup("Combat framework")]
    [Export] private double stunTimer = 2;

    [Export] private float attackRange = 2f;
    [Export] private float stopChaseDistance = 1.8f;
    [Export] private float jumpForce = 5f;
    [Export] private bool isJumpingType = false;
    [Export] float chaseWhenStationedRange = 10f;
    [Export] float chaseWhenFollowingRange = 5f;

    private bool isAttacking = false;
    private bool isJumping = false;
    private double attackTimer = 0;
    private double deadTimer = 0;
    private FriendlyState currentState = FriendlyState.Idle;
    private FriendlyState globalState = FriendlyState.Following;
    private bool IsMelee => meleeComponent != null;
    private bool IsRanged => rangedComponent != null;

    // Child Nodes
    public HealthComponent healthComponent;

    private NavigationAgent3D navigationAgent3D;
    private MeleeComponent meleeComponent;
    private RangedComponent rangedComponent;

    // Target System
    private Node3D currentTarget;
    private float currentTargetRadius = 0f;

    private double targetSearchTimer = 0;
    private const double TargetSearchInterval = 0.5;
    private EnemySlotManager slotManager;

    //[ExportGroup("Rewards")]
    //[Export] private float GoldReward = 5f;

    //[Signal]
    //public delegate void EnemyKilledEventHandler(float goldReward);

    private Node _globalData;
    private Node3D player;

    
    public void SetFriendlyLevel(int level)
    {
        this.friendlyLevel = level;
    //    GD.Print($"++++++++++++++++++++++++++++++++++++++ Enemy Stats ++++++++++++++++++++++++++++++++++++++");
    //    GD.Print($"                                Level: {enemyLevel} ");
    }

    private void ChangeState(FriendlyState newState)
    {
        currentState = newState;
    }

    public override void _Ready()
    {
        _globalData = GetNode("/root/Global");
        _globalData.Connect("friendlyStateChanged", new Callable(this, nameof(OnGlobalStateChanged)));
        player = GetTree().GetFirstNodeInGroup("Player") as Node3D;
        
        healthComponent = GetNodeOrNull<HealthComponent>("HealthComponent");
        navigationAgent3D = GetNodeOrNull<NavigationAgent3D>("NavigationAgent3D");

        meleeComponent = GetNodeOrNull<MeleeComponent>("MeleeComponent");
        rangedComponent = GetNodeOrNull<RangedComponent>("RangedComponent");

        UpdateClosestTarget();

        randomPhaseOffset = (float)GD.RandRange(0.0, Mathf.Pi * 2);
    }

    public override void _Process(double delta)
    { }

    public override void _PhysicsProcess(double delta)
    {
        targetSearchTimer += delta;
        if (targetSearchTimer >= TargetSearchInterval)
        {
            UpdateClosestTarget();
            targetSearchTimer = 0;
        }

        Vector3 currentVelocity = Velocity;
        UpdateTimers(delta);

        if (currentState != FriendlyState.Attacking && currentState != FriendlyState.Stunned && currentState != FriendlyState.Chasing && currentState != FriendlyState.Stationed)
        {
            currentState = globalState;
        }
        if (currentState != FriendlyState.Dead && currentState != FriendlyState.Stunned)
        {
            HandleRotation(delta);
        }

        if (healthComponent.isDead && currentState != FriendlyState.Dead)
        {
            ChangeState(FriendlyState.Dead);

            //Could impliment a compensation system or a limited soldier slot system
            //EmitSignal(SignalName.EnemyKilled, GoldReward);
        }

        switch (currentState)
        {
            case FriendlyState.Idle:
                HandleIdle(ref currentVelocity, delta);
                break;

            case FriendlyState.Chasing:
                HandleChasing(ref currentVelocity, delta);
                break;
                //TODO: Add following and stationed states
            case FriendlyState.Following:
                HandleFollowing(ref currentVelocity, delta);
                break;
            case FriendlyState.Stationed:
                HandleStationed(ref currentVelocity, delta);
                break;
            case FriendlyState.Attacking:
                HandleAttacking(ref currentVelocity, delta);
                break;

            case FriendlyState.Stunned:
                HandleStunned(ref currentVelocity, delta);
                break;

            case FriendlyState.Dead:
                HandleDead(ref currentVelocity, delta);
                break;
        }

        Velocity = currentVelocity;
        MoveAndSlide();
        //GD.Print("Current state: " + currentState);
    }
    private void OnGlobalStateChanged(long newState)
    {
        globalState = (FriendlyState)newState;
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
        if (!HasValidTarget())
        {
            navigationAgent3D.TargetPosition = GlobalPosition;
            currentVelocity.X = 0;
            currentVelocity.Z = 0;
            ChangeState(globalState);
            return;
        }
        float distance = currentTarget.GlobalPosition.DistanceSquaredTo(GlobalPosition);

        float maxChaseSq = (globalState == FriendlyState.Following ? chaseWhenFollowingRange : chaseWhenStationedRange);
        maxChaseSq *= maxChaseSq * 2.25f;
        if (distance > maxChaseSq)
        {
            navigationAgent3D.TargetPosition = GlobalPosition;
            currentVelocity.X = 0;
            currentVelocity.Z = 0;
            ChangeState(globalState);
            return;
        }
        float effectiveAttackRange = attackRange + currentTargetRadius;
        float effectiveStopChase = stopChaseDistance + currentTargetRadius;

        float attackRangeSq = effectiveAttackRange * effectiveAttackRange;
        float stopChaseSq = effectiveStopChase * effectiveStopChase;

        if (distance <= attackRangeSq)
        {
            ChangeState(FriendlyState.Attacking);
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
        
        if (!HasValidTarget())
        {
            ChangeState(FriendlyState.Idle);
            return Vector3.Zero;
        }
        Vector3 targetPos = currentTarget.GlobalPosition;
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
        if (!HasValidTarget()) return;

        Vector3 direction = (currentTarget.GlobalPosition - GlobalPosition).Normalized();

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

    private void UpdateClosestTarget()
    {
        var enemies = GetTree().GetNodesInGroup("Enemy");

        Node3D closest = null;
        float closestDist = float.MaxValue;
        foreach (Node3D e in enemies)
        {
            HealthComponent enemyHC = e.GetNodeOrNull<HealthComponent>("HealthComponent");
           
            if (enemyHC != null && enemyHC.isDead) continue;

            float dist = GlobalPosition.DistanceSquaredTo(e.GlobalPosition);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = e;
            }
        }

        SetTarget(closest);
    }

    private void SetTarget(Node3D newTarget)
    {
        if (currentTarget == newTarget) return;

        if (slotManager != null)
        {
            int currentIndex = slotManager.GetEnemySlotIndex(this);
            if (currentIndex != -1)
            {
                slotManager.ReleaseSlot(currentIndex, this);
            }
        }

        currentTarget = newTarget;
        currentTargetRadius = 0f;

        if (HasValidTarget())
        {
            /*
             * leftover from enemy.cs
            if (currentTarget.IsInGroup("Tower"))
            {
                CollisionShape3D colShape = currentTarget.GetNodeOrNull<CollisionShape3D>("Area3D/CollisionShape3D");
                if (colShape != null && colShape.Shape != null)
                {
                    // Extract radius based on shape type
                    if (colShape.Shape is SphereShape3D sphere) currentTargetRadius = sphere.Radius;
                    else if (colShape.Shape is CylinderShape3D cylinder) currentTargetRadius = cylinder.Radius;
                    else if (colShape.Shape is BoxShape3D box) currentTargetRadius = Mathf.Max(box.Size.X, box.Size.Z) / 2f;
                }
            }
            */

            slotManager = currentTarget.GetNodeOrNull<EnemySlotManager>("EnemySlotManager") ?? currentTarget.GetNodeOrNull<EnemySlotManager>("TowerComponent/EnemySlotManager");
        }
        else
        {
            slotManager = null;
        }
    }
    private bool HandleStationed(ref Vector3 currentVelocity, double delta)
    {
        currentVelocity.X = 0;
        currentVelocity.Z = 0;

        if (!IsOnFloor())
            currentVelocity.Y += GetGravity().Y * (float)delta;
        else
            currentVelocity.Y = 0;

        // React to enemies
        if (HasValidTarget() &&
            GlobalPosition.DistanceTo(currentTarget.GlobalPosition) < chaseWhenStationedRange)
        {
            currentState = FriendlyState.Chasing;
        }
        if (globalState == FriendlyState.Following)
        {
            ChangeState(FriendlyState.Following);
        }
        return true;
    }
    private bool HandleFollowing(ref Vector3 currentVelocity, double delta)
    {
        if (HasValidTarget() && GlobalPosition.DistanceTo(currentTarget.GlobalPosition) < chaseWhenFollowingRange)
        {
            currentState = FriendlyState.Chasing;
            return true;
        }
        float id = (float)GetInstanceId();

        Vector3 offset = new Vector3(
            Mathf.Sin(id) * followDistance,
            0,
            Mathf.Cos(id) * followDistance
        );
        Vector3 targetPos = player.GlobalPosition + offset;
        float distance = GlobalPosition.DistanceTo(targetPos);
        float minDistance = followDistance - followBuffer;
        float maxDistance = followDistance + followBuffer;

        Vector3 direction = Vector3.Zero;

        if (distance > maxDistance)
        {
            direction = (targetPos - GlobalPosition).Normalized();
        }
        else if (distance < minDistance)
        {
            direction = (GlobalPosition - targetPos).Normalized();
        }
        else
        {
            direction = Vector3.Zero;
        }
        Vector3 targetVelocity = direction * speed;
        HandleGravity(ref currentVelocity, delta);
        HandleMovement(ref currentVelocity, targetVelocity, delta);
        return true;
    }

    #endregion #################################################################### Chase State ####################################################################

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

    #endregion #################################################################### Dead State ####################################################################

    #region #################################################################### Attack State ###################################################################

    private void HandleAttacking(ref Vector3 currentVelocity, double delta)
    {
        if (!HasValidTarget()) return;
        currentVelocity.X = 0;
        currentVelocity.Z = 0;

        HandleGravity(ref currentVelocity, delta);

        if (!IsTargetInAttackRange())
        {
            ChangeState(FriendlyState.Chasing);
            return;
        }

        if (CanPerformAttack())
        {
            if (IsMelee)
            {
                if (isJumpingType)
                {
                    PerformJump(ref currentVelocity);
                }
                else
                {
                    meleeComponent.PerformAttack();
                    attackTimer = meleeComponent.cooldown;
                }
            }
            else if (IsRanged)
            {
                rangedComponent.Fire(currentTarget);
                attackTimer = rangedComponent.cooldown;
            }
        }

        if (isJumping && isJumpingType)
        {
            if (IsOnFloor() && currentVelocity.Y <= 0)
            {
                isJumping = false;
                if (IsMelee)
                {
                    meleeComponent.PerformAttack();
                    attackTimer = meleeComponent.cooldown;
                }
            }
        }
    }

    private bool IsTargetInAttackRange()
    {
        if (!HasValidTarget()) return false;

        float effectiveAttackRange = attackRange + currentTargetRadius;
        return currentTarget.GlobalPosition.DistanceSquaredTo(GlobalPosition) <= effectiveAttackRange * effectiveAttackRange;
    }

    private bool HasValidTarget()
    {
        if (currentTarget == null || !IsInstanceValid(currentTarget))
        {
            currentTarget = null;
            return false;
        }
        return true;
    }

    private bool CanPerformAttack()
    {
        if (isAttacking)
            return false;

        if (isJumping)
            return false;

        if (attackTimer > 0f)
            return false;

        if (!IsTargetInAttackRange())
            return false;

        if (currentState == FriendlyState.Stunned)
            return false;

        if (healthComponent.isDead)
            return false;

        return true;
    }

    #endregion #################################################################### Attack State ###################################################################

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
            ChangeState(FriendlyState.Chasing);
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
        //if(currentTarget != null)
            ChangeState(FriendlyState.Stationed);
    }

    private void UpdateTimers(double delta)
    {
        if (attackTimer > 0)
            attackTimer -= delta;

        if (attackTimer <= 0)
            isAttacking = false;
    }

    public enum FriendlyState
    {
        Idle = 0,
        Chasing = 1,
        Following = 2,
        Stationed = 3,
        Attacking = 4,
        Stunned = 5,
        Dead = 6
    }
    //public FriendlyState GetGlobalState()
    //{
    //    return (FriendlyState)(int)(long)_globalState.Get("currentState");
    //}
    
}