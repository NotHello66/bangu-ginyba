using Godot;

public partial class AttackClawHorizontal : Attack
{
    [Export] private float swingAngleDegrees = 60f;
    [Export] private float windupTime = 0.2f;
    [Export] private float swingTime = 0.3f;
    [Export] private bool swingInwardFromRight = true;

    private float timer = 0f;
    private float initialRotationY;

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

        initialRotationY = Rotation.Z;

        float direction = swingInwardFromRight ? 1f : -1f;

        float startAngle = initialRotationY + Mathf.DegToRad(swingAngleDegrees / 2) * direction;
        Rotation = new Vector3(Rotation.X, startAngle, Rotation.Z);
    }

    public override void _Process(double delta)
    {
        timer += (float)delta;

        if (timer >= windupTime && timer <= windupTime + swingTime)
        {
            area.Monitoring = true;

            float progress = (timer - windupTime) / swingTime;

            float direction = swingInwardFromRight ? 1f : -1f;
            float startAngle = initialRotationY + Mathf.DegToRad(swingAngleDegrees / 2) * direction;
            float endAngle = initialRotationY - Mathf.DegToRad(swingAngleDegrees / 2) * direction;

            float currentAngle = Mathf.Lerp(startAngle, endAngle, progress);
            Rotation = new Vector3(Rotation.X, currentAngle, Rotation.Z);
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
            HitBoxComponent hitbox = body.GetParent()?.GetNodeOrNull<HitBoxComponent>("HitBoxComponent")
                                     ?? body.GetNodeOrNull<HitBoxComponent>("HitBoxComponent");
            if (hitbox != null)
            {
                hitbox.Damage(this);
                hasDamaged = true;
            }
        }
    }
}