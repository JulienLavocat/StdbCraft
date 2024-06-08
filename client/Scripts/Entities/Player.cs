using Godot;
using StDBCraft.Scripts.World;
using Chunk = StDBCraft.Scripts.World.Chunk;
using ChunkManager = StDBCraft.Scripts.World.ChunkManager;

namespace StDBCraft.Scripts.Entities;

public partial class Player : CharacterBody3D
{
    private float _cameraXRotation;
    [Export] private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle() * 2;
    [Export] private float _jumpVelocity = 6f;
    [Export] private float _mouseSensitivity = 0.35f;
    [Export] private float _movementSpeed = 10f;

    private int _selectedBlockId;

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
        Signals.Instance.OnSelectedHotbarSlot += (_, blockId) => { _selectedBlockId = blockId; };
    }

    public override void _Process(double delta)
    {
        if (!RayCast.IsColliding() || RayCast.GetCollider() is not Chunk)
        {
            BlockHighlight.Visible = false;
            return;
        }

        BlockHighlight.Visible = true;

        var collisionPoint = RayCast.GetCollisionPoint() - 0.5f * RayCast.GetCollisionNormal();
        var blockPosition = new Vector3I(Mathf.FloorToInt(collisionPoint.X), Mathf.FloorToInt(collisionPoint.Y),
            Mathf.FloorToInt(collisionPoint.Z));

        BlockHighlight.GlobalPosition = blockPosition + new Vector3(0.5f, 0.5f, 0.5f);

        if (Input.IsActionJustPressed("Break"))
            BreakBlock(blockPosition);
        if (Input.IsActionJustPressed("Place"))
            PlaceBlock((Vector3I)(blockPosition + RayCast.GetCollisionNormal()));
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

    private void PlaceBlock(Vector3I placeAt)
    {
        ShapeCast.GlobalPosition = placeAt + new Vector3(0.5f, 0.5f, 0.5f);
        ShapeCast.ForceShapecastUpdate();
        if (ShapeCast.IsColliding()) return;

        ChunkManager.Instance.SetBlock(placeAt, BlockManager.Blocks[_selectedBlockId]);
    }

    private void BreakBlock(Vector3I breakAt)
    {
        ChunkManager.Instance.SetBlock(breakAt, BlockManager.GetAir());
    }
}