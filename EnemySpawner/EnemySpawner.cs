using Godot;
using System;
using System.Collections.Generic;

public partial class EnemySpawner : Node3D
{
	[Signal] public delegate void EnemyCountChangedEventHandler(int remaining);

	[Signal] public delegate void WaveStartedEventHandler(int wave, int totalEnemies);

	[Signal] public delegate void WaveFinishedEventHandler();

	[ExportGroup("Enemy Scenes")]
	[Export] private PackedScene[] enemyScenes;

	[Export] private int[] enemyUnlockLevels;

	[ExportGroup("Stats")]
	[Export] private float spawnDelay = 0.5f;

	[Export] private int maxEnemies = 5;

	[ExportGroup("Enemy Spawning Placement")]
	[Export] private float minSpawnSpacing = 1.5f;

	[Export] private float enemyRadius = 0.5f;
	[Export] private int maxPlacementAttempts = 30;

	private List<PackedScene> availableScenes = new List<PackedScene>();
	private List<Vector3> reservedSpawnPositions = new List<Vector3>();
	private RandomNumberGenerator rng = new RandomNumberGenerator();

	private int enemyLevel = 0;
	private int waveLevel = 0;
	private int enemiesToSpawn = 0;
	private int enemiesInScene = 0;

	public override void _Ready()
	{
		if (enemyScenes == null || enemyScenes.Length == 0)
		{
			GD.PrintErr("EnemySpawner: No enemy scenes assigned in the inspector!");
			return;
		}

		if (enemyUnlockLevels == null || enemyUnlockLevels.Length != enemyScenes.Length)
		{
			GD.PrintErr("EnemySpawner: enemyUnlockLevels length doesn't match enemyScenes. Defaulting all to unlock at wave 0.");
			enemyUnlockLevels = new int[enemyScenes.Length];
			Array.Fill(enemyUnlockLevels, 0);
		}

		RefreshAvailableScenes();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("debug_StartNewWave") && enemiesInScene == 0)
			StartWave();
	}

	private void RefreshAvailableScenes()
	{
		availableScenes.Clear();
		for (int i = 0; i < enemyScenes.Length; i++)
		{
			if (enemyScenes[i] == null)
			{
				GD.PrintErr($"EnemySpawner: enemyScenes[{i}] is null, skipping.");
				continue;
			}
			if (waveLevel >= enemyUnlockLevels[i])
				availableScenes.Add(enemyScenes[i]);
		}

		if (availableScenes.Count == 0)
			GD.PrintErr($"EnemySpawner: No enemies available at wave {waveLevel}. Check unlock levels.");
		else
		{
			string poolNames = string.Join(", ", availableScenes.ConvertAll(s => s.ResourcePath.GetFile()));
			GD.Print($"Wave {waveLevel} | Enemy Pool: [{poolNames}]");
		}
	}

	public void StartWave()
	{
		if (enemiesInScene != 0) return;

		waveLevel++;
		RefreshAvailableScenes();

		enemiesToSpawn = 0;
		EmitSignal(SignalName.WaveStarted, waveLevel, maxEnemies);
		SpawnEnemyWave();
	}

	private async void SpawnEnemyWave()
	{
		reservedSpawnPositions.Clear();
		while (enemiesToSpawn < maxEnemies)
		{
			SpawnEnemy();
			await ToSignal(GetTree().CreateTimer(spawnDelay), SceneTreeTimer.SignalName.Timeout);
		}
	}

	private void SpawnEnemy()
	{
		if (availableScenes.Count == 0)
		{
			GD.PrintErr("EnemySpawner: availableScenes is empty, cannot spawn.");
			enemiesToSpawn++;
			return;
		}

		int randomIndex = rng.RandiRange(0, availableScenes.Count - 1);
		PackedScene chosenScene = availableScenes[randomIndex];

		var enemy = chosenScene.Instantiate<Enemy>();
		GetTree().CurrentScene.AddChild(enemy);

		enemy.GlobalPosition = GetSafeSpawnPosition();
		enemy.AddToGroup("Enemy");
		enemy.SetName($"Enemy_W{waveLevel}_#{enemyLevel}");
		enemy.SetEnemyLevel(enemyLevel);

		enemiesToSpawn++;
		enemyLevel++;
		enemiesInScene++;
		EmitSignal(SignalName.EnemyCountChanged, enemiesInScene);
		enemy.TreeExited += OnEnemyRemoved;
	}

	private void OnEnemyRemoved()
	{
		enemiesInScene--;
		EmitSignal(SignalName.EnemyCountChanged, enemiesInScene);
		if (enemiesInScene == 0)
			EmitSignal(SignalName.WaveFinished);
	}

	private Vector3 GetSafeSpawnPosition()
	{
		float spawnRadius = Mathf.Sqrt(maxEnemies) * minSpawnSpacing * 0.75f;
		var spaceState = GetWorld3D().DirectSpaceState;

		for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
		{
			Vector3 candidate = GlobalPosition + new Vector3(
				rng.RandfRange(-spawnRadius, spawnRadius),
				0f,
				rng.RandfRange(-spawnRadius, spawnRadius)
			);

			bool tooCloseToEnemy = false;
			foreach (Vector3 reserved in reservedSpawnPositions)
			{
				if (candidate.DistanceTo(reserved) < minSpawnSpacing)
				{
					tooCloseToEnemy = true;
					break;
				}
			}
			if (tooCloseToEnemy) continue;

			var query = new PhysicsShapeQueryParameters3D();
			query.Shape = new SphereShape3D { Radius = enemyRadius };
			query.Transform = new Transform3D(Basis.Identity, candidate);
			query.CollisionMask = 1;

			if (spaceState.IntersectShape(query).Count == 0)
			{
				reservedSpawnPositions.Add(candidate);
				return candidate;
			}
		}

		GD.PrintErr("EnemySpawner: Could not find a clear spawn position, using grid fallback.");
		int index = reservedSpawnPositions.Count;
		int cols = Mathf.CeilToInt(Mathf.Sqrt(maxEnemies));
		Vector3 gridPos = GlobalPosition + new Vector3(
			(index % cols) * minSpawnSpacing, 0f,
			(index / cols) * minSpawnSpacing
		);
		reservedSpawnPositions.Add(gridPos);
		return gridPos;
	}
}
