# rmsoft.ChangeTracking

Lightweight change-tracking helpers for .NET objects and observable collections, including undo/redo support and optional property-level tracking via attributes.

## Projects

- `rmsoft.ChangeTracking/` – core library.
- `rmsoft.ChangeTracking.TestApp/` – sample WPF app demonstrating usage.

## Core concepts

- **Trackable objects**: derive from `TrackedObjectBase` and mark properties with `[PropertyChangeTracker]`.
- **Trackable collections**: use `TrackedObservableCollection<T>`.
- **Change lifecycle**:
  - `StartTracking()`
  - `StopTracking(cancelChanges)` (legacy behavior: stops and clears history)
  - `StopTracking(cancelChanges, clearHistory)` (new overload: optionally preserve history)
- **History operations**:
  - `CanUndo()` / `Undo()`
  - `CanRedo()` / `Redo()`

## Property tracking

`PropertyChangesTracker` listens to `INotifyPropertyChanged` and tracks only properties decorated with `PropertyChangeTrackerAttribute`.

Notes:
- Empty/null `PropertyName` notifications are treated as "all tracked properties changed."
- Tracked property metadata is cached to reduce repeated reflection overhead.

## Quick example

```csharp
public sealed class PersonViewModel : TrackedObjectBase
{
    private string name;

    [PropertyChangeTracker]
    public string Name
    {
        get => name;
        set
        {
            if (name == value) return;
            name = value;
            OnPropertyChanged(nameof(Name));
        }
    }
}

// usage
var vm = new PersonViewModel { IsTrackingEnabled = true };
vm.StartTracking();
vm.Name = "Alice";
vm.Name = "Bob";

if (vm.CanUndo()) vm.Undo();
vm.StopTracking(cancelChanges: false, clearHistory: false);
```

## Build

From repository root:

```bash
dotnet build ChangeTrackingProjects.sln
```

> In restricted environments, installing or running `dotnet` may be unavailable.

## NuGet notes

See `rmsoft.ChangeTracking/readme-nuget.txt` for package CLI snippets.
