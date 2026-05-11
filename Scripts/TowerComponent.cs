using Godot;

public partial class TowerComponent : Node3D
{
	private RangedComponent rangedComponent;
	private HitBoxComponent hitBoxComponent;
	public HealthComponent healthComponent;

	public bool isPreview = false;
	private bool isRanged = false;
	// private bool isAoe = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		rangedComponent = GetParent().GetNodeOrNull<RangedComponent>("RangedComponent");
		healthComponent = GetParent().GetNodeOrNull<HealthComponent>("HealthComponent");
		hitBoxComponent = GetParent().GetNodeOrNull<HitBoxComponent>("HitBoxComponent");

		if (rangedComponent != null)
		{
			isRanged = true;
			// if(rangedComponent.isAOE == true) isAoe = true;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (isPreview) return; // Checks if the tower is a display tower
		if (healthComponent.isDead)
		{
			GetParent().QueueFree();
			return;
		}
		Enemy enemy = GetClosestEnemy();
		if (isRanged && enemy != null)
		{
			rangedComponent.Fire(enemy);
		}
	}

	public override void _PhysicsProcess(double delta)
	{ }

	private Enemy GetClosestEnemy()
	{
		var enemies = GetTree().GetNodesInGroup("Enemy");
		var spaceState = GetWorld3D().DirectSpaceState;

		Enemy closestVisible = null;
		float closestVisibleDistance = float.MaxValue;

		foreach (Node node in enemies)
		{
			if (node is not Enemy enemy) continue;
			if (enemy.healthComponent.isDead) continue;

			float distance = GlobalPosition.DistanceTo(enemy.GlobalPosition);

			if (CanSeeEnemy(enemy, spaceState) && distance < closestVisibleDistance)
			{
				closestVisibleDistance = distance;
				closestVisible = enemy;
			}
		}

		//if (closestVisible == null) GD.Print("TOWER: no visible enemies");

		return closestVisible;
	}

	private bool CanSeeEnemy(Enemy enemy, PhysicsDirectSpaceState3D spaceState)
	{
		var exclude = new Godot.Collections.Array<Rid>();

		var towerBody = GetParent<StaticBody3D>();
		if (towerBody != null)
			exclude.Add(towerBody.GetRid());

		// exclude the enemy itself
		exclude.Add(enemy.GetRid());

		var query = PhysicsRayQueryParameters3D.Create(
			GlobalPosition,
			enemy.GlobalPosition,
			collisionMask: 1,
			exclude: exclude
		);

		var result = spaceState.IntersectRay(query);
		return result.Count == 0;
	}
}
