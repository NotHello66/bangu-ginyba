using Godot;

public partial class MeleeComponent : Node3D
{
	[Export] private PackedScene[] attackScenes;
    [Export] private float[] damages;
    [Export] private float[] knockbackForces;
	[Export] public float[] cooldowns;
    [Export(PropertyHint.None, "Reikalingas tik objektam kurie patys nesisuka, tik suka mesha (realiai tik playeriui)")]
    public Node3D rotationSource;

    private float[] timers;


    public override void _Ready()
	{
        timers = new float[attackScenes.Length];
        Array.Fill(timers, 0f);
    }

    public override void _PhysicsProcess(double delta)
    {
        for (int i = 0; i < timers.Length; i++)
        {
            if (timers[i] < GetCooldown(i))
                timers[i] += (float)delta;
        }
    }

    public bool CanAttack(int index)
    {
        if (index < 0 || index >= attackScenes.Length) return false;
        return timers[index] >= GetCooldown(index);
    }

    public void PerformAttack(int index)
    {
        if (!CanAttack(index)) return;

        if (attackScenes[index] == null)
        {
            GD.PrintErr($"Attack scene at index {index} is null");
            return;
        }

        timers[index] = 0f;
        Vector3 facingRotation = GetFacingRotation();

        Attack attackNode = attackScenes[index].Instantiate<Attack>();
        GetParent().AddChild(attackNode);
        attackNode.GlobalPosition = GlobalPosition;
        attackNode.GlobalRotation = facingRotation;
        attackNode.Initialize(new Attack(GetDamage(index), GetKnockback(index), GlobalPosition));
    }
    public void PerformAttack()
    {
        for (int i = 0; i < attackScenes.Length; i++)
        {
            if (timers[i] >= GetCooldown(i))
            {
                timers[i] = 0f;
                Vector3 facingRotation = GetFacingRotation();

                Attack attackNode = attackScenes[i].Instantiate<Attack>();
                GetParent().AddChild(attackNode);
                attackNode.GlobalPosition = GlobalPosition;
                attackNode.GlobalRotation = facingRotation;
                attackNode.Initialize(new Attack(GetDamage(i), GetKnockback(i), GlobalPosition));
            }
        }
    }
    public float GetCooldown(int index)
    {
        // jei per mazai cooldownu, naudoja paskutini
        if (cooldowns == null || cooldowns.Length == 0) return 0f;
        return cooldowns[Mathf.Min(index, cooldowns.Length - 1)];
    }
    private float GetDamage(int index)
    {
        if (damages == null || damages.Length == 0) return 0f;
        return damages[Mathf.Min(index, damages.Length - 1)];
    }

    private float GetKnockback(int index)
    {
        if (knockbackForces == null || knockbackForces.Length == 0) return 0f;
        return knockbackForces[Mathf.Min(index, knockbackForces.Length - 1)];
    }
    private Vector3 GetFacingRotation()
    {
        if (rotationSource != null)
            return rotationSource.GlobalRotation;
        return GetParent<Node3D>().GlobalRotation;
    }
}
