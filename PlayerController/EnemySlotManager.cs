using Godot;
using System;

public partial class EnemySlotManager : Node3D
{
    [Export] public int MaxSlots = 6; 
    [Export] public float Radius = 2.0f; 

    private Node3D[] occupiedSlots;

    public override void _Ready()
    {
        occupiedSlots = new Node3D[MaxSlots];
    }

    public int RequestSlot(Node3D enemy)
    {
        for (int i = 0; i < MaxSlots; i++)
        {
            if (occupiedSlots[i] == null)
            {
                occupiedSlots[i] = enemy;
                return i;
            }
        }
        return -1; 
    }

    public void ReleaseSlot(int slotIndex, Node3D enemy)
    {
        if (slotIndex >= 0 && slotIndex < MaxSlots && occupiedSlots[slotIndex] == enemy)
        {
            occupiedSlots[slotIndex] = null;
        }
    }

    public Vector3 GetSlotPosition(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots)
            return GlobalPosition;

        float angle = (Mathf.Pi * 2 / MaxSlots) * slotIndex;

        float offsetX = Mathf.Cos(angle) * Radius;
        float offsetZ = Mathf.Sin(angle) * Radius;

        return GlobalPosition + new Vector3(offsetX, 0, offsetZ);
    }
}