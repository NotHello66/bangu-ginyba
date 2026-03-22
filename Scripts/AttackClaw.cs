using Godot;

public partial class AttackClaw : Attack
{
    [Export] private float swingAngleDegrees = 60f;
    [Export] private float windupTime = 0.2f;
    [Export] private float swingTime = 0.3f;

    private float timer = 0f;
    private float initialRotationX;

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

        initialRotationX = Rotation.X;

        float startAngle = initialRotationX - Mathf.DegToRad(swingAngleDegrees / 2);
        Rotation = new Vector3(startAngle, Rotation.Y, Rotation.Z);
    }

    public override void _Process(double delta)
    {
        timer += (float)delta;

        if (timer >= windupTime && timer <= windupTime + swingTime)
        {
            area.Monitoring = true;

            float progress = (timer - windupTime) / swingTime;

            float startAngle = initialRotationX + Mathf.DegToRad(swingAngleDegrees / 2);
            float endAngle = initialRotationX - Mathf.DegToRad(swingAngleDegrees / 2);

            float currentAngle = Mathf.Lerp(startAngle, endAngle, progress);
            Rotation = new Vector3(currentAngle, Rotation.Y, Rotation.Z);
        }
        else if (timer > windupTime + swingTime)
        {
            area.Monitoring = false;
            QueueFree();
        }
    }

    private void OnBodyEntered(Node body)
    {
        if (hasDamaged) return;

        if (body.IsInGroup("Player") || body.IsInGroup("Tower") || body.IsInGroup("Enemy"))
        {
            HitBoxComponent hitbox = body.GetParent()?.GetNodeOrNull<HitBoxComponent>("HitBoxComponent") ?? body.GetNodeOrNull<HitBoxComponent>("HitBoxComponent");
            if (hitbox != null)
            {
                hitbox.Damage(this);
                hasDamaged = true;
            }
        }
    }
}