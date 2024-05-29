using Godot;
using StdbCraft.Scripts.SpacetimeDb;

namespace StDBCraft.Scripts.World;

public partial class Chunk : StaticBody3D
{
    public static readonly Vector3I Dimensions = new(16, 64, 16);

    private static readonly Vector3I[] Vertices =
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

    private static readonly int[] Top = { 2, 3, 7, 6 };
    private static readonly int[] Bottom = { 0, 4, 5, 1 };
    private static readonly int[] Left = { 6, 4, 0, 2 };
    private static readonly int[] Right = { 3, 1, 5, 7 };
    private static readonly int[] Back = { 7, 5, 4, 6 };
    private static readonly int[] Front = { 2, 0, 1, 3 };

    private readonly Block[,,] _blocks = new Block[Dimensions.X, Dimensions.Y, Dimensions.Z];

    private readonly SurfaceTool _surfaceTool = new();
    private bool _refresh;
    [Export] public CollisionShape3D CollisionShape { get; set; }
    [Export] public MeshInstance3D MeshInstance { get; set; }

    [Export] public FastNoiseLite Noise { get; set; }

    public Vector2I ChunkPosition { get; private set; }

    public void SetChunkPosition(Vector2I position)
    {
        ChunkManager.Instance.UpdateChunkPosition(this, position, ChunkPosition);
        ChunkPosition = position;
        MeshInstance.Mesh = null;
        CallDeferred(Node3D.MethodName.SetGlobalPosition,
            new Vector3(position.X * Dimensions.X, 0, ChunkPosition.Y * Dimensions.Z));
        UpdateMesh(true);
    }

    private void GenerateBlocks()
    {
        var globalChunkPosition = ChunkPosition * new Vector2I(Dimensions.X, Dimensions.Z);

        for (var x = 0; x < Dimensions.X; x++)
        for (var y = 0; y < Dimensions.Y; y++)
        for (var z = 0; z < Dimensions.Z; z++)
        {
            var globalBlockPosition = globalChunkPosition + new Vector2(x, z);
            var groundHeight =
                (int)(Dimensions.Y * ((Noise.GetNoise2D(globalBlockPosition.X, globalBlockPosition.Y) + 1f) / 2f));

            var blockId = 0;
            if (y == groundHeight - 1) blockId = 4;
            else if (y == groundHeight - 2) blockId = 5;
            else if (y < groundHeight - 4)
                blockId = 1;
            else if (y < groundHeight)
                blockId = 2;
            else if (y == groundHeight) blockId = 3;

            _blocks[x, y, z] = BlockManager.Blocks[blockId];
        }
    }

    private void UpdateMesh(bool generateBlocks)
    {
        WorkerThreadPool.AddTask(Callable.From(() => ProcessUpdateMesh(generateBlocks)));
    }

    private void ProcessUpdateMesh(bool generateBlocks)
    {
        if (generateBlocks) GenerateBlocks();

        _surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

        for (var x = 0; x < Dimensions.X; x++)
        for (var y = 0; y < Dimensions.Y; y++)
        for (var z = 0; z < Dimensions.Z; z++)
            CreateBlockMesh(new Vector3I(x, y, z));

        _surfaceTool.SetMaterial(BlockManager.ChunkMaterial);

        var mesh = _surfaceTool.Commit();
        var collisionShape = mesh.CreateTrimeshShape();

        if (!IsInstanceValid(this)) return;
        MeshInstance.SetDeferred(MeshInstance3D.PropertyName.Mesh, mesh);
        CollisionShape.SetDeferred(CollisionShape3D.PropertyName.Shape, collisionShape);
    }

    private void CreateBlockMesh(Vector3I position)
    {
        var block = _blocks[position.X, position.Y, position.Z];

        if (CheckTransparent(position + Vector3I.Up))
            CreateFaceMesh(Top, position, block.Top != -1 ? block.Top : block.Side);
        if (CheckTransparent(position + Vector3I.Down))
            CreateFaceMesh(Bottom, position, block.Bottom != -1 ? block.Bottom : block.Side);
        if (CheckTransparent(position + Vector3I.Left))
            CreateFaceMesh(Left, position, block.Side);
        if (CheckTransparent(position + Vector3I.Right))
            CreateFaceMesh(Right, position, block.Side);
        if (CheckTransparent(position + Vector3I.Back))
            CreateFaceMesh(Back, position, block.Side);
        if (CheckTransparent(position + Vector3I.Forward))
            CreateFaceMesh(Front, position, block.Side);
    }

    private void CreateFaceMesh(int[] face, Vector3I position, int textureIndex)
    {
        if (textureIndex == -1) return;

        var texturePosition = BlockManager.GetTextureAtlasPosition(textureIndex);
        var textureAtlasSize = BlockManager.TextureAtlasSize;

        var uvOffset = texturePosition / textureAtlasSize;
        var uvWidth = 1f / textureAtlasSize.X;
        var uvHeight = 1f / textureAtlasSize.Y;

        var uvA = uvOffset + new Vector2(0, 0);
        var uvB = uvOffset + new Vector2(0, uvHeight);
        var uvC = uvOffset + new Vector2(uvWidth, uvHeight);
        var uvD = uvOffset + new Vector2(uvWidth, 0);

        var uvTriangle1 = new[] { uvA, uvB, uvC };
        var uvTriangle2 = new[] { uvA, uvC, uvD };

        var a = Vertices[face[0]] + position;
        var b = Vertices[face[1]] + position;
        var c = Vertices[face[2]] + position;
        var d = Vertices[face[3]] + position;

        var triangle1 = new Vector3[] { a, b, c };
        var triangle2 = new Vector3[] { a, c, d };

        var normal = ((Vector3)(c - a)).Cross(b - a).Normalized();
        var normals = new[]
        {
            normal, normal, normal
        };

        _surfaceTool.AddTriangleFan(triangle1, uvTriangle1, normals: normals);
        _surfaceTool.AddTriangleFan(triangle2, uvTriangle2, normals: normals);
    }

    private bool CheckTransparent(Vector3I position)
    {
        if (position.X < 0 || position.X >= Dimensions.X) return true;
        if (position.Y < 0 || position.Y >= Dimensions.Y) return true;
        if (position.Z < 0 || position.Z >= Dimensions.Z) return true;

        return _blocks[position.X, position.Y, position.Z].IsTransparent;
    }

    /**
     * Set the block using chunk coordinates, for world coordinates see ChunkManager.SetBlock()
     */
    public void SetBlock(Vector3I position, Block block)
    {
        _blocks[position.X, position.Y, position.Z] = block;
        UpdateMesh(false);
    }
}