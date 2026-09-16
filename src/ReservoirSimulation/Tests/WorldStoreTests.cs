using ReservoirSimulation.Domain;

namespace ReservoirSimulation.Tests;

[TestFixture]
public sealed class WorldStoreTests
{
    [Test]
    public void Upsert_AtCapacity_EvictsLeastRecentlyUsedAndReplacesSameId()
    {
        var factory = new ReservoirWorldFactory();
        ReservoirWorld first = factory.Create(TestData.ConditionedRequest(1));
        ReservoirWorld second = factory.Create(TestData.ConditionedRequest(2));
        ReservoirWorld third = factory.Create(TestData.ConditionedRequest(3));
        var store = new InMemoryWorldStore(2);
        store.Upsert(first);
        store.Upsert(second);
        Assert.That(store.TryGet(first.Summary.WorldId, out _), Is.True);

        store.Upsert(third);

        Assert.Multiple(() =>
        {
            Assert.That(store.Count, Is.EqualTo(2));
            Assert.That(store.TryGet(second.Summary.WorldId, out _), Is.False);
            Assert.That(store.TryGet(first.Summary.WorldId, out _), Is.True);
            Assert.That(store.TryGet(third.Summary.WorldId, out _), Is.True);
        });

        ReservoirWorld replacement = factory.Create(TestData.ConditionedRequest(1));
        store.Upsert(replacement);
        Assert.That(store.TryGet(first.Summary.WorldId, out ReservoirWorld? stored), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(store.Count, Is.EqualTo(2));
            Assert.That(stored, Is.SameAs(replacement));
        });
    }
}
