using Godot;

public partial class GoldPickup : RigidBody3D
{
    private float value;

    public void SetValue(float amount) => value = amount;

    public override void _Ready()
    {
        Freeze = false;
        GravityScale = 1.0f;
        LinearDamp = 0.5f;
        AngularDamp = 0.5f;

        var pickupArea = GetNode<Area3D>("PickupArea");
        pickupArea.BodyEntered += OnPickupAreaBodyEntered;
    }

    private void OnPickupAreaBodyEntered(Node3D body)
    {
        if (body is PlayerController player)
        {
            var economy = player.GetNodeOrNull<EconomyComponent>("EconomyComponent")
                       ?? GetTree().Root.FindChild("EconomyComponent", true, false)
                          as EconomyComponent;

            if (economy != null)
            {
                economy.AddGold(value);
                QueueFree();
            }
        }
    }
}