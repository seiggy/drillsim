namespace ReservoirSimulation.Domain;

internal interface IWorldStore
{
    int Count { get; }
    void Upsert(ReservoirWorld world);
    bool TryGet(string worldId, out ReservoirWorld? world);
    void Remove(string worldId);
}

internal sealed class InMemoryWorldStore : IWorldStore
{
    private readonly int _capacity;
    private readonly Dictionary<string, Entry> _worlds = new(StringComparer.Ordinal);
    private readonly Lock _lock = new();
    private long _accessSequence;

    internal InMemoryWorldStore(int capacity = 4)
    {
        if (capacity is < 1 or > 64)
            throw new ArgumentOutOfRangeException(nameof(capacity), "World store capacity must be between 1 and 64.");
        _capacity = capacity;
    }

    public int Count
    {
        get
        {
            lock (_lock)
                return _worlds.Count;
        }
    }

    public void Upsert(ReservoirWorld world)
    {
        lock (_lock)
        {
            _worlds[world.Summary.WorldId] = new Entry(world, NextSequence());
            if (_worlds.Count <= _capacity)
                return;

            string evictedId = _worlds
                .OrderBy(pair => pair.Value.LastAccess)
                .ThenBy(pair => pair.Key, StringComparer.Ordinal)
                .First().Key;
            _worlds.Remove(evictedId);
        }
    }

    public bool TryGet(string worldId, out ReservoirWorld? world)
    {
        lock (_lock)
        {
            if (!_worlds.TryGetValue(worldId, out Entry? entry))
            {
                world = null;
                return false;
            }
            entry.LastAccess = NextSequence();
            world = entry.World;
            return true;
        }
    }

    public void Remove(string worldId)
    {
        lock (_lock)
            _worlds.Remove(worldId);
    }


    private long NextSequence() => checked(++_accessSequence);

    private sealed class Entry(ReservoirWorld world, long lastAccess)
    {
        internal ReservoirWorld World { get; } = world;
        internal long LastAccess { get; set; } = lastAccess;
    }
}
