extends CanvasLayer

@onready var bar_fill: TextureRect = $healthControl/barFill
@onready var health_label: Label = $healthControl/healthLabel
@onready var gold_label: Label = $goldControl/goldLabel
@onready var damage_label: Label = $damageControl/damageLabel
@onready var wave_label: Label = $waveControl/bg/waveLabel
@onready var enemies_label: Label = $enemiesControl/bg/enemiesLabel
@onready var announcement_label: Label = $announcementLabel

const BAR_MAX_WIDTH = 130.0
var health_component: Node

func _ready() -> void:
	await get_tree().process_frame
	health_component = get_tree().root.get_node_or_null("Node3D/Player/HealthComponent")
	if health_component:
		health_component.connect("HealthChanged", _on_health_changed)
	Global.gold_changed.connect(_on_gold_changed)
	Global.stats_changed.connect(_on_stats_changed)
	update_gold(Global.gold)
	_on_stats_changed()

func _on_health_changed(current: float, maximum: float) -> void:
	var ratio = current / maximum
	bar_fill.size.x = BAR_MAX_WIDTH * ratio
	health_label.text = "%d / %d" % [int(current), int(maximum)]

func _on_gold_changed(amount: int) -> void:
	update_gold(amount)

func update_gold(amount: int) -> void:
	gold_label.text = str(amount)

func update_damage(amount: int) -> void:
	damage_label.text = "%d DMG" % amount

func update_wave(wave: int) -> void:
	wave_label.text = str(wave) + " Wawe"

func update_enemies(remaining: int) -> void:
	enemies_label.text = "%d enemies left" % remaining

func _on_stats_changed() -> void:
	print("HUD: stats_changed received")
	var ranged = get_tree().root.get_node_or_null("Node3D/Player/RangedComponent")
	var melee = get_tree().root.get_node_or_null("Node3D/Player/MeleeComponent")
	var dmg = 0.0
	if ranged:
		dmg = ranged.get("damage")
		print("HUD: ranged damage = ", dmg)
	elif melee:
		dmg = melee.get("damage")
		print("HUD: melee damage = ", dmg)
	update_damage(int(dmg))

func show_announcement(text: String) -> void:
	announcement_label.text = text
	announcement_label.modulate.a = 0.0
	announcement_label.visible = true

	var tween = create_tween()
	tween.set_pause_mode(Tween.TWEEN_PAUSE_PROCESS)
	tween.tween_property(announcement_label, "modulate:a", 1.0, 0.4)
	tween.tween_interval(1.5)
	tween.tween_property(announcement_label, "modulate:a", 0.0, 0.6)
	await tween.finished
	announcement_label.visible = false
