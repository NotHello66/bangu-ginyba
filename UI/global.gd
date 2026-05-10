extends Node

var using_wasd: bool = true
var pause_reasons: Array = []
var current_wave: int = 0
var can_pull: bool = false
signal stats_changed

enum FriendlyState{
	Idle = 0,
	Chasing = 1,
	Following = 2,
	Stationed = 3,
	Attacking = 4,
	Stunned = 5,
	Dead = 6
}
signal friendlyStateChanged(newState: int)
signal attackOrderIssue
var currentState:int = FriendlyState.Following:
	set(value):
		currentState = value
		friendlyStateChanged.emit(currentState)
		print("Friendly state changed: %d" % currentState)

signal gold_changed(amount: int)

var gold: int = 100:
	set(value):
		if value > gold:
			play_gold()
		gold = value
		gold_changed.emit(gold)

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("unitsFollow"):
		currentState = FriendlyState.Following
	if event.is_action_pressed("unitsStationed"):
		currentState = FriendlyState.Stationed
	if event.is_action_pressed("unitsAttack"):
		attackOrderIssue.emit()
func add_pause(reason: String) -> void:
	if reason not in pause_reasons:
		pause_reasons.append(reason)
	get_tree().paused = true

func remove_pause(reason: String) -> void:
	pause_reasons.erase(reason)
	if pause_reasons.is_empty():
		get_tree().paused = false

func apply_buff(buff: String, value: int) -> void:
	var health_comp = Engine.get_main_loop().root.get_node_or_null("Node3D/Player/HealthComponent")
	var melee_comp = Engine.get_main_loop().root.get_node_or_null("Node3D/Player/MeleeComponent")
	var ranged_comp = Engine.get_main_loop().root.get_node_or_null("Node3D/Player/RangedComponent")
	
	print("apply_buff called: ", buff, " value: ", value)
	print("health_comp: ", health_comp)
	print("melee_comp: ", melee_comp)
	print("ranged_comp: ", ranged_comp)
	match buff:
		"health":
			if health_comp:
				health_comp.call("IncreaseMaxHP", float(value))
		"damage":
			if melee_comp:
				melee_comp.set("BonusDamage", melee_comp.get("BonusDamage") + value)
				print("melee damage now: ", melee_comp.get("damage"))
			if ranged_comp:
				ranged_comp.set("damage", ranged_comp.get("damage") + value)
				print("ranged damage now: ", ranged_comp.get("damage"))
		"all":
			apply_buff("health", value)
			apply_buff("damage", value)
	stats_changed.emit()
	print("stats_changed emitted")

var audio_button: AudioStreamPlayer
var audio_spin: AudioStreamPlayer
var audio_win: AudioStreamPlayer
var audio_wave_start: AudioStreamPlayer
var audio_wave_clear: AudioStreamPlayer
var audio_heartbeat: AudioStreamPlayer
var audio_gold: AudioStreamPlayer

func _ready() -> void:
	setup_audio()

func setup_audio() -> void:
	audio_button = _make_player("res://UI/Sounds/button_click.wav")
	audio_spin = _make_player("res://UI/Sounds/case_opening2.wav")
	audio_win = _make_player("res://UI/Sounds/win.wav")
	audio_wave_start = _make_player("res://UI/Sounds/wave_start.wav")
	audio_wave_clear = _make_player("res://UI/Sounds/win.wav")
	audio_heartbeat = _make_player("res://UI/Sounds/heartbeat.wav")
	audio_gold = _make_player("res://UI/Sounds/coin_pickup.wav")

func _make_player(path: String) -> AudioStreamPlayer:
	var player = AudioStreamPlayer.new()
	add_child(player)
	if ResourceLoader.exists(path):
		player.stream = load(path)
	else:
		print("Audio missing: ", path)
	return player

func play_button() -> void:
	if audio_button and audio_button.stream:
		audio_button.play()

func play_spin() -> void:
	if audio_spin and audio_spin.stream:
		audio_spin.play()

func stop_spin() -> void:
	if audio_spin:
		audio_spin.stop()

func play_win() -> void:
	stop_spin()
	if audio_win and audio_win.stream:
		audio_win.play()

func play_wave_start() -> void:
	if audio_wave_start and audio_wave_start.stream:
		audio_wave_start.play()

func play_wave_clear() -> void:
	if audio_wave_clear and audio_wave_clear.stream:
		audio_wave_clear.play()

func play_gold() -> void:
	if audio_gold and audio_gold.stream:
		audio_gold.play()

func start_heartbeat() -> void:
	if audio_heartbeat and audio_heartbeat.stream:
		audio_heartbeat.play()

func stop_heartbeat() -> void:
	if audio_heartbeat:
		audio_heartbeat.stop()
