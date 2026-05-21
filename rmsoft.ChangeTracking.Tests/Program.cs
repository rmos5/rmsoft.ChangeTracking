using System.Collections.Generic;
using rmsoft.ChangeTracking;

var tests = new (string Name, Action Test)[]
{
    ("Tracked object can undo and redo property changes", TrackedObjectCanUndoAndRedoPropertyChanges),
    ("Tracked object can cancel back to original values", TrackedObjectCanCancelBackToOriginalValues),
    ("Tracked collection can undo and redo add/remove/move/replace", TrackedCollectionCanUndoAndRedoChanges),
    ("Tracked collection can cancel reset back to original items", TrackedCollectionCanCancelBackToOriginalItems),
};

foreach ((string name, Action test) in tests)
{
    test();
    Console.WriteLine($"PASS {name}");
}

static void TrackedObjectCanUndoAndRedoPropertyChanges()
{
    using TestTrackedObject item = new TestTrackedObject { IsTrackingEnabled = true, Name = "Original" };

    item.StartTracking();
    item.Name = "Updated";

    AssertEqual("Updated", item.Name, "property should change while tracking");
    AssertTrue(item.CanUndo(), "tracked object should be undoable after a property change");

    item.Undo();
    AssertEqual("Original", item.Name, "undo should restore original property value");
    AssertTrue(item.CanRedo(), "tracked object should be redoable after undo");

    item.Redo();
    AssertEqual("Updated", item.Name, "redo should restore changed property value");
}

static void TrackedObjectCanCancelBackToOriginalValues()
{
    using TestTrackedObject item = new TestTrackedObject { IsTrackingEnabled = true, Name = "Original" };

    item.StartTracking();
    item.Name = "Updated";
    item.StopTracking(cancelChanges: true);

    AssertEqual("Original", item.Name, "cancel should restore original property value");
    AssertFalse(item.HasChanges, "cancel should clear property change history");
}

static void TrackedCollectionCanUndoAndRedoChanges()
{
    using TestTrackedCollection collection = new TestTrackedCollection(new[] { "one", "two", "three" })
    {
        IsTrackingEnabled = true
    };

    collection.StartTracking();

    collection.Add("four", true);
    AssertSequence(collection, "one", "two", "three", "four");
    collection.Undo();
    AssertSequence(collection, "one", "two", "three");
    collection.Redo();
    AssertSequence(collection, "one", "two", "three", "four");

    collection.Remove("two", true);
    AssertSequence(collection, "one", "three", "four");
    collection.Undo();
    AssertSequence(collection, "one", "two", "three", "four");

    collection.Move(0, 2, true);
    AssertSequence(collection, "two", "three", "one", "four");
    collection.Undo();
    AssertSequence(collection, "one", "two", "three", "four");

    collection.ReplaceAt(1, "replacement", true);
    AssertSequence(collection, "one", "replacement", "three", "four");
    collection.Undo();
    AssertSequence(collection, "one", "two", "three", "four");
}

static void TrackedCollectionCanCancelBackToOriginalItems()
{
    using TestTrackedCollection collection = new TestTrackedCollection(new[] { "one", "two" })
    {
        IsTrackingEnabled = true
    };

    collection.StartTracking();
    collection.Add("three", true);
    collection.Clear();

    AssertFalse(collection.IsTracking, "reset should stop tracking because reset event args do not include removed items");
    AssertSequence(collection, "one", "two");
}

static void AssertSequence(IReadOnlyList<string> collection, params string[] expected)
{
    AssertEqual(expected.Length, collection.Count, "collection count mismatch");
    for (int i = 0; i < expected.Length; i++)
    {
        AssertEqual(expected[i], collection[i], $"collection item mismatch at index {i}");
    }
}

static void AssertTrue(bool actual, string message)
{
    if (!actual)
        throw new InvalidOperationException(message);
}

static void AssertFalse(bool actual, string message)
{
    if (actual)
        throw new InvalidOperationException(message);
}

static void AssertEqual<T>(T expected, T actual, string message)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException($"{message}. Expected '{expected}', actual '{actual}'.");
}

sealed class TestTrackedObject : TrackedObjectBase
{
    private string? name;

    [PropertyChangeTracker]
    public string? Name
    {
        get => name;
        set
        {
            if (name == value)
                return;

            name = value;
            OnPropertyChanged(nameof(Name));
        }
    }
}

sealed class TestTrackedCollection : TrackedObservableCollection<string>
{
    public TestTrackedCollection(IEnumerable<string> items)
        : base(items)
    {
    }
}
