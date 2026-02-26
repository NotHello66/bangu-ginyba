extends Panel
@onready var controls_settings: Panel = $"."
@onready var wasd_button: CheckButton = $wasdButton
@onready var arrow_button: CheckButton = $arrowButton
@onready var settings: Panel = $"../settings"

func _ready() -> void:
	if Global.using_wasd:
		wasd_button.button_pressed = true
		arrow_button.button_pressed = false
		set_wasd()
	else:
		wasd_button.button_pressed = false
		arrow_button.button_pressed = true
		set_arrows()

func _on_wasd_button_pressed() -> void:
	if wasd_button.button_pressed:
		arrow_button.button_pressed = false
		Global.using_wasd = true
		set_wasd()
	else:
		wasd_button.button_pressed = true


func _on_arrow_button_pressed() -> void:
	if arrow_button.button_pressed:
		wasd_button.button_pressed = false
		Global.using_wasd = false
		set_arrows()
	else:
		arrow_button.button_pressed = true

func set_wasd() -> void:
	InputMap.action_erase_events("moveLeft")
	InputMap.action_erase_events("moveRight")
	InputMap.action_erase_events("moveUp")
	InputMap.action_erase_events("moveDown")
	add_key("moveLeft", KEY_A)
	add_key("moveRight", KEY_D)
	add_key("moveUp", KEY_W)
	add_key("moveDown", KEY_S)

func set_arrows() -> void:
	InputMap.action_erase_events("moveLeft")
	InputMap.action_erase_events("moveRight")
	InputMap.action_erase_events("moveUp")
	InputMap.action_erase_events("moveDown")
	add_key("moveLeft", KEY_LEFT)
	add_key("moveRight", KEY_RIGHT)
	add_key("moveUp", KEY_UP)
	add_key("moveDown", KEY_DOWN)

func add_key(action: String, key: Key) -> void:
	var event = InputEventKey.new()
	event.keycode = key
	InputMap.action_add_event(action, event)


func _on_back_button_pressed() -> void:
	controls_settings.visible = false
	settings.visible = true
