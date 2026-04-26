using Godot;

public partial class HitBoxComponent : Node3D
{
	private HealthComponent healthComponent;
	private Node3D parent;

	public override void _Ready()
	{
		parent = GetParent<Node3D>();
		healthComponent = GetParent().GetNode<HealthComponent>("HealthComponent");
	}

	public void Damage(Attack attack)
	{
		healthComponent.Damage(attack);
		if (parent is CharacterBody3D characterBody)
		{
			Vector3 direction = (characterBody.GlobalPosition - attack.AttackOrigin).Normalized();
			direction.Y = 0f;

			Vector2 currentHorizontal = new Vector2(characterBody.Velocity.X, characterBody.Velocity.Z);
			float maxKnockback = 15f;
			if (currentHorizontal.Length() < maxKnockback)
			{
				characterBody.Velocity += direction * attack.Knockback;
			}
		}
	}
}
