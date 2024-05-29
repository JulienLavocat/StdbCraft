using System.Linq;
using Godot;
using Godot.Collections;
using StDBCraft.Scripts.Utils;
using Block = StdbCraft.SpacetimeDb.Block;

namespace StDBCraft.Scripts;

public partial class BlockManager : Node
{
    private const int GridWidth = 4;
    private readonly Dictionary<Texture2D, Vector2I> _atlasLookup = new();
    private readonly Logger _logger = new(typeof(BlockManager));
    private int _gridHeight;

    [Export] private Texture2D[] _textures;

    private Vector2I BlockTextureSize { get; set; } = new(16, 16);
    public Vector2 TextureAtlasSize { get; private set; }

    public static BlockManager Instance { get; private set; }

    public StandardMaterial3D ChunkMaterial { get; private set; }

    public Block[] Blocks { get; private set; }

    public override void _EnterTree()
    {
        Instance?.QueueFree();
        Instance = this;
    }

    public void GenerateTextureAtlas()
    {
        Blocks = Block.Iter().ToArray();
        var blockTextures = Block.Iter().SelectMany(b => new[] { b.Top, b.Bottom, b.Side }).Where(index => index != -1)
            .Distinct().Select(textureIndex => _textures[textureIndex]).ToArray();

        for (var i = 0; i < blockTextures.Length; i++)
        {
            var texture = blockTextures[i];
            _atlasLookup.Add(texture, new Vector2I(i % GridWidth, Mathf.FloorToInt(i / (float)GridWidth)));
        }

        _gridHeight = Mathf.CeilToInt(blockTextures.Length / (float)GridWidth);

        var image = Image.Create(GridWidth * BlockTextureSize.X, _gridHeight * BlockTextureSize.Y, false,
            Image.Format.Rgba8);

        for (var x = 0; x < GridWidth; x++)
        for (var y = 0; y < _gridHeight; y++)
        {
            var imgIndex = x + y * GridWidth;
            if (imgIndex >= blockTextures.Length) continue;

            var currentImage = blockTextures[imgIndex].GetImage();
            currentImage.Convert(Image.Format.Rgba8);

            image.BlitRect(currentImage, new Rect2I(Vector2I.Zero, BlockTextureSize),
                new Vector2I(x, y) * BlockTextureSize);
        }

        var textureAtlas = ImageTexture.CreateFromImage(image);
        ChunkMaterial = new StandardMaterial3D
        {
            AlbedoTexture = textureAtlas,
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
            Transparency = BaseMaterial3D.TransparencyEnum.AlphaScissor
        };

        TextureAtlasSize = new Vector2(GridWidth, _gridHeight);
        _logger.Info($"Loaded {blockTextures.Length} into a {GridWidth}x{_gridHeight} texture atlas");
    }

    public Vector2I GetTextureAtlasPosition(int textureIndex)
    {
        var texture = _textures[textureIndex];
        return texture == null ? Vector2I.Zero : _atlasLookup[texture];
    }
}