using Godot;
using StDBCraft.Scripts.StateMachine;

namespace StDBCraft.Scripts.Entities.States;

[GlobalClass]
public partial class Idle : FsmState
{
    [Export] private float _walkSpeed = 4.317f;

    public override void OnEnter()
    {
        GD.Print("[PlayerFSM] Entering Idle");
    }

    public override void OnProcess(double delta)
    {
        if (Input.IsActionPressed("Sprint")) ChangeState("Running");
        if (Input.IsActionPressed("Jump")) ChangeState("Jumping");
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