extends Node

enum Rarity { GREY, BLUE, PURPLE, PINK, RED, GOLD }

const BASE_CHANCES = { Rarity.GREY: 60, Rarity.BLUE: 25, Rarity.PURPLE: 10, 
	Rarity.PINK: 3.5, Rarity.RED: 1, Rarity.GOLD: 0.5 }

const RARITY_NAMES = { Rarity.GREY: "Common", Rarity.BLUE: "Decent", Rarity.PURPLE: "Good",
	Rarity.PINK: "Great", Rarity.RED: "Epic", Rarity.GOLD: "Legendary" }

const RARITY_COLORS = { Rarity.GREY:   Color("gray"), Rarity.BLUE:   Color("blue"),
	Rarity.PURPLE: Color("purple"), Rarity.PINK:   Color("hotpink"), 
	Rarity.RED:    Color("red"), Rarity.GOLD:   Color("gold") }

const ITEMS = [
	{ "name": "Sword I", "rarity": Rarity.GREY, "buff": "damage", "value": 2, "desc": "+2 Damage" },
	{ "name": "HP I", "rarity": Rarity.GREY, "buff": "health", "value": 10, "desc": "+10 Max HP" },

	{ "name": "Sword II", "rarity": Rarity.BLUE, "buff": "damage", "value": 8, "desc": "+8 Damage" },
	{ "name": "HP II", "rarity": Rarity.BLUE, "buff": "health", "value": 25, "desc": "+25 Max HP" },

	{ "name": "Sword III", "rarity": Rarity.PURPLE, "buff": "damage", "value": 18, "desc": "+18 Damage" },
	{ "name": "HP III", "rarity": Rarity.PURPLE, "buff": "health", "value": 50, "desc": "+50 Max HP" },

	{ "name": "Sword IV", "rarity": Rarity.PINK, "buff": "damage", "value": 30, "desc": "+30 Damage" },
	{ "name": "HP IV", "rarity": Rarity.PINK, "buff": "health", "value": 80, "desc": "+80 Max HP" },

	{ "name": "Sword V", "rarity": Rarity.RED, "buff": "damage", "value": 50, "desc": "+50 Damage" },
	{ "name": "HP V", "rarity": Rarity.RED, "buff": "health", "value": 120, "desc": "+120 Max HP" },

	{ "name": "Core", "rarity": Rarity.GOLD, "buff": "all", "value": 75, "desc": "+75 All Stats" },
	{ "name": "Sword X", "rarity": Rarity.GOLD, "buff": "damage", "value": 100, "desc": "+100 Damage" },
	{ "name": "HP X", "rarity": Rarity.GOLD, "buff": "health", "value": 250, "desc": "+250 Max HP" }
]

static func get_items_by_rarity(rarity: Rarity) -> Array:
	return ITEMS.filter(func(item): return item.rarity == rarity)
