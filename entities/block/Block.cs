using Godot;

[Tool]
[GlobalClass]
public partial class Block : Resource
{
    [Export] public Texture2D Texture { get; set; }
    [Export] public Texture2D TopTexture { get; set; }
    [Export] public Texture2D BottomTexture { get; set; }
    public Texture2D[] Textures => new[] { Texture, TopTexture, BottomTexture };
}