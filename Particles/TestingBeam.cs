using Godot;
using System;

public partial class TestingBeam : GpuParticles3D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        OneShot = true;
        Finished += OnParticlesFinished;
        Emitting = true;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
    void OnParticlesFinished()
    {
        QueueFree();
    }
}
