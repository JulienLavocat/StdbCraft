using Godot;

namespace StDBCraft.Scripts.World;

[GlobalClass]
public partial class WorldGen : Resource
{
    [Export] private FastNoiseLite _continentalness;
    [Export] private Curve _height;

    public void SetSeed(int seed)
    {
        _continentalness.Seed = seed;
    }

    public int GetBlock(Vector3I position)
    {
        return GetBlock(position.X, position.Y, position.Z);
    }

    public int GetBlock(int x, int y, int z)
    {
        if (ChangesRegistry.TryGetBlock(new Vector3I(x, y, z), out var blockId)) return blockId;

        var continentalness = _continentalness.GetNoise2D(x, z);
        var terrainHeight = Mathf.Round(_height.Sample((continentalness + 1) / 2) * 200);

        return y < terrainHeight ? 1 : 0; // Stone ? Air
    }
}