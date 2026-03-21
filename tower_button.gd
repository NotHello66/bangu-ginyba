extends Button
@onready var tower_menu: CanvasLayer = $"../towerMenu"

func _ready() -> void:
	pass # Replace with function body.

func _process(delta: float) -> void:
	pass


func _on_pressed() -> void:
	tower_menu.show_menu()
