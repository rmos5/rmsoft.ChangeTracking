# rmsoft.ChangeTracking

Change tracking helpers for .NET objects and observable collections.

`rmsoft.ChangeTracking` provides small base classes for tracking edits, undoing and redoing changes, and cancelling back to original values. It is built around familiar .NET binding primitives such as `INotifyPropertyChanged`, `ObservableCollection<T>`, `INotifyCollectionChanged`, and `ICommand`.

## Targets

- `netstandard2.0`
- `net8.0`

The `netstandard2.0` target keeps the library broadly usable, including from .NET MAUI projects. UI-bound changes should still be made on the UI thread in MAUI, WPF, WinUI, or similar UI frameworks.

## Install

When published as a NuGet package:

```powershell
dotnet add package rmsoft.ChangeTracking
```

For local development, reference the project directly:

```xml
<ItemGroup>
  <ProjectReference Include="..\rmsoft.ChangeTracking\rmsoft.ChangeTracking.csproj" />
</ItemGroup>
```

## Track Object Changes

Derive from `TrackedObjectBase` and mark properties that should participate in change tracking with `[PropertyChangeTracker]`.

```csharp
using rmsoft.ChangeTracking;

public sealed class Person : TrackedObjectBase
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
```

Use the tracking API:

```csharp
using Person person = new Person
{
    IsTrackingEnabled = true,
    Name = "Original"
};

person.StartTracking();
person.Name = "Updated";

person.Undo(); // Name == "Original"
person.Redo(); // Name == "Updated"

person.StopTracking(cancelChanges: true); // restore original values and clear history
```

Tracked properties must be instance properties with a getter and setter. Indexers, static properties, and read-only properties are rejected when tracking starts.

## Track Collection Changes

Derive from `TrackedObservableCollection<T>` to track add, remove, move, replace, clear/reset, undo, and redo operations.

```csharp
using rmsoft.ChangeTracking;

public sealed class People : TrackedObservableCollection<Person>
{
    public People(IEnumerable<Person> items)
        : base(items)
    {
        IsTrackingEnabled = true;
    }
}
```

```csharp
using People people = new People(new[]
{
    new Person { Name = "One" },
    new Person { Name = "Two" }
});

people.StartTracking();
people.Add(new Person { Name = "Three" }, select: true);

people.Undo(); // removes "Three"
people.Redo(); // adds "Three" again
```

## Commands

Tracked objects and collections expose `ICommand` helpers for binding from UI frameworks:

- `ToggleTrackingCommand`
- `UndoChangesCommand`
- `RedoChangesCommand`
- `ApplyChangesCommand`

Tracked collections also expose:

- `RemoveItemCommand`
- `MoveItemCommand`
- `ReplaceItemCommand`
- `ClearItemsCommand`

## Development

Build and test with:

```powershell
dotnet test ChangeTrackingProjects.slnx
```

The repository also includes a small WPF test app:

```powershell
dotnet build rmsoft.ChangeTracking.TestApp\rmsoft.ChangeTracking.TestApp.csproj
```

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE).
