using Godot;

public partial class ProjectileComponent : Attack
{
	private Node3D target;
	private float speed;
	private bool isAoe;
	private float aoeRadius;
	private float turnspeed;

	private Area3D area;
	private Node3D owner;

	public void Initialize(Node3D target, Node3D owner, float speed, Attack data, bool isAoe, float aoeRadius, float turnSpeed)
	{
		base.Initialize(data);

		this.target = target;
		this.owner = owner;
		this.speed = speed;
		this.isAoe = isAoe;
		this.aoeRadius = aoeRadius;
		this.turnspeed = turnSpeed;
	}

	public override void _Ready()
	{
		area = GetNode<Area3D>("Area3D");
		area.BodyEntered += OnBodyEntered;
		area.Monitoring = true;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (target == null || !IsInstanceValid(target))
		{
			QueueFree();
			return;
		}

		Vector3 direction = (target.GlobalPosition - GlobalPosition).Normalized();

		Quaternion currentRotation = GlobalTransform.Basis.GetRotationQuaternion();
		Quaternion targetRotation = Basis.LookingAt(direction, Vector3.Up).GetRotationQuaternion();

		Quaternion newRotation = currentRotation.Slerp(targetRotation, turnspeed * (float)delta);

		GlobalTransform = new Transform3D(new Basis(newRotation), GlobalPosition);

		Vector3 forward = -GlobalTransform.Basis.Z;
		GlobalPosition += forward * speed * (float)delta;
	}

	private void OnBodyEntered(Node body)
	{
		if (body == owner)
			return;

		HitBoxComponent hitbox = body.GetParent()?.GetNodeOrNull<HitBoxComponent>("HitBoxComponent") ?? body.GetNodeOrNull<HitBoxComponent>("HitBoxComponent");
		if (hitbox != null)
		{
			hitbox.Damage(this);
			QueueFree();
		}
	}
}
