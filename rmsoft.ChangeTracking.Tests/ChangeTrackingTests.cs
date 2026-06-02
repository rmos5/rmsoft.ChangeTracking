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
    public void TrackedObjectClearsChangesWhenPropertyReturnsToOriginalValue()
    {
        using TestTrackedObject item = new TestTrackedObject { IsTrackingEnabled = true, Name = "Original" };

        item.StartTracking();
        item.Name = "Updated";
        Assert.True(item.HasChanges);
        Assert.Equal(1, item.ChangesCount);

        item.Name = "Original";

        Assert.False(item.HasChanges);
        Assert.Equal(0, item.ChangesCount);
        Assert.False(item.CanUndo());
    }

    [Fact]
    public void TrackedObjectKeepsOtherChangesWhenOnePropertyReturnsToOriginalValue()
    {
        using TestTrackedObject item = new TestTrackedObject
        {
            IsTrackingEnabled = true,
            Name = "Original",
            Description = "First"
        };

        item.StartTracking();
        item.Name = "Updated";
        item.Description = "Second";
        item.Name = "Original";

        Assert.True(item.HasChanges);
        Assert.Equal(1, item.ChangesCount);
        Assert.Equal(nameof(TestTrackedObject.Description), item.CurrentChange?.PropertyName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void TrackedObjectTreatsEmptyPropertyNameAsAllPropertiesChanged(string? propertyName)
    {
        using TestTrackedObject item = new TestTrackedObject
        {
            IsTrackingEnabled = true,
            Name = "Original",
            Description = "First"
        };

        item.StartTracking();
        item.SetValuesWithoutSpecificPropertyName("Updated", "Second", propertyName);

        Assert.Equal(2, item.ChangesCount);

        item.Undo();
        item.Undo();
        Assert.Equal("Original", item.Name);
        Assert.Equal("First", item.Description);
    }

    [Fact]
    public void TrackedObjectIgnoresUntrackedPropertyChanges()
    {
        using TestTrackedObject item = new TestTrackedObject { IsTrackingEnabled = true };

        item.StartTracking();
        item.UntrackedName = "Ignored";

        Assert.False(item.HasChanges);
        Assert.False(item.CanUndo());
    }

    [Fact]
    public void TrackedObjectIgnoresNoOpPropertyAssignments()
    {
        using TestTrackedObject item = new TestTrackedObject { IsTrackingEnabled = true, Name = "Original" };

        item.StartTracking();
        item.Name = "Original";
        item.RaiseTrackedNameChanged();

        Assert.False(item.HasChanges);
        Assert.False(item.CanUndo());
    }

    [Theory]
    [InlineData(typeof(ReadOnlyTrackedObject))]
    [InlineData(typeof(StaticTrackedObject))]
    [InlineData(typeof(IndexerTrackedObject))]
    public void TrackedObjectRejectsUnsupportedTrackedPropertyShapes(Type itemType)
    {
        using TrackedObjectBase item = (TrackedObjectBase)Activator.CreateInstance(itemType)!;
        item.IsTrackingEnabled = true;

        Assert.Throws<InvalidOperationException>(item.StartTracking);
    }

    [Fact]
    public void TrackedObjectEnforcesTrackingStateGuards()
    {
        using TestTrackedObject disabled = new TestTrackedObject();
        Assert.Throws<InvalidOperationException>(disabled.StartTracking);

        using TestTrackedObject item = new TestTrackedObject { IsTrackingEnabled = true };
        Assert.Throws<InvalidOperationException>(() => item.StopTracking(cancelChanges: false));

        item.StartTracking();
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

    [Fact]
    public void TrackedCollectionClearsRedoHistoryAfterNewChange()
    {
        using TestTrackedCollection collection = new TestTrackedCollection(new[] { "one" })
        {
            IsTrackingEnabled = true
        };

        collection.StartTracking();
        collection.Add("two", true);
        collection.Add("three", true);

        collection.Undo();
        AssertSequence(collection, "one", "two");
        Assert.True(collection.CanRedo());

        collection.Add("branch", true);
        Assert.False(collection.CanRedo());

        collection.Undo();
        AssertSequence(collection, "one", "two");
    }

    [Fact]
    public void TrackedCollectionClearsChangesWhenItemsReturnToOriginalSequence()
    {
        using TestTrackedCollection collection = new TestTrackedCollection(new[] { "one", "two" })
        {
            IsTrackingEnabled = true
        };

        collection.StartTracking();
        collection.Add("three", true);
        Assert.True(collection.HasChanges);
        Assert.Equal(1, collection.ChangesCount);

        collection.Remove("three", true);

        AssertSequence(collection, "one", "two");
        Assert.False(collection.HasChanges);
        Assert.Equal(0, collection.ChangesCount);
        Assert.False(collection.CanUndo());
    }

    private static void AssertSequence(IReadOnlyList<string> collection, params string[] expected)
    {
        Assert.Equal(expected, collection);
    }

    private sealed class TestTrackedObject : TrackedObjectBase
    {
        private string? name;
        private string? description;
        private string? untrackedName;

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

        public string? UntrackedName
        {
            get => untrackedName;
            set
            {
                if (untrackedName == value)
                    return;

                untrackedName = value;
                OnPropertyChanged(nameof(UntrackedName));
            }
        }

        public void RaiseTrackedNameChanged()
        {
            OnPropertyChanged(nameof(Name));
        }

        public void SetValuesWithoutSpecificPropertyName(string name, string description, string? propertyName)
        {
            this.name = name;
            this.description = description;
            OnPropertyChanged(propertyName!);
        }
    }

    private sealed class ReadOnlyTrackedObject : TrackedObjectBase
    {
        [PropertyChangeTracker]
        public string Name => "Read only";
    }

    private sealed class StaticTrackedObject : TrackedObjectBase
    {
        [PropertyChangeTracker]
        public static string Name { get; set; } = "Static";
    }

    private sealed class IndexerTrackedObject : TrackedObjectBase
    {
        [PropertyChangeTracker]
        public string this[int index]
        {
            get => index.ToString();
            set { }
        }
    }

    private sealed class TestTrackedCollection : TrackedObservableCollection<string>
    {
        public TestTrackedCollection(IEnumerable<string> items)
            : base(items)
        {
        }
    }
}
