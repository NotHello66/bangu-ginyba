using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class PlayerController : CharacterBody3D
{
	[Export] public float speed = 5.0f;
    [Export] public float sprintSpeed = 15.0f;
    //public const float JumpVelocity = 4.5f;
    [Export] float rotationSpeed = 5f;
	float theta = new float();
    private Vector3 lastDirection = Vector3.Zero;
	private PlayerControllerMouse MouseController;
    public override void _Ready()
    {
        Node3D body = GetNode<Node3D>("%Meshes");
		MouseController = GetNode<PlayerControllerMouse>("PlayerControllerMouse");
		
    }

    public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		//// Handle Jump.
		//if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		//{
		//	velocity.Y = JumpVelocity;
		//}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		bool isSprinting = Input.IsActionPressed("sprint");
		float currentSpeed = isSprinting ? sprintSpeed : speed;
		Vector2 inputDir = Input.GetVector("moveLeft", "moveRight", "moveUp", "moveDown");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			lastDirection = direction;
			velocity.X = direction.X * currentSpeed;
			velocity.Z = direction.Z * currentSpeed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speed);
		}
		var body = GetNode<Node3D>("%Meshes");
        body.Rotation = new Vector3(body.Rotation.X, (float)Mathf.LerpAngle(body.Rotation.Y, Mathf.Atan2(lastDirection.X, lastDirection.Z), delta * rotationSpeed), body.Rotation.Z);
        Velocity = velocity;
		
		MoveAndSlide();
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			Vector3 hitpos = Vector3.Zero;
			GodotObject obj = null;
			if (MouseController.RayCastFromMouse(out hitpos, out obj) == true)
			{
				var beamScene = GD.Load<PackedScene>("res://Particles/TestingBeam.tscn");
				var beamInstance = beamScene.Instantiate();
				GetTree().CurrentScene.AddChild(beamInstance);
                if (beamInstance is Node3D beamNode3D)
				{
					beamNode3D.GlobalPosition = hitpos;
					GD.Print("hit");
					GD.Print($"Beam spawned at: {beamNode3D.GlobalPosition}");

                }
			}
		}
	}
}
