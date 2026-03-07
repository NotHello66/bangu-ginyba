using Godot;
using System;

public partial class EnemySpawner : Node3D
{
    [Export] private PackedScene enemyScene;
    [Export] int maxEnemies;
    private float spawnTimer = 0f;
    [Export] private float spawnDelay = 2f;
    int enemyCount;
    int spawnCount;
    private RandomNumberGenerator rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        spawnCount = 0;
    }

    public override void _PhysicsProcess(double delta)
    {
        spawnTimer -= (float)delta;

        if (spawnTimer <= 0f)
        {
            if (enemyCount < maxEnemies)
            {
                SpawnEnemy();
                spawnTimer = spawnDelay;
            }
        }
    }

    private void SpawnEnemy()
    {
        if (enemyScene == null)
        {
            GD.PrintErr("EnemySpawner: enemyScene is not assigned.");
            return;
        }
        var enemy = enemyScene.Instantiate<TestingEnemy>();
        GetTree().CurrentScene.AddChild(enemy);
        
        Vector3 randomOffset = new Vector3(rng.RandfRange(-1f, 1f), 0f, rng.RandfRange(-1f, 1f));
        enemy.GlobalPosition = GlobalPosition + randomOffset;
        
        enemy.AddToGroup("Enemy");
        enemy.SetName("Testing Enemy nr:" + spawnCount);
        enemy.SetEnemyLevel(spawnCount);

        enemyCount++;
        spawnCount++;

        enemy.TreeExited += OnEnemyRemoved;
    }

    private async void OnEnemyRemoved()
    {
        enemyCount--;
    }
}