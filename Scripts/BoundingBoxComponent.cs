using Godot;
using System;

public partial class BoundingBoxComponent : Node3D
{
	// Called when the node enters the scene tree for the first time.
	List<Node3D> enemies = new List<Node3D>();
	[Export]CharacterBody3D player;
	public override void _Ready()
	{
        GetTree().NodeAdded += OnNodeAdded;
		GetTree().NodeRemoved += OnNodeRemoved;
    }
    private void OnNodeAdded(Node node)
    {
        if (node.IsInGroup("Enemy") && node is NavigationAgent3D agent)
        {
            if (agent.GetParent() is CharacterBody3D body)
            {
                enemies.Add(body);
            }
        }
    }
    private void OnNodeRemoved(Node node)
	{
        if (node.IsInGroup("Enemy") && node is NavigationAgent3D agent)
        {
            if (agent.GetParent() is CharacterBody3D body)
            {
                enemies.Remove(body);
            }
        }
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
    public override void _PhysicsProcess(double delta)
    {
        if(player.GlobalPosition.Y <= -100f)
		{
			player.GlobalPosition = GlobalPosition;
        }
		foreach (Node enemy in enemies)
		{
            //GD.Print("Enemy type:" + enemy.GetType());

            if (enemy is CharacterBody3D enemyNode3D)
			{
				
				//GD.Print("AAAAAAAAAAAA");
				if (enemyNode3D.GlobalPosition.Y <= -100f)
				{
					enemy.QueueFree();
                    GD.Print("Enemy fell out of bounds and was removed.", enemy.Name);
                }
            }
        }
    }
}
