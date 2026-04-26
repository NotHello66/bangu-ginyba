using Godot;
using System;

public partial class MantisBossEnemy : Enemy
{
    protected MeleeComponent secondaryMeleeComponent;

    public override void _Ready()
    {
        base._Ready();

        secondaryMeleeComponent = GetNodeOrNull<MeleeComponent>("SecondaryMeleeComponent");
    }

    protected override void HandleAttacking(ref Vector3 currentVelocity, double delta)
    {
        bool willAttackThisFrame = CanPerformAttack();

        base.HandleAttacking(ref currentVelocity, delta);

        if (willAttackThisFrame && secondaryMeleeComponent != null)
        {
            secondaryMeleeComponent.PerformAttack(0);

            double secondaryCooldown = secondaryMeleeComponent.GetCooldown(0);
            if (secondaryCooldown > attackTimer)
            {
                attackTimer = secondaryCooldown;
            }
        }
    }
}