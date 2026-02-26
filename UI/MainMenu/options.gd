extends Panel
@onready var audio_settings: Panel = $"../audioSettings"
@onready var controls_settings: Panel = $"../controlsSettings"
@onready var graphics_settings: Panel = $"../graphicsSettings"
@onready var settings: Panel = $"."
@onready var main_buttons: VBoxContainer = $"../mainButtons"


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func _on_audio_button_pressed() -> void:
	audio_settings.visible = true
	settings.visible = false

func _on_controls_button_pressed() -> void:
	controls_settings.visible = true
	settings.visible = false


func _on_graphics_button_pressed() -> void:
	graphics_settings.visible = true
	settings.visible = false


func _on_back_button_pressed() -> void:
	settings.visible = false
	main_buttons.visible = true
