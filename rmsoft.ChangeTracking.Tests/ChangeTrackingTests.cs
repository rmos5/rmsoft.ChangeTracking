using rmsoft.ChangeTracking;
using Xunit;

namespace rmsoft.ChangeTracking.Tests;

public class ChangeTrackingTests
{
    [Fact]
    public void TrackedObjectCanUndoAndRedoPropertyChanges()
    {
        using TestTrackedObject item = new TestTrackedObject { IsTrackingEnabled = true, Name = "Original" };

        item.StartTracking();
        item.Name = "Updated";

        Assert.Equal("Updated", item.Name);
        Assert.True(item.CanUndo());

        item.Undo();
        Assert.Equal("Original", item.Name);
        Assert.True(item.CanRedo());

        item.Redo();
        Assert.Equal("Updated", item.Name);
    }

    [Fact]
    public void TrackedObjectCanCancelBackToOriginalValues()
    {
        using TestTrackedObject item = new TestTrackedObject { IsTrackingEnabled = true, Name = "Original" };

        item.StartTracking();
        item.Name = "Updated";
        item.StopTracking(cancelChanges: true);

        Assert.Equal("Original", item.Name);
        Assert.False(item.HasChanges);
    }

    [Fact]
    public void TrackedObjectUndoesInterleavedPropertyChangesChronologically()
    {
        using TestTrackedObject item = new TestTrackedObject
        {
            IsTrackingEnabled = true,
            Name = "Original",
            Description = "First"
        };

        item.StartTracking();
        item.Name = "Second";
        item.Description = "Updated";
        item.Name = "Third";

        item.Undo();
        Assert.Equal("Second", item.Name);
        Assert.Equal("Updated", item.Description);

        item.Undo();
        Assert.Equal("Second", item.Name);
        Assert.Equal("First", item.Description);

        item.Undo();
        Assert.Equal("Original", item.Name);
        Assert.Equal("First", item.Description);

        item.Redo();
        Assert.Equal("Second", item.Name);
        Assert.Equal("First", item.Description);

        item.Redo();
        Assert.Equal("Second", item.Name);
        Assert.Equal("Updated", item.Description);

        item.Redo();
        Assert.Equal("Third", item.Name);
        Assert.Equal("Updated", item.Description);
    }

    [Fact]
    public void TrackedObjectClearsRedoHistoryAfterNewPropertyChange()
    {
        using TestTrackedObject item = new TestTrackedObject { IsTrackingEnabled = true, Name = "Original" };

        item.StartTracking();
        item.Name = "First";
        item.Name = "Second";

        item.Undo();
        Assert.Equal("First", item.Name);
        Assert.True(item.CanRedo());

        item.Name = "Branched";
        Assert.False(item.CanRedo());

        item.Undo();
        Assert.Equal("First", item.Name);
    }

    [Fact]
    public void TrackedObjectRejectsInvalidTrackedProperties()
    {
        using ReadOnlyTrackedObject item = new ReadOnlyTrackedObject { IsTrackingEnabled = true };

        Assert.Throws<InvalidOperationException>(item.StartTracking);
    }

    [Fact]
    public void TrackedCollectionCanUndoAndRedoChanges()
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

    [Fact]
    public void TrackedCollectionCanCancelResetBackToOriginalItems()
    {
        using TestTrackedCollection collection = new TestTrackedCollection(new[] { "one", "two" })
        {
            IsTrackingEnabled = true
        };

        collection.StartTracking();
        collection.Add("three", true);
        collection.Clear();

        Assert.False(collection.IsTracking);
        AssertSequence(collection, "one", "two");
    }

    [Fact]
    public void TrackedCollectionIgnoresMissingRemoveWithoutChangingSelection()
    {
        using TestTrackedCollection collection = new TestTrackedCollection(new[] { "one", "two" })
        {
            IsTrackingEnabled = true,
            SelectedItem = "two"
        };

        collection.StartTracking();
        collection.Remove("missing", true);

        AssertSequence(collection, "one", "two");
        Assert.Equal("two", collection.SelectedItem);
        Assert.False(collection.HasChanges);
    }

    private static void AssertSequence(IReadOnlyList<string> collection, params string[] expected)
    {
        Assert.Equal(expected, collection);
    }

    private sealed class TestTrackedObject : TrackedObjectBase
    {
        private string? name;
        private string? description;

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

        [PropertyChangeTracker]
        public string? Description
        {
            get => description;
            set
            {
                if (description == value)
                    return;

                description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
    }

    private sealed class ReadOnlyTrackedObject : TrackedObjectBase
    {
        [PropertyChangeTracker]
        public string Name => "Read only";
    }

    private sealed class TestTrackedCollection : TrackedObservableCollection<string>
    {
        public TestTrackedCollection(IEnumerable<string> items)
            : base(items)
        {
        }
    }
}
