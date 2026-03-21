using Godot;

public partial class MeleeComponent : Node3D
{
	[Export] private PackedScene attackScene;
	[Export] private float damage = 10f;
	[Export] private float knockbackForce = 5f;
	[Export] public float cooldown = 1.5f;

	private float timer;

	public override void _Ready()
	{
		timer = cooldown;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (timer < cooldown)
		{
			timer += (float)delta;
		}
	}

	public bool CanAttack()
	{
		return timer >= cooldown;
	}

	public void PerformAttack()
	{
		if (!CanAttack())
			return;

		timer = 0f;

		if (attackScene == null)
		{
			GD.PrintErr("Melee attack scene is null");
			return;
		}

		Attack attackNode = attackScene.Instantiate<Attack>();
		GetParent().AddChild(attackNode);

		attackNode.GlobalPosition = this.GlobalPosition;
		attackNode.GlobalRotation = this.GlobalRotation;

		attackNode.Initialize(new Attack(damage, knockbackForce, GlobalPosition));
	}
}
