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
var currentState:int = FriendlyState.Following:
	set(value):
		currentState = value
		friendlyStateChanged.emit(currentState)

signal gold_changed(amount: int)

var gold: int = 100:
	set(value):
		gold = value
		gold_changed.emit(gold)

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("unitsFollow"):
		currentState = FriendlyState.Following
	if event.is_action_pressed("unitsStationed"):
		currentState = FriendlyState.Stationed

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
				melee_comp.set("damage", melee_comp.get("damage") + value)
				print("melee damage now: ", melee_comp.get("damage"))
			if ranged_comp:
				ranged_comp.set("damage", ranged_comp.get("damage") + value)
				print("ranged damage now: ", ranged_comp.get("damage"))
		"all":
			apply_buff("health", value)
			apply_buff("damage", value)
	stats_changed.emit()
	print("stats_changed emitted")
