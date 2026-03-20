extends Node3D
@onready var game_over: CanvasLayer = $GameOver
@onready var enemy_spawner: Node3D = $EnemySpawner

func _ready() -> void:
	enemy_spawner.connect("WaveFinished", _on_wave_finished)
	
	var health_component = get_tree().root.get_node_or_null("Node3D/Player/HealthComponent")
	if health_component:
		health_component.connect("PlayerDied", _on_player_died)

func _on_wave_finished() -> void:
	Global.current_wave += 1
	print("Wave %d finished!" % Global.current_wave)
	# automatically
	# $reward/rewardSystemUI.visible = true
	# get_tree().paused = true

func _on_player_died() -> void:
	var wave = enemy_spawner.get("waveLevel")
	game_over.show_game_over(wave)
