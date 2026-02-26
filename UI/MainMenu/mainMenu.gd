extends Control
@onready var main_buttons: VBoxContainer = $mainButtons
@onready var fade: CanvasLayer = $fade
@onready var audio_settings: Panel = $audioSettings
@onready var controls_settings: Panel = $controlsSettings
@onready var graphics_settings: Panel = $graphicsSettings
@onready var settings: Panel = $settings

func _ready() -> void:
	main_buttons.visible = true
	settings.visible = false
	audio_settings.visible = false
	controls_settings.visible = false
	graphics_settings.visible = false

func _on_start_pressed() -> void:
	var tween = fade.fade(1.0, 1.0)
	await tween.finished
	get_tree().change_scene_to_file("res://test.tscn")

func _on_options_pressed() -> void:
	main_buttons.visible = false
	settings.visible = true
	audio_settings.visible = false
	controls_settings.visible = false
	graphics_settings.visible = false


func _on_exit_pressed() -> void:
	get_tree().quit()


func _on_back_options_pressed() -> void:
	_ready()


func _on_audio_control_value_changed(value: float) -> void:
	pass


func _on_back_button_pressed() -> void:
	pass # Replace with function body.
