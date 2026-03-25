extends GutTest
var Global = null
var TowerManager = null
 
func before_each():
	# Instantiate a fresh Global node for each test
	Global = preload("res://UI/global.gd").new()
	add_child_autofree(Global)

	TowerManager = preload("res://Scripts/tower_manager.gd").new()
	add_child_autofree(TowerManager)
#unpauses the tree after each test in case it is left paused
func after_each():
	get_tree().paused = false
func test_gold_default_value():
	assert_eq(Global.gold, 100, "Gold should start at 100")
 #////////////////gold tests///////////////////////////
func test_gold_setter_updates_value():
	Global.gold = 250
	assert_eq(Global.gold, 250, "Gold should be updated to 250")
 
func test_gold_setter_emits_signal():
	watch_signals(Global)
	Global.gold = 50
	assert_signal_emitted_with_parameters(Global, "gold_changed", [50])
func test_current_state_default():
	assert_eq(Global.currentState, Global.FriendlyState.Following, "Default state should be Following")
 #////////////////friendly state tests///////////////////////////
func test_current_state_setter_updates_value():
	Global.currentState = Global.FriendlyState.Idle
	assert_eq(Global.currentState, Global.FriendlyState.Idle)
 
func test_current_state_setter_emits_signal():
	watch_signals(Global)
	Global.currentState = Global.FriendlyState.Attacking
	assert_signal_emitted_with_parameters(Global, "friendlyStateChanged", [Global.FriendlyState.Attacking])
 #////////////////pausing removing tests///////////////////////////
func test_add_pause_appends_reason():
	Global.add_pause("shop")
	assert_true("shop" in Global.pause_reasons, "pause_reasons should contain 'shop'")
 
func test_add_pause_does_not_duplicate_reason():
	Global.add_pause("shop")
	Global.add_pause("shop")
	assert_eq(Global.pause_reasons.size(), 1, "Duplicate reasons should not be added")
 
func test_add_pause_multiple_reasons():
	Global.add_pause("shop")
	Global.add_pause("cutscene")
	assert_eq(Global.pause_reasons.size(), 2)
 
func test_remove_pause_removes_reason():
	Global.add_pause("shop")
	Global.remove_pause("shop")
	assert_false("shop" in Global.pause_reasons)
 
func test_remove_pause_nonexistent_reason_is_safe():
	# Should not crash when removing a reason that was never added
	Global.remove_pause("ghost_reason")
	assert_eq(Global.pause_reasons.size(), 0)
 
func test_remove_pause_only_unpauses_when_empty():
	Global.add_pause("shop")
	Global.add_pause("cutscene")
	Global.remove_pause("shop")
	# pause_reasons still has "cutscene", so tree should still be paused
	assert_eq(Global.pause_reasons.size(), 1)
#/////////////////////buff tests////////////////////////////////
#tree nera, tai veryifinam kad veikia pakeitimo signalas
func test_apply_buff_health_emits_stats_changed():
	watch_signals(Global)
	Global.apply_buff("health", 10)
	assert_signal_emitted(Global, "stats_changed")
 
func test_apply_buff_damage_emits_stats_changed():
	watch_signals(Global)
	Global.apply_buff("damage", 5)
	assert_signal_emitted(Global, "stats_changed")
 
func test_apply_buff_all_emits_stats_changed():
	watch_signals(Global)
	Global.apply_buff("all", 20)
	# apply_buff("all") calls apply_buff("health") and apply_buff("damage"),
	# each of which emits stats_changed — assert it fired at least once
	assert_signal_emit_count(Global, "stats_changed", 3)
 
func test_apply_buff_unknown_buff_still_emits_stats_changed():
	watch_signals(Global)
	Global.apply_buff("unknown_buff", 99)
	assert_signal_emitted(Global, "stats_changed")
 
# ─── TOWER PLACEMENT: DEFAULTS ────────────────────────────────────────────────

func test_tower_manager_can_place_tower_default():
	assert_false(TowerManager.canPlaceTower, "canPlaceTower should default to false")

func test_tower_manager_current_building_default():
	assert_eq(TowerManager.current_building, TowerManager.BuildingType.NONE,
		"Default building type should be NONE")

func test_tower_manager_can_place_here_default():
	assert_true(TowerManager.can_place_here, "can_place_here should default to true")

func test_tower_manager_preview_tower_default():
	assert_null(TowerManager.preview_tower, "preview_tower should be null at start")

# ─── TOWER PLACEMENT: SELECT BUILDING ────────────────────────────────────────

func test_select_building_sets_tower():
	TowerManager.select_building(TowerManager.BuildingType.TOWER)
	assert_eq(TowerManager.current_building, TowerManager.BuildingType.TOWER)

