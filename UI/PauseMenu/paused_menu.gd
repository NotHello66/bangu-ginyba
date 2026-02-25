extends Control

@onready var grid: GridContainer = $mainButtons
@onready var options: Panel = $options

var _is_paused:bool = false:
	set = set_paused

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("pause"):
		_is_paused = !_is_paused

func set_paused(value:bool) -> void:
	_is_paused = value
	get_tree().paused = _is_paused
	visible = _is_paused

func _on_resume_button_pressed() -> void:
	_is_paused = false


func _on_options_button_pressed() -> void:
	grid.visible = false
	options.visible = true

func _on_main_menu_button_pressed() -> void:
	get_tree().paused = false
	get_tree().change_scene_to_file("res://UI/MainMenu/in_game_ui.tscn")

func _on_quit_button_pressed() -> void:
	get_tree().quit()


func _on_back_button_pressed() -> void:
	grid.visible = true
	options.visible = false
