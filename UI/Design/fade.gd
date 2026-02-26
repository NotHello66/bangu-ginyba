extends CanvasLayer
@onready var color_rect: ColorRect = $ColorRect


func _ready() -> void:
	color_rect.color.a = 0.0

func fade(target_alpha: float, duration: float = 1.0):
	var tween = create_tween()
	tween.set_ease(Tween.EASE_IN_OUT)
	tween.set_trans(Tween.TRANS_SINE)
	tween.tween_property(color_rect, "color:a", target_alpha, duration)
	return tween
