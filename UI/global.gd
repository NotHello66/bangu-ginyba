extends Node

var using_wasd: bool = true
var pause_reasons: Array = []

signal gold_changed(amount: int)

var gold: int = 1000:
	set(value):
		gold = value
		gold_changed.emit(gold)

func add_pause(reason: String) -> void:
	if reason not in pause_reasons:
		pause_reasons.append(reason)
	get_tree().paused = true

func remove_pause(reason: String) -> void:
	pause_reasons.erase(reason)
	if pause_reasons.is_empty():
		get_tree().paused = false
