using Godot;
using System;

public partial class EconomyComponent : Node3D
{
    [Export] float startingGold = 100f;
    public float currentGold{get; private set;}
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        currentGold = startingGold;
        GetTree().NodeAdded += OnNodeAdded;
        var rewardUI = GetTree().Root.FindChild("rewardSystemUI", true, false);
        if(rewardUI != null)
        {
            rewardUI.Connect("gold_changed", new Callable(this, nameof(OnGoldSpent)));
        }
    }

    private void OnNodeAdded(Node node)
    {
        if (node is TestingEnemy enemy)
        {
            enemy.EnemyKilled += OnEnemyKilled;
        }
    }
    private void OnGoldSpent(float amount)
    {
        currentGold -= amount;
        GD.Print($"Gold Spent: {amount}, Current Gold: {currentGold}");
    }
    private void OnEnemyKilled(float goldReward)
	{
		currentGold += goldReward;
		GD.Print($"Enemy killed Current Gold: {currentGold}");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
