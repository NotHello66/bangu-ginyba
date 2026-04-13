extends StaticBody3D

@export var meleeSoldierScene : PackedScene;
@export var rangerScene : PackedScene;
@export var archerScene : PackedScene;
var Econ : Node
var isUIOpen:bool = false

func on_clicked():
	print ("Castle clicked")
	if isUIOpen == false:
		$UI.visible = true

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	isUIOpen = false
	var player = get_tree().get_first_node_in_group("Player")
	Econ = player.get_node("EconomyComponent")


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass



func _on_recruit_soldier_pressed() -> void:
	if Econ.currentGold >= 30:
		var newSoldier :CharacterBody3D = meleeSoldierScene.instantiate()
		get_tree().root.add_child(newSoldier)
		newSoldier.global_position = $Marker3D.global_position
		Econ.currentGold -= 30


func _on_back_button_pressed() -> void:
	$UI.visible =false;


func _on_recruit_ranger_pressed() -> void:
	if Econ.currentGold >= 50:
		var newSoldier :CharacterBody3D = rangerScene.instantiate()
		get_tree().root.add_child(newSoldier)
		newSoldier.global_position = $Marker3D.global_position
		Econ.currentGold -= 50


func _on_recruit_archer_pressed() -> void:
	if Econ.currentGold >= 25:
		var newSoldier :CharacterBody3D = archerScene.instantiate()
		get_tree().root.add_child(newSoldier)
		newSoldier.global_position = $Marker3D.global_position
		Econ.currentGold -= 25
