using Godot;

namespace StDBCraft.Scripts.Ui.Hotbar;

public partial class Slot : MarginContainer
{
    private static readonly Color InactiveColor = Color.FromString("3b3b3b93", Colors.Black);
    private static readonly Color ActiveColor = Color.FromString("b9b9b9", Colors.Black);
    [Export] private TextureRect Item { get; set; }
    [Export] private ColorRect Background { get; set; }

    public int BlockId { get; private set; }

    public void SetItem(Texture2D texture, int blockId)
    {
        Item.Texture = texture;
        BlockId = blockId;
    }

    public void SetActive(bool isActive)
    {
        Background.Color = isActive ? ActiveColor : InactiveColor;
    }
}