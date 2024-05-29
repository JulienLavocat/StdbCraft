using System.Linq;
using Godot;
using StDBCraft.Scripts.World;

namespace StDBCraft.Scripts.Ui.Hotbar;

public partial class Hotbar : HBoxContainer
{
    private int _activeSlotIndex;
    private Slot[] _slots;

    public override void _Ready()
    {
        _slots = GetChildren().OfType<Slot>().ToArray();

        var slotIndex = 0;
        foreach (var block in BlockManager.Blocks)
        {
            var textureId = block.Top != -1 ? block.Top : block.Side;
            GD.Print("Checking ", block.Id, " ", textureId);
            if (textureId == -1) continue;

            SetItemAt(slotIndex, BlockManager.GetTexture(textureId), block.Id);
            slotIndex++;
        }

        SetSelectedHotbarSlot(0);
    }

    private void SetItemAt(int slotIndex, Texture2D item, int blockId)
    {
        GD.Print("Setting ", slotIndex, " ", blockId);
        _slots[slotIndex].SetItem(item, blockId);
    }

    private void SetSelectedHotbarSlot(int slotIndex)
    {
        _slots[_activeSlotIndex].SetActive(false);
        _slots[slotIndex].SetActive(true);
        _activeSlotIndex = slotIndex;

        GD.Print(_activeSlotIndex, " ", _slots[_activeSlotIndex].BlockId);
        Signals.EmitSelectedHotbarSlot(_activeSlotIndex, _slots[_activeSlotIndex].BlockId);
    }

    public override void _Process(double delta)
    {
        var newActiveSlotIndex = _activeSlotIndex;

        if (Input.IsActionJustReleased("ScrollUp")) newActiveSlotIndex--;
        if (Input.IsActionJustReleased("ScrollDown")) newActiveSlotIndex++;

        if (newActiveSlotIndex > _slots.Length - 1) newActiveSlotIndex = 0;
        if (newActiveSlotIndex < 0) newActiveSlotIndex = _slots.Length - 1;

        if (newActiveSlotIndex != _activeSlotIndex) SetSelectedHotbarSlot(newActiveSlotIndex);
    }
}