extends GutTest
var Global = null
 
func before_each():
	# Instantiate a fresh Global node for each test
	Global = preload("res://UI/global.gd").new()
	add_child_autofree(Global)
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
 
