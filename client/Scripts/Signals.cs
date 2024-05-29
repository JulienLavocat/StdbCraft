using Godot;

namespace StDBCraft.Scripts;

public partial class Signals : Node
{
    [Signal]
    public delegate void OnSelectedHotbarSlotEventHandler(int slotId, int blockId);

    public static Signals Instance;

    public override void _EnterTree()
    {
        Instance?.QueueFree();
        Instance = this;
    }

    public static void EmitSelectedHotbarSlot(int slotId, int blockId)
    {
        Instance.EmitSignal(SignalName.OnSelectedHotbarSlot, slotId, blockId);
    }
}