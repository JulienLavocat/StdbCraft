using Godot;
using StDBCraft.Scripts.StateMachine;

namespace StDBCraft.Scripts.Entities.States;

public partial class Running : FsmState
{
    [Export] private float _runSpeed = 5.612f;

    [Export] private CharacterBody3D CharacterBody { get; set; }
    [Export] private Node3D Head { get; set; }

    public override void OnEnter()
    {
        GD.Print("[PlayerFSM] Entering Running");
    }

    public override void OnProcess(double delta)
    {
        if (Input.IsActionJustPressed("Jump"))
            ChangeState("Jumping");
        if (Player.GetInputDirection().IsZeroApprox()) ChangeState("Idle");
    }

    public override void OnPhysicsProcess(double delta)
    {
        Player.Instance.ProcessMovement(delta, _runSpeed);
    }

    public override void OnInput(InputEvent @event)
    {
        Player.Instance.ProcessRotationInput(@event);
    }
}