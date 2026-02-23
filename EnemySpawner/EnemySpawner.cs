using Godot;
using System;

public partial class EnemySpawner : Node3D
{
    [Export] private PackedScene enemyScene;
    [Export] private float respawnDelay = 3f;
    [Export] int maxEnemies = 1;
    int enemyCount;
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    public override async void _Ready()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        SpawnEnemy();
    }
    public override void _PhysicsProcess(double delta)
    {
    }

    private void SpawnEnemy()
    {
        if (enemyScene == null)
        {
            GD.PrintErr("EnemySpawner: enemyScene is not assigned.");
            return;
        }
        //GD.Print("Enemy count " + enemyCount + " out of max " + maxEnemies);
        int desiredEnemyAmount = maxEnemies - enemyCount;
        for (int i=0; i < desiredEnemyAmount; i++)
        {
            var enemy = enemyScene.Instantiate<TestingEnemy>();
            GetTree().CurrentScene.AddChild(enemy);
            Vector3 randomOffset = new Vector3(rng.RandfRange(-1f, 1f), 0f, rng.RandfRange(-1f, 1f));
            enemy.GlobalPosition = GlobalPosition + randomOffset;
            enemy.AddToGroup("Enemy");
            enemy.SetName("Testing Enemy nr:" + i);
            //GD.Print("spawned group" + enemy.IsInGroup("Enemy"));
            enemyCount++;
            //GD.Print("Enemy count " + enemyCount + " out of max " + maxEnemies + " i: " + i);
            enemy.TreeExited += OnEnemyRemoved;
        }
        
    }

    private async void OnEnemyRemoved()
    {
        enemyCount--;
        await ToSignal(GetTree().CreateTimer(respawnDelay), SceneTreeTimer.SignalName.Timeout);
        SpawnEnemy();
    }
}
