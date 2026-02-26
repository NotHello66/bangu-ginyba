extends Panel

@onready var audio_settings: Panel = $"."
@onready var settings: Panel = $"../settings"

func _on_back_button_pressed() -> void:
	audio_settings.visible = false
	settings.visible = true
