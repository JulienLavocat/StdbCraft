using Godot;
using StDBCraft.Scenes.Chunk;
using StDBCraft.Scenes.ChunkManager;
using StDBCraft.Scripts;

namespace StDBCraft.Entities.Player;

public partial class Player : CharacterBody3D
{
    private float _cameraXRotation;
    [Export] private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle() * 2;
    [Export] private float _jumpVelocity = 6f;
    [Export] private float _mouseSensitivity = 0.35f;
    [Export] private float _movementSpeed = 10f;

    [Export] private RayCast3D RayCast { get; set; }
    [Export] public MeshInstance3D BlockHighlight { get; set; }
    [Export] private ShapeCast3D ShapeCast { get; set; }
    [Export] private Node3D Head { get; set; }
    [Export] private Camera3D Camera { get; set; }

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
                ChunkManager.Instance.SetBlock(intBlockPosition, BlockManager.Instance.Blocks[0]);
            if (Input.IsActionJustPressed("Place"))
            {
                var placeAt = (Vector3I)(intBlockPosition + RayCast.GetCollisionNormal());

                ShapeCast.GlobalPosition = placeAt + new Vector3(0.5f, 0.5f, 0.5f);
                ShapeCast.ForceShapecastUpdate();
                if (ShapeCast.IsColliding()) return;

                ChunkManager.Instance.SetBlock(placeAt, BlockManager.Instance.Blocks[1]);
            }
        }
        else
        {
            BlockHighlight.Visible = false;
        }
    }

    public static Vector2 GetInputDirection()
    {
        return Input.GetVector("Left", "Right", "Back", "Forward").Normalized();
    }

    public void ProcessRotationInput(InputEvent @event)
    {
        if (@event is not InputEventMouseMotion mouseMotion) return;

        var deltaX = mouseMotion.Relative.Y * _mouseSensitivity;
        var deltaY = -mouseMotion.Relative.X * _mouseSensitivity;

        Head.RotateY(Mathf.DegToRad(deltaY));

        if (!(_cameraXRotation + deltaX > -91) || !(_cameraXRotation + deltaX < 90)) return;
        Camera.RotateX(Mathf.DegToRad(-deltaX));
        _cameraXRotation += deltaX;
    }

    public void ProcessMovement(double delta, float speed, bool withGravity = true)
    {
        var velocity = Velocity;

        if (withGravity && !IsOnFloor()) velocity.Y -= _gravity * (float)delta;

        var inputDirection = Input.GetVector("Left", "Right", "Back", "Forward").Normalized();
        var direction = Vector3.Zero;
        direction += inputDirection.X * Head.GlobalBasis.X;
        direction += inputDirection.Y * -Head.GlobalBasis.Z;

        velocity.X = direction.X * speed;
        velocity.Z = direction.Z * speed;

        Velocity = velocity;
        MoveAndSlide();
    }
}