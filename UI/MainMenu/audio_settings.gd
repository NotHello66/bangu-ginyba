extends Panel
@onready var audio_settings: Panel = $"."
@onready var settings: Panel = $"../settings"


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass




func _on_back_button_pressed() -> void:
	audio_settings.visible = false
	settings.visible = true
