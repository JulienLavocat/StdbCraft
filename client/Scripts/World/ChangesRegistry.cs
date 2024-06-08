using System.Collections.Generic;
using Godot;
using StdbCraft.Scripts.SpacetimeDb;

namespace StDBCraft.Scripts.World;

public static class ChangesRegistry
{
    private static readonly Dictionary<Vector3I, int> Changes = new();

    public static void Add(BlockChange bc)
    {
        Changes[new Vector3I(bc.X, bc.Y, bc.Z)] = bc.BlockId;
    }

    public static void Remove(BlockChange bc)
    {
        Changes.Remove(new Vector3I(bc.X, bc.Y, bc.Z));
    }


    public static bool TryGetBlock(Vector3I position, out int value)
    {
        return Changes.TryGetValue(position, out value);
    }
}