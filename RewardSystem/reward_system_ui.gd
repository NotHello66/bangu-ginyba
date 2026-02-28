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

var manager: GamblingManager

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	manager = GamblingManager.new()
	add_child(manager)
	result_panel.visible = false
	update_ui()

func update_ui() -> void:
	var investment = int(invest_slider.value)
	gold_label.text = "Gold: %d" % manager.player_gold
	invest_value_label.text = "%d gold" % investment
	var tier = manager.get_investment_tier(investment)
	boost_label.text = "Boost: %s (x%.1f)" % [tier.label, tier.multiplier]
	update_chances(investment)
	pull_button.disabled = !manager.can_afford(investment)

func update_chances(investment: int) -> void:
	for child in chance_container.get_children():
		child.queue_free()
	var chances = manager.calculate_chances(investment)
	var order = [DB.Rarity.GREY, DB.Rarity.BLUE, DB.Rarity.PURPLE, DB.Rarity.PINK, DB.Rarity.RED, DB.Rarity.GOLD]
	for rarity in order:
		var label = Label.new()
		label.text = "%s: %.2f%%" % [DB.RARITY_NAMES[rarity], chances[rarity]]
		label.add_theme_color_override("font_color", DB.RARITY_COLORS[rarity])
		label.add_theme_font_size_override("font_size", 10)
		label.add_theme_font_override("font", preload("res://UI/Design/Metamorphous-Regular.ttf"))
		chance_container.add_child(label)

func _on_invest_slider_value_changed(_value: float) -> void:
	result_panel.visible = false
	update_ui()

func _on_pull_button_pressed() -> void:
	var investment = int(invest_slider.value)
	if !manager.can_afford(investment):
		return
	var item = manager.pull_item(investment)
	if item.is_empty():
		return
	show_result(item)
	update_ui()

func show_result(item: Dictionary) -> void:
	var color = DB.RARITY_COLORS[item.rarity]
	result_panel.visible = true
	result_rarity_label.text = DB.RARITY_NAMES[item.rarity]
	result_rarity_label.add_theme_color_override("font_color", color)
	result_item_label.text = item.name
	result_item_label.add_theme_color_override("font_color", color)
	result_buff_label.text = item.desc

func _on_close_button_pressed() -> void:
	Global.remove_pause("reward")
	visible = false
	get_tree().paused = false
