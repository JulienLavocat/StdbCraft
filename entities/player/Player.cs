using Godot;

public partial class Player : CharacterBody3D
{
    private float _cameraXRotation;
    private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
    [Export] private float _jumpVelocity = 10f;
    [Export] private float _mouseSensitivity = 0.35f;
    [Export] private float _movementSpeed = 10f;

    [Export] public RayCast3D RayCast { get; set; }
    [Export] public MeshInstance3D BlockHighlight { get; set; }
    [Export] public ShapeCast3D ShapeCast { get; set; }

    public static Player Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Process(double delta)
    {
        if (RayCast.IsColliding() && RayCast.GetCollider() is Chunk)
        {
            BlockHighlight.Visible = true;

            var blockPosition = RayCast.GetCollisionPoint() - 0.5f * RayCast.GetCollisionNormal();
            var intBlockPosition = new Vector3I(Mathf.FloorToInt(blockPosition.X), Mathf.FloorToInt(blockPosition.Y),
                Mathf.FloorToInt(blockPosition.Z));

            BlockHighlight.GlobalPosition = intBlockPosition + new Vector3(0.5f, 0.5f, 0.5f);

            if (Input.IsActionJustPressed("Break"))
                ChunkManager.Instance.SetBlock(intBlockPosition, BlockManager.Instance.Air);
            if (Input.IsActionJustPressed("Place"))
            {
                var placeAt = (Vector3I)(intBlockPosition + RayCast.GetCollisionNormal());

                ShapeCast.GlobalPosition = placeAt + new Vector3(0.5f, 0.5f, 0.5f);
                ShapeCast.ForceShapecastUpdate();
                if (ShapeCast.IsColliding()) return;

                ChunkManager.Instance.SetBlock(placeAt, BlockManager.Instance.Stone);
            }
        }
        else
        {
            BlockHighlight.Visible = false;
        }
    }
}