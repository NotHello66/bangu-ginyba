using Godot;
using System;

public partial class CameraPosTarget : Node3D
{
	[Export] Node3D target;
	Vector3 offset = new Vector3();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		offset = Position;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition = target.GlobalPosition + offset;
	}
}
