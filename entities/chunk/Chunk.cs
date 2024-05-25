using Godot;

[Tool]
public partial class Chunk : StaticBody3D
{
    public static Vector3I dimensions = new(16, 64, 16);

    private static readonly Vector3I[] _vertices =
    {
        new(0, 0, 0),
        new(1, 0, 0),
        new(0, 1, 0),
        new(1, 1, 0),
        new(0, 0, 1),
        new(1, 0, 1),
        new(0, 1, 1),
        new(1, 1, 1)
    };

    private static readonly int[] _top = { 2, 3, 7, 6 };
    private static readonly int[] _bottom = { 0, 4, 5, 1 };
    private static readonly int[] _left = { 6, 4, 0, 2 };
    private static readonly int[] _right = { 3, 1, 5, 7 };
    private static readonly int[] _back = { 7, 5, 4, 6 };
    private static readonly int[] _front = { 2, 0, 1, 3 };

    private readonly Block[,,] _blocks = new Block[dimensions.X, dimensions.Y, dimensions.Z];
    private bool _refresh;

    private SurfaceTool _surfaceTool = new();
    [Export] public CollisionShape3D CollisionShape { get; set; }
    [Export] public MeshInstance3D MeshInstance { get; set; }

    [Export]
    public bool refresh
    {
        get => _refresh;
        set
        {
            _refresh = value;
            Generate();
            Update();
        }
    }

    [Export] public FastNoiseLite Noise { get; set; }

    public Vector2I ChunkPosition { get; private set; }


    public override void _Ready()
    {
        ChunkPosition =
            new Vector2I(Mathf.FloorToInt(GlobalPosition.X / dimensions.X),
                Mathf.FloorToInt(GlobalPosition.Z / dimensions.Z));
        Generate();
        Update();
    }

    private void Generate()
    {
        var globalChunkPosition = ChunkPosition * new Vector2I(dimensions.X, dimensions.Z);

        for (var x = 0; x < dimensions.X; x++)
        for (var y = 0; y < dimensions.Y; y++)
        for (var z = 0; z < dimensions.Z; z++)
        {
            var globalBlockPosition = globalChunkPosition + new Vector2(x, z);
            var groundHeight =
                (int)(dimensions.Y * ((Noise.GetNoise2D(globalBlockPosition.X, globalBlockPosition.Y) + 1f) / 2f));

            var block = BlockManager.Instance.Air;
            if (y < groundHeight - 4)
                block = BlockManager.Instance.Stone;
            else if (y < groundHeight)
                block = BlockManager.Instance.Dirt;
            else if (y == groundHeight) block = BlockManager.Instance.Grass;

            _blocks[x, y, z] = block;
        }
    }

    private void Update()
    {
        _surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

        for (var x = 0; x < dimensions.X; x++)
        for (var y = 0; y < dimensions.Y; y++)
        for (var z = 0; z < dimensions.Z; z++)
            CreateBlockMesh(new Vector3I(x, y, z));

        _surfaceTool.SetMaterial(BlockManager.Instance.ChunkMaterial);

        var mesh = _surfaceTool.Commit();

        MeshInstance.Mesh = mesh;
        CollisionShape.Shape = mesh.CreateTrimeshShape();
    }

    private void CreateBlockMesh(Vector3I position)
    {
        var block = _blocks[position.X, position.Y, position.Z];

        if (block == BlockManager.Instance.Air) return;

        if (CheckTransparent(position + Vector3I.Up))
            CreateFaceMesh(_top, position, block.TopTexture ?? block.Texture);
        if (CheckTransparent(position + Vector3I.Down))
            CreateFaceMesh(_bottom, position, block.BottomTexture ?? block.Texture);
        if (CheckTransparent(position + Vector3I.Left))
            CreateFaceMesh(_left, position, block.Texture ?? block.Texture);
        if (CheckTransparent(position + Vector3I.Right))
            CreateFaceMesh(_right, position, block.Texture ?? block.Texture);
        if (CheckTransparent(position + Vector3I.Back))
            CreateFaceMesh(_back, position, block.Texture ?? block.Texture);
        if (CheckTransparent(position + Vector3I.Forward))
            CreateFaceMesh(_front, position, block.Texture ?? block.Texture);
    }

    private void CreateFaceMesh(int[] face, Vector3I position, Texture2D texture)
    {
        var texturePosition = BlockManager.Instance.GetTextureAtlasPosition(texture);
        var textureAtlasSize = BlockManager.Instance.TextureAtlasSize;

        var uvOffset = texturePosition / textureAtlasSize;
        var uvWidth = 1f / textureAtlasSize.X;
        var uvHeight = 1f / textureAtlasSize.Y;

        var uvA = uvOffset + new Vector2(0, 0);
        var uvB = uvOffset + new Vector2(0, uvHeight);
        var uvC = uvOffset + new Vector2(uvWidth, uvHeight);
        var uvD = uvOffset + new Vector2(uvWidth, 0);

        var uvTriangle1 = new[] { uvA, uvB, uvC };
        var uvTriangle2 = new[] { uvA, uvC, uvD };

        var a = _vertices[face[0]] + position;
        var b = _vertices[face[1]] + position;
        var c = _vertices[face[2]] + position;
        var d = _vertices[face[3]] + position;

        var triangle1 = new Vector3[] { a, b, c };
        var triangle2 = new Vector3[] { a, c, d };

        _surfaceTool.AddTriangleFan(triangle1, uvTriangle1);
        _surfaceTool.AddTriangleFan(triangle2, uvTriangle2);
    }

    private bool CheckTransparent(Vector3I position)
    {
        if (position.X < 0 || position.X >= dimensions.X) return true;
        if (position.Y < 0 || position.Y >= dimensions.Y) return true;
        if (position.Z < 0 || position.Z >= dimensions.Z) return true;

        return _blocks[position.X, position.Y, position.Z] == BlockManager.Instance.Air;
    }

    /**
     * Set the block using chunk coordinates, for world coordinates see SetBlock()
     */
    public void SetBlockRelative(Vector3I position, Block block)
    {
        _blocks[position.X, position.Y, position.Z] = block;
        Update();
    }

    /**
     * Set the block using world coordinates, for chunk coordinates see SetBlockRelative()
     */
    public void SetBlock(Vector3I position, Block block)
    {
        GD.Print("Setblock called");
        SetBlockRelative((Vector3I)(position - GlobalPosition), block);
    }
}