using Godot;

public partial class GoldPickup : Area3D
{
    private float value;

    public void SetValue(float amount) => value = amount;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is PlayerController player)
        {
            // Walk up the tree to find EconomyComponent
            var economy = player.GetNodeOrNull<EconomyComponent>("EconomyComponent")
                       ?? GetTree().Root.FindChild("EconomyComponent", true, false) as EconomyComponent;

            if (economy != null)
            {
                economy.AddGold(value);
                QueueFree();
            }
        }
    }
}