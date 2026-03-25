extends Node

const DB = preload("res://RewardSystem/item_database.gd")
var PlayerEcon
const INVESTMENT_TIERS = [
	{ "cost": 0, "multiplier": 1.0, "label": "No Boost" },
	{ "cost": 100, "multiplier": 2.0, "label": "Small Boost" },
	{ "cost": 200, "multiplier": 3.0, "label": "Medium Boost" },
	{ "cost": 300, "multiplier": 5.0, "label": "Large Boost" },
	{ "cost": 400, "multiplier": 7.5, "label": "Huge Boost" },
	{ "cost": 500, "multiplier": 10.0, "label": "MAX Boost" },
]

var player_gold
func _ready() -> void:
	var player = get_tree().get_first_node_in_group("Player")
	if player:
		PlayerEcon = player.get_node_or_null("EconomyComponent")
		if PlayerEcon:
			player_gold = PlayerEcon.currentGold
	if player_gold == null:
		player_gold = Global.gold
func _process(delta: float) -> void:
	player_gold = PlayerEcon.currentGold
	#print("PlayerGold %d" % player_gold)
func _physics_process(delta: float) -> void:
	Global.gold = player_gold

func get_investment_tier(investment: int) -> Dictionary:
	var best_tier = INVESTMENT_TIERS[0]
	for tier in INVESTMENT_TIERS:
		if investment >= tier.cost:
			best_tier = tier
	return best_tier

func calculate_chances(investment: int) -> Dictionary:
	var tier = get_investment_tier(investment)
	var multiplier = tier.multiplier
	
	var chances = {}
	chances[DB.Rarity.GREY] = DB.BASE_CHANCES[DB.Rarity.GREY] / multiplier
	chances[DB.Rarity.BLUE] = DB.BASE_CHANCES[DB.Rarity.BLUE] / multiplier
	chances[DB.Rarity.PURPLE] = DB.BASE_CHANCES[DB.Rarity.PURPLE] * multiplier
	chances[DB.Rarity.PINK] = DB.BASE_CHANCES[DB.Rarity.PINK] * multiplier
	chances[DB.Rarity.RED] = DB.BASE_CHANCES[DB.Rarity.RED] * multiplier
	chances[DB.Rarity.GOLD] = DB.BASE_CHANCES[DB.Rarity.GOLD] * multiplier
	
	var total = 0.0
	for key in chances:
		total += chances[key]
	for key in chances:
		chances[key] = (chances[key] / total) * 100.0
	
	return chances

func roll_rarity(investment: int) -> DB.Rarity:
	var chances = calculate_chances(investment)
	var roll = randf() * 100.0
	var cumulative = 0.0
	var sorted_order = [DB.Rarity.GREY, DB.Rarity.BLUE, DB.Rarity.PURPLE, DB.Rarity.PINK, DB.Rarity.RED, DB.Rarity.GOLD]
	for rarity in sorted_order:
		cumulative += chances[rarity]
		if roll <= cumulative:
			return rarity
	
	return DB.Rarity.GREY

func pull_item(investment: int) -> Dictionary:
	Global.gold -= investment
	var rarity = roll_rarity(investment)
	
	var pool = DB.new().get_items_by_rarity(rarity)
	if pool.is_empty():
		return {}
	
	var item = pool[randi() % pool.size()]
	return item

func can_afford(investment: int) -> bool:
	return Global.gold >= investment
