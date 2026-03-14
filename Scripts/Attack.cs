using Godot;
using System;

public partial class Attack : Node3D
{
    protected float damage;
    protected float knockback;
    protected Vector3 origin;

    public float Damage => damage;
    public float Knockback => knockback;
    public Vector3 AttackOrigin => origin;

    public Attack() { }

    public Attack(float damage, float knockback, Vector3 origin)
    {
        this.damage = damage;
        this.knockback = knockback;
        this.origin = origin;
    }

    public virtual void Initialize(Attack data)
    {
        damage = data.damage;
        knockback = data.knockback;
        origin = data.origin;
    }
}