extends Node3D
@onready var game_over: CanvasLayer = $GameOver
@onready var enemy_spawner: Node3D = $EnemySpawner
@onready var hud: CanvasLayer = $HUD
@onready var reward_button: Button = $rewardButton
@onready var wave_button: Button = $waveButton

func _ready() -> void:
	enemy_spawner.connect("WaveFinished", _on_wave_finished)
	enemy_spawner.connect("EnemyCountChanged", _on_enemy_count_changed)
	enemy_spawner.connect("WaveStarted", _on_wave_started)
	var health_component = get_tree().root.get_node_or_null("Node3D/Player/HealthComponent")
	if health_component:
		health_component.connect("PlayerDied", _on_player_died)
	reward_button.visible = false
	wave_button.visible = true

func _on_wave_started(wave: int, total: int) -> void:
	Global.current_wave = wave
	hud.update_wave(wave)
	hud.update_enemies(total)
	Global.can_pull = false
	reward_button.visible = false
	wave_button.visible = false

func _on_wave_finished() -> void:
	Global.can_pull = true
	print("Wave %d finished!" % Global.current_wave)
	reward_button.visible = true
	wave_button.visible = true

func _on_player_died() -> void:
	var wave = enemy_spawner.get("waveLevel")
	game_over.show_game_over(wave)

func _on_enemy_count_changed(remaining: int) -> void:
	hud.update_enemies(remaining)


func _on_wave_button_pressed() -> void:
	wave_button.visible = false
	enemy_spawner.call("StartWave")
