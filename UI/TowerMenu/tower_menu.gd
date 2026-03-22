extends CanvasLayer

@export var tower_manager: Node3D
@export var archer_towerselect_button: Button
@export var wall_towerselect_button: Button
@export var close_button: Button
@export var bomb_towerselect_button: Button

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	visible = false
	archer_towerselect_button.pressed.connect(_on_archer_button_pressed)
	wall_towerselect_button.pressed.connect(_on_wall_button_pressed)
	bomb_towerselect_button.pressed.connect(_on_bomb_Tower_button_pressed)

func show_menu() -> void:
	visible = true
	get_tree().paused = true

func _on_close_button_pressed() -> void:
	tower_manager.select_building(tower_manager.BuildingType.NONE)
	visible = false
	get_tree().paused = false

func _on_archer_button_pressed() -> void:
	tower_manager.select_building(tower_manager.BuildingType.TOWER)
	visible = false
	get_tree().paused = false

func _on_wall_button_pressed() -> void:
	tower_manager.select_building(tower_manager.BuildingType.WALL)
	visible = false
	get_tree().paused = false
	
func _on_bomb_Tower_button_pressed() -> void:
	tower_manager.select_building(tower_manager.BuildingType.BOMB_TOWER)
	visible = false
	get_tree().paused = false
