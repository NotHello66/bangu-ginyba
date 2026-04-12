using Godot;

public partial class Enemy : CharacterBody3D
{
	[ExportGroup("Stats")]
	[Export] private float speed = 5f;

	[Export] private float knockBackResist = 10f;
	private int enemyLevel;

	[ExportGroup("Position & Rotation")]
	[Export] private float bodyRotationSpeed = 5f;

	[Export] private float WobbleAmplitude = 3.0f;
	[Export] private float WobbleFrequency = 4.0f;
	private float randomPhaseOffset;
	private Vector3 lastDirection = Vector3.Zero;

	[ExportGroup("Combat framework")]
	[Export] private double stunTimer = 2;

	[Export] private float attackRange = 2f;
	[Export] private float stopChaseDistance = 1.8f;
	[Export] private float jumpForce = 5f;
	[Export] private bool isJumpingType = false;
	[Export] private bool isFlyingType = false;
	protected bool isAttacking = false;
	protected double attackTimer = 0;
	protected bool isFlying = false;
	private bool isJumping = false;
	private double deadTimer = 0;
	private EnemyState currentState = EnemyState.Idle;
	private bool IsMelee => meleeComponent != null;
	private bool IsRanged => rangedComponent != null;

	// Child Nodes
	public HealthComponent healthComponent;

	private NavigationAgent3D navigationAgent3D;
	protected MeleeComponent meleeComponent;
	protected RangedComponent rangedComponent;

	// Target System
	protected Node3D currentTarget;

	private float currentTargetRadius = 0f;
	private double targetSearchTimer = 0;
	private const double TargetSearchInterval = 0.5;
	private EnemySlotManager slotManager;

	[ExportGroup("Rewards")]
	[Export] private float GoldReward = 5f;

	[Signal]
	public delegate void EnemyKilledEventHandler(float goldReward);

	public void SetEnemyLevel(int level)
	{
		this.enemyLevel = level;
		GD.Print($"++++++++++++++++++++++++++++++++++++++ Enemy Stats ++++++++++++++++++++++++++++++++++++++");
		GD.Print($"                                Level: {enemyLevel} ");
	}

	protected virtual void ChangeState(EnemyState newState)
	{
		currentState = newState;
	}

	public override void _Ready()
	{
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
		if (!HasValidTarget()) return;
		float distance = currentTarget.GlobalPosition.DistanceSquaredTo(GlobalPosition);

		float effectiveAttackRange = attackRange + currentTargetRadius;
		float effectiveStopChase = stopChaseDistance + currentTargetRadius;

		float attackRangeSq = effectiveAttackRange * effectiveAttackRange;
		float stopChaseSq = effectiveStopChase * effectiveStopChase;

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
		if (!HasValidTarget())
		{
			ChangeState(EnemyState.Idle);
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
		var player = GetTree().GetFirstNodeInGroup("Player");
		var towers = GetTree().GetNodesInGroup("Tower");

		Node3D closest = null;
		float closestDist = float.MaxValue;

		if (player is PlayerController pc)
		{
			if (!pc.healthComponent.isDead)
			{
				float dist = GlobalPosition.DistanceSquaredTo(pc.GlobalPosition);
				if (dist < closestDist)
				{
					closestDist = dist;
					closest = pc;
				}
			}
		}

		foreach (Node3D t in towers)
		{
			TowerComponent towerComp = t.GetNodeOrNull<TowerComponent>("TowerComponent");

			if (towerComp != null && !towerComp.isPreview)
			{
				if (towerComp.healthComponent != null && towerComp.healthComponent.isDead) continue;

				float dist = GlobalPosition.DistanceSquaredTo(t.GlobalPosition);
				if (dist < closestDist)
				{
					closestDist = dist;
					closest = t;
				}
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

			slotManager = currentTarget.GetNodeOrNull<EnemySlotManager>("EnemySlotManager") ?? currentTarget.GetNodeOrNull<EnemySlotManager>("TowerComponent/EnemySlotManager");
		}
		else
		{
			slotManager = null;
		}
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

	protected virtual void HandleAttacking(ref Vector3 currentVelocity, double delta)
	{
		if (!HasValidTarget()) return;

		HandleGravity(ref currentVelocity, delta);

		if (!IsTargetInAttackRange())
		{
			ChangeState(EnemyState.Chasing);
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
                    meleeComponent.PerformAttack(0);
                    attackTimer = meleeComponent.GetCooldown(0);
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
                    meleeComponent.PerformAttack(0);
                    attackTimer = meleeComponent.GetCooldown(0);
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
			return false;

		return true;
	}

	protected virtual bool CanPerformAttack()
	{
		if (isAttacking)
			return false;

		if (isJumping)
			return false;

		if (attackTimer > 0f)
			return false;

		if (!IsTargetInAttackRange())
			return false;

		if (currentState == EnemyState.Stunned)
			return false;

		if (healthComponent.isDead)
			return false;

		return true;
	}

	#endregion #################################################################### Attack State ###################################################################

	private void HandleGravity(ref Vector3 velocity, double delta)
	{
		if (!isFlying && !isFlyingType && !IsOnFloor())
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
