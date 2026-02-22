using Godot;
using System;

public partial class TowerComponent : Node3D
{
	[Export] RangedComponent rangedComponent;

	private bool isRanged = false;
//    private bool isAoe = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        if (rangedComponent != null)
        {
            isRanged = true;
           // if(rangedComponent.isAOE == true) isAoe = true;
        }
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        TestingEnemy enemy = GetClosestEnemy();
        if(isRanged)
        {
            rangedComponent.Fire(enemy);
        }
	}
    public override void _PhysicsProcess(double delta)
    {
    }
    private TestingEnemy GetClosestEnemy()
    {
        var enemies = GetTree().GetNodesInGroup("Enemy");

        TestingEnemy closest = null;
        float closestDistance = float.MaxValue;

        foreach (Node node in enemies)
        {
            if (node is TestingEnemy enemy)
            {
                float distance = GlobalPosition.DistanceTo(enemy.GlobalPosition);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = enemy;
                }
            }
        }
        return closest;
    }
}
