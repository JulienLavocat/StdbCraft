extends Label

func _process(delta: float):
	set_text("FPS: " + str(Engine.get_frames_per_second()))
	