using Godot;

namespace StDBCraft.Scripts.StateMachine;

[GlobalClass]
public abstract partial class FsmState : Node
{
    [Signal]
    public delegate void StateEnteredEventHandler();

    [Signal]
    public delegate void StateExitedEventHandler();

    protected Fsm StateMachine;

    public virtual void OnEnter()
    {
    }

    public virtual void OnProcess(double delta)
    {
    }

    public virtual void OnPhysicsProcess(double delta)
    {
    }

    public virtual void OnInput(InputEvent @event)
    {
    }

    public virtual void OnExit()
    {
    }

    public virtual void OnAnimationStarted(StringName name)
    {
    }

    public virtual void OnAnimationFinished(StringName name)
    {
    }

    public virtual void OnAnimationChanged(StringName oldName, StringName newName)
    {
    }

    public void EmitStateEntered()
    {
        EmitSignal(SignalName.StateEntered);
    }

    public void EmitStateExited()
    {
        EmitSignal(SignalName.StateExited);
    }

    public void SetFsm(Fsm machine)
    {
        StateMachine = machine;
    }

    protected void ChangeState(string stateName)
    {
        if (IsInstanceValid(StateMachine))
            StateMachine.ChangeState(stateName);
        else
            GD.PushError($"State {this} has not yet been initialized");
    }
}