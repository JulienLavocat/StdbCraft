using System.Collections.Generic;
using System.Linq;
using Godot;
using StdbCraft.Scripts.SpacetimeDb;

namespace StDBCraft.Scripts.World;

public static class ChangesRegistry
{
    private static readonly Dictionary<Vector2I, Dictionary<Vector3I, int>> Changes = new();

    public static void Add(BlockChange bc)
    {
        if (Changes.TryGetValue(ChunkManager.WorldToChunk(bc.X, bc.Z), out var changesList))
            changesList[new Vector3I(bc.X, bc.Y, bc.Z)] = bc.BlockId;
        else
            Changes.Add(ChunkManager.WorldToChunk(bc.X, bc.Z), new Dictionary<Vector3I, int>
            {
                { new Vector3I(bc.X, bc.Y, bc.Z), bc.BlockId }
            });

        GD.Print("Got changes: ", Changes.Select(c => c.Value.Count).Sum());
    }

    public static void Remove(BlockChange bc)
    {
        if (!Changes.TryGetValue(ChunkManager.WorldToChunk(bc.X, bc.Z), out var changesList)) return;
        changesList.Remove(new Vector3I(bc.X, bc.Y, bc.Z));
        if (changesList.Count == 0) Changes.Remove(ChunkManager.WorldToChunk(bc.X, bc.Z));
    }


    public static bool TryGetValue(Vector2I chunk, out Dictionary<Vector3I, int> value)
    {
        return Changes.TryGetValue(chunk, out value);
    }
}