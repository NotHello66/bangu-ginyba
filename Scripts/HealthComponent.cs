using Godot;

public partial class HealthComponent : Node3D
{
	[Signal] public delegate void HealthChangedEventHandler(float current, float max);
	[Signal] public delegate void PlayerDiedEventHandler();
	[Export] private float MaxHP;
	public float HP;
	public bool isDead = false;
	public double deathDespawnTimer = 2;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		HP = MaxHP;
	}

	public void Damage(Attack attack)
	{
		HP -= attack.Damage;
		if (HP <= 0)
		{
			isDead = true;
			EmitSignal(SignalName.PlayerDied);
		}
		EmitSignal(SignalName.HealthChanged, HP, MaxHP);
		GD.Print($"| {GetParent().Name} | HP: {HP} | isDead: {isDead} |");
		}
}
