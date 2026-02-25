extends Node3D

@export var tower_scene: PackedScene
@export var canPlaceTower: bool = false
@export var navigation_region_3D: NavigationRegion3D

@onready var camera: Camera3D = get_viewport().get_camera_3d()

var preview_tower: Node3D = null
var preview_material: StandardMaterial3D = null
var can_place_here: bool = true

func _process(delta):
	if canPlaceTower:
		update_preview_position()
	else:
		remove_preview()

# Currently just checks for the placement_toggle
func _input(event):
	if event.is_action_pressed("toggle_placement"):
		canPlaceTower = !canPlaceTower
		print("Placement mode:", canPlaceTower)

# Handle click input
func _unhandled_input(event):
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT and event.pressed and canPlaceTower:
			if preview_tower and preview_tower.visible and can_place_here:
				spawn_tower(preview_tower.global_position)
				remove_preview()
				navigation_region_3D.bake_navigation_mesh(true)

func create_preview():
	if preview_tower != null:
		return
		
	preview_tower = tower_scene.instantiate()

	var tower_component = preview_tower.get_node_or_null("TowerComponent")
	if tower_component:
		tower_component.isPreview = true

	get_tree().current_scene.add_child(preview_tower)
	can_place_here = true

	var area = preview_tower.get_node_or_null("Area3D")
	if area:
		area.body_entered.connect(_on_preview_body_entered)
		area.body_exited.connect(_on_preview_body_exited)
	
	#  Disable collision completely
	disable_static_body_collision(preview_tower)
	make_preview_material(preview_tower)

# Disables the display towers collider so that raycasts can work
func disable_static_body_collision(node: Node):
	if node is StaticBody3D:
		node.collision_layer = 0
		node.collision_mask = 0
	
	for child in node.get_children():
		disable_static_body_collision(child)

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

# When an obsticle enters the body
func _on_preview_body_entered(body):
	if !body.is_in_group("Ground"):
		update_overlap_state()

# When an obsticle exits the body
func _on_preview_body_exited(body):
	update_overlap_state()

# Updates the logic depending on if there are or aren't any overlaps inside the display tower
func update_overlap_state():
	if preview_tower == null:
		return
	
	var area = preview_tower.get_node_or_null("Area3D")
	if area == null:
		return
	
	var overlapping = area.get_overlapping_bodies()
	
	for body in overlapping:
		if !body.is_in_group("Ground"):
			can_place_here = false
			set_preview_color(Color(1, 0, 0, 0.4))
			return
	
	# If we reach here, then there are no blocking overlaps
	can_place_here = true
	set_preview_color(Color(0, 1, 0, 0.4))

func set_preview_color(color: Color):
	preview_material.albedo_color = color

# Literally just spawns the tower
func spawn_tower(position: Vector3):
	var tower = tower_scene.instantiate()
	navigation_region_3D.add_child(tower)
	tower.global_position = position
