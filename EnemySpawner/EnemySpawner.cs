using Godot;
using System;

public partial class EnemySpawner : Node3D
{
    [Export] private PackedScene enemyScene;
    [Export] private float respawnDelay = 3f;

    private TestingEnemy currentEnemy;

    public override void _Ready()
    {
        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        if (enemyScene == null)
        {
            GD.PrintErr("EnemySpawner: enemyScene is not assigned.");
            return;
        }

        currentEnemy = enemyScene.Instantiate<TestingEnemy>();
        AddChild(currentEnemy);
        currentEnemy.AddToGroup("Enemy");
        currentEnemy.Position = Vector3.Zero;
        currentEnemy.TreeExited += OnEnemyRemoved;
    }

    private async void OnEnemyRemoved()
    {
        await ToSignal(GetTree().CreateTimer(respawnDelay), SceneTreeTimer.SignalName.Timeout);
        SpawnEnemy();
    }
}
