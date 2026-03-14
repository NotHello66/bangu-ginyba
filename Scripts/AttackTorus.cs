using Godot;
using System;

public partial class AttackTorus : Attack
{
    [Export] private float growSpeed = 2f;
    [Export] private float spinSpeed = 5f;
    [Export] private float lifetime = 1.2f;

    private float timer = 0f;
    private bool hasDamaged = false;

    private Area3D area;

    public override void Initialize(Attack data)
    {
        base.Initialize(data);
    }

    public override void _Ready()
    {
        area = GetNode<Area3D>("Area3D");
        area.BodyEntered += OnBodyEntered;
        area.Monitoring = false;
        StartAttack();
    }

    public override void _Process(double delta)
    {
        Scale += Vector3.One * growSpeed * (float)delta;
        RotateY(spinSpeed * (float)delta);

        timer += (float)delta;
        if (timer >= lifetime)
            QueueFree();
    }

    private void OnBodyEntered(Node body)
    {
        if (hasDamaged) return;

        if (body.IsInGroup("Player"))
        {
            var hitbox = body.GetNodeOrNull<HitBoxComponent>("HitBoxComponent");

            if (hitbox != null)
            {
                hitbox.Damage(this);
                hasDamaged = true;
            }
        }
    }

    private async void StartAttack()
    {
        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
        area.Monitoring = true;

        await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
        QueueFree();
    }
}