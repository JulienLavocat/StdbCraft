using Godot;

public partial class MoveComponent : Node3D
{
    private readonly float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle() * 1.8f;
    private float _cameraXRotation;
    [Export] private float _jumpVelocity = 6f;
    [Export] private float _mouseSensitivity = 0.35f;
    [Export] private float _movementSpeed = 8f;

    [Export] public CharacterBody3D CharacterBody { get; set; }
    [Export] public Node3D Head { get; set; }
    [Export] public Camera3D Camera { get; set; }

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

    public override void _PhysicsProcess(double delta)
    {
        var velocity = CharacterBody.Velocity;

        if (!CharacterBody.IsOnFloor()) velocity.Y -= _gravity * (float)delta;
        if (Input.IsActionJustPressed("Jump") && CharacterBody.IsOnFloor()) velocity.Y = _jumpVelocity;

        var inputDirection = Input.GetVector("Left", "Right", "Back", "Forward").Normalized();
        var direction = Vector3.Zero;
        direction += inputDirection.X * Head.GlobalBasis.X;
        direction += inputDirection.Y * -Head.GlobalBasis.Z;

        velocity.X = direction.X * _movementSpeed;
        velocity.Z = direction.Z * _movementSpeed;

        CharacterBody.Velocity = velocity;
        CharacterBody.MoveAndSlide();
    }
}