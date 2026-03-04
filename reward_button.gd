extends Button

@onready var reward_system_ui: Control = $"../reward/rewardSystemUI"

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	reward_system_ui.visible = false

func _on_pressed() -> void:
	Global.add_pause("reward")
	if reward_system_ui.visible:
		reward_system_ui.visible = false
		get_tree().paused = false
	else:
		reward_system_ui.visible = true
		get_tree().paused = true
