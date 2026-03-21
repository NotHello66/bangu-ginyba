extends CanvasLayer

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	visible = false

func show_menu() -> void:
	visible = true
	get_tree().paused = true

func _on_close_button_pressed() -> void:
	visible = false
	get_tree().paused = false
