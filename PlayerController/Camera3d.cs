using Godot;
using System;

public partial class Camera3d : Camera3D
{
	[Export] Node3D Target;
	[Export] float lerpSpeed;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GlobalPosition = GlobalPosition.Lerp(Target.GlobalPosition, lerpSpeed);

	}
}
