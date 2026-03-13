using Godot;
using System;

public partial class EnemySlotManager : Node3D
{
    [Export] public int MaxSlots = 6; 
    [Export] public float Radius = 2.0f; 
    [Export] public float ShuffleInterval = 3f;

    private Node3D[] occupiedSlots;
    private double shuffleTimer = 0;

    public override void _Ready()
    {
        occupiedSlots = new Node3D[MaxSlots];
    }

    public override void _PhysicsProcess(double delta)
    {
        shuffleTimer += delta;

        if (shuffleTimer >= ShuffleInterval)
        {
            shuffleTimer = 0;
            RotateSlots();
            ShuffleSlots();
        }
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

    public int GetEnemySlotIndex(Node3D enemy)
    {
        return Array.IndexOf(occupiedSlots, enemy);
    }

    public void RotateSlots(int direction = 1)
    {
        if (MaxSlots <= 1) return;

        Node3D[] newSlots = new Node3D[MaxSlots];
        for (int i = 0; i < MaxSlots; i++)
        {
            int newIndex = (i + direction) % MaxSlots;
            if (newIndex < 0) newIndex += MaxSlots;

            newSlots[newIndex] = occupiedSlots[i];
        }
        occupiedSlots = newSlots;
    }

    public void ShuffleSlots()
    {
        Random rand = new Random();
        for (int i = MaxSlots - 1; i > 0; i--)
        {
            int j = rand.Next(i + 1);
            Node3D temp = occupiedSlots[i];
            occupiedSlots[i] = occupiedSlots[j];
            occupiedSlots[j] = temp;
        }
    }
}