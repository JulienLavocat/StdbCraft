using Godot;

public partial class Player : CharacterBody3D
{
    private float _cameraXRotation;
    private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
    [Export] private float _jumpVelocity = 10f;
    [Export] private float _mouseSensitivity = 0.35f;
    [Export] private float _movementSpeed = 10f;

    [Export] public Node3D Head { get; set; }
    [Export] public Camera3D Camera { get; set; }
    [Export] public RayCast3D RayCast { get; set; }
    [Export] public MeshInstance3D BlockHighlight { get; set; }

    public static Player Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventMouseMotion mouseMotion) return;

        var deltaX = mouseMotion.Relative.Y * _mouseSensitivity;
        var deltaY = -mouseMotion.Relative.X * _mouseSensitivity;

        Head.RotateY(Mathf.DegToRad(deltaY));

        if (!(_cameraXRotation + deltaX > -90) || !(_cameraXRotation + deltaX < 90)) return;
        Camera.RotateX(Mathf.DegToRad(-deltaX));
        _cameraXRotation += deltaX;
    }

    public override void _Process(double delta)
    {
        if (RayCast.IsColliding() && RayCast.GetCollider() is Chunk chunk)
        {
            BlockHighlight.Visible = true;

            var blockPosition = RayCast.GetCollisionPoint() - 0.5f * RayCast.GetCollisionNormal();
            var intBlockPosition = new Vector3I(Mathf.FloorToInt(blockPosition.X), Mathf.FloorToInt(blockPosition.Y),
                Mathf.FloorToInt(blockPosition.Z));

            BlockHighlight.GlobalPosition = intBlockPosition + new Vector3(0.5f, 0.5f, 0.5f);

            if (Input.IsActionJustPressed("Break"))
                chunk.SetBlock(intBlockPosition, BlockManager.Instance.Air);
            if (Input.IsActionJustPressed("Place"))
                chunk.SetBlock(intBlockPosition + (Vector3I)RayCast.GetCollisionNormal(), BlockManager.Instance.Stone);
        }
        else
        {
            BlockHighlight.Visible = false;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;

        if (!IsOnFloor()) velocity.Y -= _gravity * (float)delta;
        if (Input.IsActionJustPressed("Jump") && IsOnFloor()) velocity.Y = _jumpVelocity;

        var inputDirection = Input.GetVector("Left", "Right", "Back", "Forward").Normalized();
        var direction = Vector3.Zero;
        direction += inputDirection.X * Head.GlobalBasis.X;
        direction += inputDirection.Y * -Head.GlobalBasis.Z;

        velocity.X = direction.X * _movementSpeed;
        velocity.Z = direction.Z * _movementSpeed;

        Velocity = velocity;
        MoveAndSlide();
    }
}