using Godot;
using StdbCraft.Scripts.SpacetimeDb;

namespace StDBCraft.Scripts.World;

public partial class Chunk : StaticBody3D
{
    public static readonly Vector3I Dimensions = new(16, 256, 16);

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
    private WorldGen _worldGen;

    [Export] public CollisionShape3D CollisionShape { get; set; }
    [Export] public MeshInstance3D MeshInstance { get; set; }

    private Vector2I ChunkPosition { get; set; }

    public void Init(Vector2I position, WorldGen generator)
    {
        _worldGen = generator;
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
            var blockId = _worldGen.GetBlock(x + globalChunkPosition.X, y, z + globalChunkPosition.Y);
            _blocks[x, y, z] = BlockManager.Blocks[blockId];
        }
    }

    public void UpdateMesh(bool generateBlocks)
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

        if (!IsInstanceValid(_surfaceTool)) return;
        _surfaceTool.SetMaterial(TextureAtlas.ChunkMaterial);

        _surfaceTool.Index();
        var mesh = _surfaceTool.Commit();
        var collisionShape = mesh.CreateTrimeshShape();

        if (!IsInstanceValid(this)) return;
        MeshInstance.SetDeferred(MeshInstance3D.PropertyName.Mesh, mesh);
        CollisionShape.SetDeferred(CollisionShape3D.PropertyName.Shape, collisionShape);
    }

    private void CreateBlockMesh(Vector3I position)
    {
        var block = _blocks[position.X, position.Y, position.Z];

        if (IsBlockTransparent(position + Vector3I.Up))
            CreateFaceMesh(Top, position, block.Top);
        if (IsBlockTransparent(position + Vector3I.Down))
            CreateFaceMesh(Bottom, position, block.Bottom);
        if (IsBlockTransparent(position + Vector3I.Left))
            CreateFaceMesh(Left, position, block.Left);
        if (IsBlockTransparent(position + Vector3I.Right))
            CreateFaceMesh(Right, position, block.Right);
        if (IsBlockTransparent(position + Vector3I.Back))
            CreateFaceMesh(Back, position, block.Back);
        if (IsBlockTransparent(position + Vector3I.Forward))
            CreateFaceMesh(Front, position, block.Front);
    }

    private void CreateFaceMesh(int[] face, Vector3I position, int textureIndex)
    {
        if (textureIndex == -1) return;

        var texturePosition = TextureAtlas.GetTextureAtlasPosition(textureIndex);
        var textureAtlasSize = TextureAtlas.TextureAtlasSize;

        var uvOffset = texturePosition / textureAtlasSize;
        var uvWidth = 1f / textureAtlasSize.X;
        var uvHeight = 1f / textureAtlasSize.Y;

        var uvB = uvOffset + new Vector2(0, uvHeight);
        var uvC = uvOffset + new Vector2(uvWidth, uvHeight);
        var uvD = uvOffset + new Vector2(uvWidth, 0);

        var uvTriangle1 = new[] { uvOffset, uvB, uvC };
        var uvTriangle2 = new[] { uvOffset, uvC, uvD };

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

    private bool IsBlockTransparent(Vector3I position)
    {
        if (IsChunkEdgeOrOutside(position))
            return BlockManager.Blocks[_worldGen.GetBlock(WorldUtils.WorldFromChunkPosition(position, ChunkPosition))]
                .IsTransparent;
        return _blocks[position.X, position.Y, position.Z].IsTransparent;
    }

    private static bool IsChunkEdgeOrOutside(Vector3I position)
    {
        if (position.X < 0 || position.X >= Dimensions.X) return true;
        if (position.Y < 0 || position.Y >= Dimensions.Y) return true;
        return position.Z < 0 || position.Z >= Dimensions.Z;
    }

    /**
     * Set the block using chunk coordinates, for world coordinates see ChunkManager.SetBlock()
     */
    public void SetBlock(Vector3I position, Block block)
    {
        _blocks[position.X, position.Y, position.Z] = block;
        UpdateMesh(false);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey && Input.IsKeyPressed(Key.P))
            GetViewport().DebugDraw = (Viewport.DebugDrawEnum)(((int)GetViewport().DebugDraw + 1) % 4);
    }
}