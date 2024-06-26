﻿using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Godot;
using StDBCraft.Scripts.Entities;
using StdbCraft.Scripts.SpacetimeDb;
using StDBCraft.Scripts.Utils;

namespace StDBCraft.Scripts.World;

public partial class ChunkManager : Node
{
    private readonly ConcurrentDictionary<Vector2I, Chunk> _chunks = new();
    private readonly Logger _logger = new(typeof(ChunkManager));
    private float _lastUpdateTime;
    private Vector3 _playerPos = Vector3.Zero;

    private int _updateTime = 1000;
    private int _viewDistance;

    [Export] public PackedScene ChunkScene { get; set; }
    [Export] public WorldGen Generator { get; set; }
    public static ChunkManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance?.QueueFree();
        Instance = this;
    }

    public override void _PhysicsProcess(double delta)
    {
        _playerPos = Player.Instance.GlobalPosition;
        _playerPos.Y = 0;
    }

    public void Init(WorldGen generator, int viewDistance)
    {
        Generator = generator;
        _viewDistance = viewDistance;
    }

    public void BeginChunkUpdates()
    {
        new Thread(ProcessUpdateChunks).Start();
    }

    private void ProcessUpdateChunks()
    {
        while (IsInstanceValid(this))
        {
            Thread.Sleep(1000);
            UpdateChunks();
        }
    }

    public void UpdateChunks()
    {
        var startTime = Time.GetTicksMsec();

        var playerChunk = WorldUtils.ChunkFromWorldPosition(_playerPos);

        var minChunkX = playerChunk.X - _viewDistance;
        var maxChunkX = playerChunk.X + _viewDistance + 1;
        var minChunkZ = playerChunk.Y - _viewDistance;
        var maxChunkZ = playerChunk.Y + _viewDistance + 1;

        foreach (var chunkPos in _chunks.Keys.Where(
                     pos => pos.X < minChunkX || pos.X > maxChunkX || pos.Y < minChunkZ || pos.Y > maxChunkZ))
            if (_chunks.TryRemove(chunkPos, out var removed))
                removed.QueueFree();


        for (var x = minChunkX; x < maxChunkX; x++)
        for (var z = minChunkZ; z < maxChunkZ; z++)
        {
            var chunkPos = new Vector2I(x, z);
            if (_chunks.ContainsKey(chunkPos)) continue;

            if (IsInstanceValid(this))
                CallDeferred(MethodName.CreateChunk, chunkPos);

            Thread.Sleep(10);
        }

        _lastUpdateTime = Time.GetTicksMsec();
        _logger.Info("Update took ", Time.GetTicksMsec() - startTime, "ms");
    }


    public void SetBlock(Vector3I worldPosition, Block block)
    {
        var chunkPosition = WorldUtils.ChunkFromWorldPosition(worldPosition);
        ChangesRegistry.Add(new BlockChange
            { Id = 0, BlockId = block.Id, X = worldPosition.X, Y = worldPosition.Y, Z = worldPosition.Z });
        Reducer.SendBlockChange(worldPosition.X, worldPosition.Y, worldPosition.Z, block.Id);

        if (!_chunks.TryGetValue(chunkPosition, out var chunk)) return;

        var blockPosition = (Vector3I)(worldPosition - chunk.GlobalPosition);
        chunk.SetBlock(blockPosition, block);

        if (blockPosition.X != 0 && blockPosition.X != Chunk.Dimensions.X - 1 &&
            blockPosition.Z != 0 && blockPosition.Z != Chunk.Dimensions.Z - 1) return;

        if (_chunks.TryGetValue(chunkPosition + Vector2I.Left, out var left)) left.UpdateMesh(false);
        if (_chunks.TryGetValue(chunkPosition + Vector2I.Right, out var right)) right.UpdateMesh(false);
        if (_chunks.TryGetValue(chunkPosition + Vector2I.Up, out var up)) up.UpdateMesh(false);
        if (_chunks.TryGetValue(chunkPosition + Vector2I.Down, out var down)) down.UpdateMesh(false);
    }

    private void CreateChunk(Vector2I position)
    {
        var chunk = ChunkScene.Instantiate<Chunk>();
        CallDeferred(Node.MethodName.AddChild, chunk);
        chunk.Init(position, Generator);
        _chunks.TryAdd(position, chunk);
    }
}