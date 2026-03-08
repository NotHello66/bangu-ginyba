extends CanvasLayer

@onready var bar_fill: TextureRect = $healthControl/barFill
@onready var health_label: Label = $healthControl/healthLabel
@onready var gold_label: Label = $goldControl/goldLabel
@onready var damage_label: Label = $damageControl/damageLabel

const BAR_MAX_WIDTH = 130.0
var health_component: Node

func _ready() -> void:
	await get_tree().process_frame
	health_component = get_tree().root.get_node_or_null("Node3D/Player/HealthComponent")
	if health_component:
		health_component.connect("HealthChanged", _on_health_changed)
	Global.gold_changed.connect(_on_gold_changed)
	update_gold(Global.gold)
	update_damage(10)

func _on_health_changed(current: float, maximum: float) -> void:
	var ratio = current / maximum
	bar_fill.size.x = BAR_MAX_WIDTH * ratio
	health_label.text = "%d / %d" % [int(current), int(maximum)]

func _on_gold_changed(amount: int) -> void:  # this was missing
	update_gold(amount)

func update_gold(amount: int) -> void:
	gold_label.text = str(amount)

func update_damage(amount: int) -> void:
	damage_label.text = "%d DMG" % amount
