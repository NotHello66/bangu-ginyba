using Godot;
using System;

public partial class EnemySpawner : Node3D
{
    [ExportGroup("Enemey Scenes")]
    [Export] private PackedScene scorpionEnemyScene;
    [Export] private PackedScene grasshopperEnemyScene;
    [Export] private PackedScene bombardierbeetleEnemyScene;

    [ExportGroup("Stats")]
    [Export] int maxEnemies;
    [Export] private float spawnDelay = 2f;
    private float spawnTimer = 0f;
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
        PackedScene[] availableScenes = { scorpionEnemyScene, grasshopperEnemyScene, bombardierbeetleEnemyScene};

        int randomIndex = rng.RandiRange(0, availableScenes.Length - 1);
        PackedScene chosenScene = availableScenes[randomIndex];

        if (chosenScene == null)
        {
            GD.PrintErr("EnemySpawner: A chosen enemy scene is not assigned in the inspector!");
            return;
        }

        var enemy = chosenScene.Instantiate<Enemy>(); 
        
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