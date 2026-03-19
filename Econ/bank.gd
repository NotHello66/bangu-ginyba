extends StaticBody3D

var isUIOpen : bool
var Econ : Node
signal bank_clicked
var CurrentMoney :int
func on_clicked():
	print ("Bank clicked")
	if isUIOpen == false:
		$UI.visible = true
		$UI/CenterContainer/VBoxContainer/CurrentMoney.text = "Current gold: %d" % CurrentMoney
		$UI/CenterContainer/VBoxContainer/NextMoney.text = "Gold next turn %d" % (CurrentMoney * 1.2)
	

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	isUIOpen = false
	var player = get_tree().get_first_node_in_group("Player")
	Econ = player.get_node("EconomyComponent")
	var EnemySpawner = get_tree().get_first_node_in_group("EnemySpawner")
	#print(EnemySpawner.get_signal_list())
	if EnemySpawner != null:
		EnemySpawner.WaveFinished.connect(on_wave_finished)
	
func on_wave_finished():
	CurrentMoney *= 1.2
	$UI/CenterContainer/VBoxContainer/CurrentMoney.text = "Current gold: %d" % CurrentMoney
	$UI/CenterContainer/VBoxContainer/NextMoney.text = "Gold next turn %d" % (CurrentMoney * 1.2)
	
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func _on_invest_pressed() -> void:
	if Econ.currentGold >= 10:
		Econ.currentGold -= 10;
		CurrentMoney += 10;
		$UI/CenterContainer/VBoxContainer/CurrentMoney.text = "Current gold: %d" % CurrentMoney
		$UI/CenterContainer/VBoxContainer/NextMoney.text = "Gold next turn %d" % (CurrentMoney * 1.2)


func _on_withdraw_pressed() -> void:
	if CurrentMoney >= 10:
		Econ.currentGold +=10;
		CurrentMoney -= 10;
		$UI/CenterContainer/VBoxContainer/CurrentMoney.text = "Current gold: %d" % CurrentMoney
		$UI/CenterContainer/VBoxContainer/NextMoney.text = "Gold next turn %d" % (CurrentMoney * 1.2)
		

func _on_back_pressed() -> void:
	$UI.visible = false;
	isUIOpen = false;
