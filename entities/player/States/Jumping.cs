using Godot;
using StDBCraft.Scripts.StateMachine;

namespace StDBCraft.Entities.Player.States;

public partial class Jumping : FsmState
{
    [Export] private float _jumpVelocity = 6f;
    [Export] private float _walkSpeed = 5.612f;
    [Export] private CharacterBody3D CharacterBody { get; set; }

    public override void OnEnter()
    {
        GD.Print("[PlayerFSM] Entering Jumping");
        var velocity = CharacterBody.Velocity;
        velocity.Y = _jumpVelocity;
        CharacterBody.Velocity = velocity;
    }

    public override void OnProcess(double delta)
    {
        if (CharacterBody.IsOnFloor())
            ChangeState("Idle");
        else if (Input.IsActionJustPressed("Jump")) ChangeState("Flying");
    }

    public override void OnPhysicsProcess(double delta)
    {
        Player.Instance.ProcessMovement(delta, _walkSpeed);
    }

    public override void OnInput(InputEvent @event)
    {
        Player.Instance.ProcessRotationInput(@event);
    }
}