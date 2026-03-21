extends Control

const GamblingManager = preload("res://RewardSystem/reward_system_manager.gd")
const DB = preload("res://RewardSystem/item_database.gd")
@onready var gold_label: Label = $Panel/goldLabel
@onready var invest_slider: HSlider = $Panel/investmentContainer/investSlider
@onready var invest_value_label: Label = $Panel/investmentContainer/investValueLabel
@onready var boost_label: Label = $Panel/boostLabel
@onready var pull_button: Button = $Panel/pullButton
@onready var result_panel: Panel = $Panel/resultPanel
@onready var result_rarity_label: Label = $Panel/resultPanel/resultRarityLabel
@onready var result_item_label: Label = $Panel/resultPanel/resultItemLabel
@onready var result_buff_label: Label = $Panel/resultPanel/resultBuffLabel
@onready var chance_container: VBoxContainer = $Panel/chanceContainer
@onready var spin_container: Control = $Panel/spinContainer
@onready var item_strip: HBoxContainer = $Panel/spinContainer/itemStrip
var border_overlay: Panel

const MY_FONT = preload("res://UI/Design/MedievalSharp-Regular.ttf")
const CARD_WIDTH = 90
const CARD_MARGIN = 6
const WIN_POSITION = 24
const STRIP_COUNT = 36

var manager: GamblingManager
signal gold_changed(newGold: float)
var is_spinning: bool = false
func _ready() -> void:
	manager = GamblingManager.new()
	add_child(manager)
	result_panel.visible = false
	spin_container.clip_contents = true
	border_overlay = Panel.new()
	border_overlay.size = spin_container.size
	border_overlay.position = spin_container.position
	border_overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
	
	var border_style = StyleBoxFlat.new()
	border_style.bg_color = Color.TRANSPARENT
	border_style.border_color = Color(0.6, 0.4, 0.1)
	border_style.set_border_width_all(2)
	border_style.set_corner_radius_all(6)
	border_overlay.add_theme_stylebox_override("panel", border_style)
	
	spin_container.get_parent().add_child(border_overlay)
	spin_container.get_parent().move_child(border_overlay, -1)
	border_overlay.visible = true
	
	var indicator = Panel.new()
	indicator.mouse_filter = Control.MOUSE_FILTER_IGNORE
	indicator.size = Vector2(2, spin_container.size.y)
	indicator.position = Vector2(spin_container.size.x / 2.0 - 2, 0)

	var indicator_style = StyleBoxFlat.new()
	indicator_style.bg_color = Color.GOLD
	indicator_style.set_corner_radius_all(2)
	indicator.add_theme_stylebox_override("panel", indicator_style)

	var glow_indicator = Panel.new()
	glow_indicator.mouse_filter = Control.MOUSE_FILTER_IGNORE
	glow_indicator.size = Vector2(14, spin_container.size.y)
	glow_indicator.position = Vector2(spin_container.size.x / 2.0 - 7, 0)

	var glow_style = StyleBoxFlat.new()
	glow_style.bg_color = Color(1.0, 0.8, 0.1, 0.15)
	glow_indicator.add_theme_stylebox_override("panel", glow_style)

	spin_container.add_child(glow_indicator)
	spin_container.add_child(indicator)
	spin_container.move_child(indicator, -1)
	spin_container.move_child(glow_indicator, -1)
	
	invest_slider.min_value = 0
	invest_slider.max_value = 500
	invest_slider.step = 50
	invest_slider.value = 0
	update_ui()

func update_ui() -> void:
	var investment = int(invest_slider.value)
	gold_label.text = "Gold: %d" % Global.gold
	invest_value_label.text = "%d gold" % investment
	var tier = manager.get_investment_tier(investment)
	boost_label.text = "Boost: %s (x%.1f)" % [tier.label, tier.multiplier]
	update_chances(investment)
	pull_button.disabled = !manager.can_afford(investment) or is_spinning

func update_chances(investment: int) -> void:
	for child in chance_container.get_children():
		child.queue_free()
	var chances = manager.calculate_chances(investment)
	var order = [DB.Rarity.GREY, DB.Rarity.BLUE, DB.Rarity.PURPLE,
				 DB.Rarity.PINK, DB.Rarity.RED, DB.Rarity.GOLD]
	for rarity in order:
		var label = Label.new()
		label.text = "%s: %.2f%%" % [DB.RARITY_NAMES[rarity], chances[rarity]]
		label.add_theme_color_override("font_color", DB.RARITY_COLORS[rarity])
		label.add_theme_font_size_override("font_size", 13)
		label.add_theme_font_override("font", MY_FONT)
		chance_container.add_child(label)

