extends Control

@onready var main_buttons: VBoxContainer = $mainButtons
@onready var settings: Panel = $"settings"
@onready var audio_settings: Panel = $"audioSettings"
@onready var controls_settings: Panel = $"controlsSettings"
@onready var graphics_settings: Panel = $"graphicsSettings"

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

func _on_settings_button_pressed() -> void:
	main_buttons.visible = false
	settings.visible = true
	audio_settings.visible = false
	controls_settings.visible = false
	graphics_settings.visible = false


func _on_main_menu_button_pressed() -> void:
	get_tree().paused = false
	get_tree().change_scene_to_file("res://UI/MainMenu/in_game_ui.tscn")

func _on_exit_button_pressed() -> void:
	get_tree().quit()
