using System.Collections.Generic;
using System.Linq;
using Godot;
using StdbCraft.Scripts.SpacetimeDb;
using StDBCraft.Scripts.Utils;

namespace StDBCraft.Scripts.World;

public static class TextureAtlas
{
    private const int GridWidth = 4;
    private static readonly Dictionary<Texture2D, Vector2I> AtlasLookup = new();
    private static int _gridHeight;

    private static Texture2D[] _textures;
    private static readonly Logger Logger = new(typeof(TextureAtlas));

    private static Vector2I BlockTextureSize { get; } = new(16, 16);
    public static Vector2 TextureAtlasSize { get; private set; }

    public static StandardMaterial3D ChunkMaterial { get; private set; }

    public static void GenerateTextureAtlas(Block[] blocks)
    {
        var blockTextures = blocks.SelectMany(b => new[] { b.Top, b.Bottom, b.Left, b.Right, b.Front, b.Back })
            .Where(index => index != -1)
            .Distinct().Select(textureIndex => _textures[textureIndex]).ToArray();

        for (var i = 0; i < blockTextures.Length; i++)
        {
            var texture = blockTextures[i];
            AtlasLookup.Add(texture, new Vector2I(i % GridWidth, Mathf.FloorToInt(i / (float)GridWidth)));
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
        Logger.Info($"Loaded {blockTextures.Length} into a {GridWidth}x{_gridHeight} texture atlas");
    }

    public static Vector2I GetTextureAtlasPosition(int textureIndex)
    {
        var texture = _textures[textureIndex];
        return texture == null ? Vector2I.Zero : AtlasLookup[texture];
    }

    public static Texture2D GetTexture(int textureId)
    {
        return _textures[textureId];
    }

    public static void SetTextures(Texture2D[] textures)
    {
        _textures = textures;
    }
}