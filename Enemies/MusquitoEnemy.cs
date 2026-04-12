using Godot;

public partial class MusquitoEnemy : Enemy
{
	[ExportGroup("Mosquito Dash Stats")]
	[Export] private float dashSpeed = 15f;

	[Export] private float dashDuration = 0.5f;

	private bool isDashing = false;
	private double dashTimer = 0;
	private Vector3 dashDirection;

	protected override void HandleAttacking(ref Vector3 currentVelocity, double delta)
	{
		if (isDashing)
		{
			dashTimer -= delta;

			currentVelocity = dashDirection * dashSpeed;

			if (dashTimer <= 0)
			{
				isDashing = false;
				ChangeState(EnemyState.Idle);
                attackTimer = meleeComponent.GetCooldown(0);
            }
			return;
		}

		if (CanPerformAttack())
			StartDash(ref currentVelocity);
	}

	private void StartDash(ref Vector3 currentVelocity)
	{
		isDashing = true;
		dashTimer = dashDuration;

		dashDirection = (currentTarget.GlobalPosition - GlobalPosition).Normalized();
		dashDirection.Y = 0;

		meleeComponent.PerformAttack();
	}
}
