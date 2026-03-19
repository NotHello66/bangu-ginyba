using Godot;
using System;
using System.Collections.Generic;

public partial class EnemySpawner : Node3D
{
    [ExportGroup("Enemey Scenes")]
    [Export] private PackedScene scorpionEnemyScene;
    [Export] private PackedScene grasshopperEnemyScene;
    [Export] private PackedScene bombardierbeetleEnemyScene;
    private List<PackedScene> availableScenes;

    [ExportGroup("Stats")]
    [Export] private float spawnDelay = 0.5f;
    [Export] private int waveLevelToSpawnGrassHopperEnemy = 5;
    [Export] private int waveLevelToSpawnBombardierEnemy = 10;
    private float spawnTimer = 0f;
    private int maxEnemies = 5;
    private int enemyLevel = 0;
    private int waveLevel = 0;
    private int enemiesToSpawn = 0;
    private int enemiesInScene = 0;
    private RandomNumberGenerator rng = new RandomNumberGenerator();

    [Signal] public delegate void WaveFinishedEventHandler();
    public override void _Ready()
    {
        availableScenes = new List<PackedScene>
        {
            scorpionEnemyScene
        };
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("debug_StartNewWave") && enemiesInScene == 0)
        {
            waveLevel++;

            if (waveLevel >= waveLevelToSpawnGrassHopperEnemy && !availableScenes.Contains(grasshopperEnemyScene))
                availableScenes.Add(grasshopperEnemyScene);
            if (waveLevel >= waveLevelToSpawnBombardierEnemy && !availableScenes.Contains(bombardierbeetleEnemyScene))
                availableScenes.Add(bombardierbeetleEnemyScene);

            string poolNames = string.Join(", ", availableScenes.ConvertAll(scene => scene.ResourcePath.GetFile()));
            GD.Print($"Wave Level: {waveLevel} | Enemy Pool: [{poolNames}]");

            enemiesToSpawn = 0;

            SpawnEnemyWave();
        }
    }

    private async void SpawnEnemyWave()
    {
        while (enemiesToSpawn < maxEnemies)
        {
            SpawnEnemy();
            await ToSignal(GetTree().CreateTimer(spawnDelay), SceneTreeTimer.SignalName.Timeout);
        }
    }

    private void SpawnEnemy()
    {
        int randomIndex = rng.RandiRange(0, availableScenes.Count - 1);
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
        enemy.SetName("Testing Enemy nr:" + enemyLevel);
        enemy.SetEnemyLevel(enemyLevel); 

        enemiesToSpawn++;
        enemyLevel++;
        enemiesInScene++;

        enemy.TreeExited += OnEnemyRemoved;
    }

    private async void OnEnemyRemoved()
    {
        enemiesInScene--;
        if (enemiesInScene == 0) EmitSignal(SignalName.WaveFinished);
    }
}