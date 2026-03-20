extends CanvasLayer

@onready var gold_label: Label = $Panel/statsPanel/goldLabel
@onready var wave_label: Label = $Panel/statsPanel/waveLabel
@onready var panel: Panel = $Panel

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	visible = false

func show_game_over(wave_reached: int) -> void:
	visible = true
	get_tree().paused = true
	wave_label.text = "Wave reached: %d" % wave_reached
	gold_label.text = "Gold earned: %d" % Global.gold

	panel.modulate.a = 0.0
	var tween = create_tween()
	tween.set_pause_mode(Tween.TWEEN_PAUSE_PROCESS)
	tween.tween_property(panel, "modulate:a", 1.0, 1.0)

func _on_restart_button_pressed() -> void:
	get_tree().paused = false
	Global.gold = 1000
	get_tree().change_scene_to_file("res://test.tscn")

func _on_main_menu_button_pressed() -> void:
	get_tree().paused = false
	Global.gold = 1000
	get_tree().change_scene_to_file("res://UI/MainMenu/in_game_ui.tscn")

func _on_exit_button_pressed() -> void:
	get_tree().paused = false
	get_tree().quit()