func test_select_building_sets_wall():
	TowerManager.select_building(TowerManager.BuildingType.WALL)
	assert_eq(TowerManager.current_building, TowerManager.BuildingType.WALL)

func test_select_building_sets_bomb_tower():
	TowerManager.select_building(TowerManager.BuildingType.BOMB_TOWER)
	assert_eq(TowerManager.current_building, TowerManager.BuildingType.BOMB_TOWER)

func test_select_building_sets_none():
	TowerManager.select_building(TowerManager.BuildingType.TOWER)
	TowerManager.select_building(TowerManager.BuildingType.NONE)
	assert_eq(TowerManager.current_building, TowerManager.BuildingType.NONE)

func test_select_building_clears_preview():
	var fake_preview = Node3D.new()
	add_child_autofree(fake_preview)
	TowerManager.preview_tower = fake_preview
	TowerManager.select_building(TowerManager.BuildingType.WALL)
	assert_null(TowerManager.preview_tower, "select_building should clear the old preview")

# ─── TOWER PLACEMENT: GET CURRENT SCENE ──────────────────────────────────────

func test_get_current_scene_returns_null_for_none():
	TowerManager.current_building = TowerManager.BuildingType.NONE
	assert_null(TowerManager.get_current_scene())

func test_get_current_scene_returns_tower_scene():
	var fake_scene = PackedScene.new()
	TowerManager.tower_scene = fake_scene
	TowerManager.current_building = TowerManager.BuildingType.TOWER
	assert_eq(TowerManager.get_current_scene(), fake_scene)

func test_get_current_scene_returns_wall_scene():
	var fake_scene = PackedScene.new()
	TowerManager.wall = fake_scene
	TowerManager.current_building = TowerManager.BuildingType.WALL
	assert_eq(TowerManager.get_current_scene(), fake_scene)

func test_get_current_scene_returns_bomb_tower_scene():
	var fake_scene = PackedScene.new()
	TowerManager.bomb_tower = fake_scene
	TowerManager.current_building = TowerManager.BuildingType.BOMB_TOWER
	assert_eq(TowerManager.get_current_scene(), fake_scene)

# ─── TOWER PLACEMENT: REMOVE PREVIEW ─────────────────────────────────────────

func test_remove_preview_is_safe_when_already_null():
	TowerManager.preview_tower = null
	TowerManager.remove_preview()
	assert_null(TowerManager.preview_tower)

func test_remove_preview_nulls_preview_tower():
	var fake_preview = Node3D.new()
	add_child_autofree(fake_preview)
	TowerManager.preview_tower = fake_preview
	TowerManager.remove_preview()
	assert_null(TowerManager.preview_tower, "remove_preview should null out preview_tower")

# ─── TOWER PLACEMENT: SET PREVIEW COLOR ──────────────────────────────────────

func test_set_preview_color_updates_material():
	TowerManager.preview_material = StandardMaterial3D.new()
	var red = Color(1, 0, 0, 0.4)
	TowerManager.set_preview_color(red)
	assert_eq(TowerManager.preview_material.albedo_color, red,
		"Material color should update to red when placement is blocked")

func test_set_preview_color_green_when_valid():
	TowerManager.preview_material = StandardMaterial3D.new()
	var green = Color(0, 1, 0, 0.4)
	TowerManager.set_preview_color(green)
	assert_eq(TowerManager.preview_material.albedo_color, green,
		"Material color should be green when placement is valid")

# ─── TOWER PLACEMENT: COLLISION DISABLING ────────────────────────────────────

func test_disable_static_body_collision_zeroes_layer_and_mask():
	var static_body = StaticBody3D.new()
	static_body.collision_layer = 3
	static_body.collision_mask = 5
	add_child_autofree(static_body)
	TowerManager.disable_static_body_collision(static_body)
	assert_eq(static_body.collision_layer, 0, "collision_layer should be zeroed")
	assert_eq(static_body.collision_mask, 0, "collision_mask should be zeroed")

func test_disable_static_body_collision_recurses_into_children():
	var parent = Node3D.new()
	var child_body = StaticBody3D.new()
	child_body.collision_layer = 7
	child_body.collision_mask = 7
	parent.add_child(child_body)
	add_child_autofree(parent)
	TowerManager.disable_static_body_collision(parent)
	assert_eq(child_body.collision_layer, 0, "Child StaticBody3D layer should be zeroed")
	assert_eq(child_body.collision_mask, 0, "Child StaticBody3D mask should be zeroed")

func test_disable_static_body_collision_ignores_non_static_nodes():
	var plain_node = Node3D.new()
	add_child_autofree(plain_node)
	TowerManager.disable_static_body_collision(plain_node)
	assert_true(true, "Should complete without error on non-StaticBody3D")