func make_card(item: Dictionary) -> PanelContainer:
	var card = PanelContainer.new()
	card.custom_minimum_size = Vector2(CARD_WIDTH, 88)

	var style = StyleBoxFlat.new()
	var col: Color = DB.RARITY_COLORS[item.rarity]
	style.bg_color = col.darkened(0.65)
	style.border_color = col
	style.set_border_width_all(2)
	style.set_corner_radius_all(6)
	card.add_theme_stylebox_override("panel", style)

	var vbox = VBoxContainer.new()
	vbox.alignment = BoxContainer.ALIGNMENT_CENTER
	card.add_child(vbox)

	var name_lbl = Label.new()
	name_lbl.text = item.name
	name_lbl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	name_lbl.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	name_lbl.add_theme_color_override("font_color", DB.RARITY_COLORS[item.rarity])
	name_lbl.add_theme_font_size_override("font_size", 10)
	vbox.add_child(name_lbl)

	var buff_lbl = Label.new()
	buff_lbl.text = item.desc
	buff_lbl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	buff_lbl.add_theme_color_override("font_color", Color.WHITE)
	buff_lbl.add_theme_font_size_override("font_size", 9)
	vbox.add_child(buff_lbl)
	return card

func build_strip(winning_item: Dictionary) -> void:
	for child in item_strip.get_children():
		child.queue_free()
	item_strip.add_theme_constant_override("separation", CARD_MARGIN)
	for i in range(STRIP_COUNT):
		var item = winning_item if i == WIN_POSITION else DB.ITEMS[randi() % DB.ITEMS.size()]
		item_strip.add_child(make_card(item))

func start_spin(winning_item: Dictionary) -> void:
	border_overlay.visible = true
	is_spinning = true
	pull_button.disabled = true
	result_panel.visible = false
	build_strip(winning_item)

	await get_tree().process_frame
	await get_tree().process_frame

	var card_total = CARD_WIDTH + CARD_MARGIN
	var visible_w = spin_container.size.x
	var target_x = -(WIN_POSITION * card_total - visible_w / 2.0 + CARD_WIDTH / 2.0)

	item_strip.position.x = 0

	var tween = create_tween()
	tween.set_pause_mode(Tween.TWEEN_PAUSE_PROCESS)
	tween.set_ease(Tween.EASE_OUT)
	tween.set_trans(Tween.TRANS_CUBIC)
	tween.tween_property(item_strip, "position:x", target_x, 3.5)

	await tween.finished
	flash_winner(winning_item)

func flash_winner(item: Dictionary) -> void:
	var winner_card = item_strip.get_child(WIN_POSITION)
	if not winner_card:
		return

	var col = DB.RARITY_COLORS[item.rarity]
	
	var tween = create_tween()
	tween.set_pause_mode(Tween.TWEEN_PAUSE_PROCESS)
	tween.set_loops(3)
	tween.tween_property(winner_card, "scale", Vector2(1.12, 1.12), 0.1)
	tween.tween_property(winner_card, "scale", Vector2(1.0, 1.0), 0.1)
	await tween.finished

	var flash = ColorRect.new()
	flash.color = Color(col.r, col.g, col.b, 0.0)
	flash.set_anchors_preset(Control.PRESET_FULL_RECT)
	add_child(flash)
	var flash_tween = create_tween()
	flash_tween.set_pause_mode(Tween.TWEEN_PAUSE_PROCESS)
	flash_tween.tween_property(flash, "color:a", 0.25, 0.1)
	flash_tween.tween_property(flash, "color:a", 0.0, 0.4)
	await flash_tween.finished
	flash.queue_free()

	show_result(item)
	is_spinning = false
	update_ui()

func show_result(item: Dictionary) -> void:
	Global.apply_buff(item.buff, item.value)
	border_overlay.visible = false
	var color = DB.RARITY_COLORS[item.rarity]
	result_panel.visible = true

	result_panel.custom_minimum_size = Vector2(100, 100)
	result_panel.modulate.a = 0.0
	result_panel.position.y += 20
	var tween = create_tween()
	tween.set_pause_mode(Tween.TWEEN_PAUSE_PROCESS)
	tween.set_parallel(true)
	tween.tween_property(result_panel, "modulate:a", 1.0, 0.3)
	tween.tween_property(result_panel, "position:y", result_panel.position.y - 20, 0.3)

	result_rarity_label.text = "✦ %s ✦" % DB.RARITY_NAMES[item.rarity]
	result_rarity_label.add_theme_color_override("font_color", color)
	result_item_label.text = item.name
	result_item_label.add_theme_color_override("font_color", color)
	result_buff_label.text = item.desc

	var style = StyleBoxFlat.new()
	style.bg_color = color.darkened(0.75)
	style.border_color = color
	style.set_border_width_all(2)
	style.set_corner_radius_all(8)
	result_panel.add_theme_stylebox_override("panel", style)


func _on_invest_slider_value_changed(value: float) -> void:
	result_panel.visible = false
	update_ui()


func _on_close_button_pressed() -> void:
	if not is_spinning:
		visible = false
		get_tree().paused = false


func _on_pull_button_pressed() -> void:
	if is_spinning:
		return
	if not Global.can_pull:
		return
	var investment = int(invest_slider.value)
	if !manager.can_afford(investment):
		return
	var item = manager.pull_item(investment)
	if item.is_empty():
		return
	Global.can_pull = false
	emit_signal("gold_changed", investment)
	update_ui()
	start_spin(item)
