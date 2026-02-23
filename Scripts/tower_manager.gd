extends Node3D

@export var tower_scene: PackedScene
@export var canPlaceTower: bool = false
@export var navigation_region_3D: NavigationRegion3D

@onready var camera: Camera3D = get_viewport().get_camera_3d()

var preview_tower: Node3D = null
var preview_material: StandardMaterial3D = null

func _process(delta):
	if canPlaceTower:
		update_preview_position()
	else:
		remove_preview()

# Handle click input
func _unhandled_input(event):
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT and event.pressed and canPlaceTower:
			if preview_tower and preview_tower.visible:
				spawn_tower(preview_tower.global_position)
				navigation_region_3D.bake_navigation_mesh(true)

func create_preview():
	if preview_tower != null:
		return
		
	preview_tower = tower_scene.instantiate()

	var tower_component = preview_tower.get_node_or_null("TowerComponent")
	if tower_component:
		tower_component.isPreview = true

	get_tree().current_scene.add_child(preview_tower)

	#  Disable collision completely
	disable_collision_recursive(preview_tower)
	make_preview_material(preview_tower)

# Disables the display towers collider so that raycasts can work
func disable_collision_recursive(node: Node):
	if node is CollisionObject3D:
		node.collision_layer = 0
		node.collision_mask = 0
	
	for child in node.get_children():
		disable_collision_recursive(child)

# Applies the preview material
func make_preview_material(node: Node):
	preview_material = StandardMaterial3D.new()
	preview_material.albedo_color = Color(0, 1, 0, 0.4) # Green transparent
	preview_material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	preview_material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	
	apply_material_recursive(node)

# Only important if the tower has mesh children
func apply_material_recursive(node: Node):
	if node is MeshInstance3D:
		node.material_override = preview_material
	
	for child in node.get_children():
		apply_material_recursive(child)

# Main preview tower logic
func update_preview_position():
	if camera == null:
		return
		
	if preview_tower == null:
		create_preview()
		
	var mouse_pos = get_viewport().get_mouse_position()
	var ray_origin = camera.project_ray_origin(mouse_pos)
	var ray_end = ray_origin + camera.project_ray_normal(mouse_pos) * 1000
	
	var space_state = get_world_3d().direct_space_state
	var query = PhysicsRayQueryParameters3D.create(ray_origin, ray_end)
	query.collide_with_areas = false
	query.collide_with_bodies = true
	
	var result = space_state.intersect_ray(query)
	
	if result and result.collider.is_in_group("Ground"):
		preview_tower.visible = true
		preview_tower.global_position = result.position
	else:
		preview_tower.visible = false

# Removes the display preview
func remove_preview():
	if preview_tower != null:
		preview_tower.queue_free()
		preview_tower = null

# Literally just spawns the tower
func spawn_tower(position: Vector3):
	var tower = tower_scene.instantiate()
	navigation_region_3D.add_child(tower)
	tower.global_position = position
