using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace rmsoft.ChangeTracking.TestApp
{
    public partial class MainContext : TrackedObjectBase, IEnumerable<NamedObject>, INotifyCollectionChanged
    {
        public class ObservableList<T> : ObservableCollection<T>, INotifyListChanged<T>
        {
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        private readonly INotifyListChanged<NamedObject> _data;

        private readonly ListChangeTracker<NamedObject> _dataChangeTracker;

        private NamedObject? selectedItem;

        public NamedObject? SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem != value)
                {
                    selectedItem = value;
                    RefreshModelState();
                }
            }
        }

        private bool isAppendData;

        public bool IsAppendData
        {
            get => isAppendData;
            set 
            { 
                if (value != isAppendData)
                {
                    isAppendData = value;
                    OnPropertyChanged(nameof(IsAppendData));
                }
            }
        }


        public override bool IsTracking =>
            base.IsTracking
            || _data.Any(o => o.IsTracking);

        public bool HasSelectedItem => SelectedItem != null;

        public IContextCommand GetDataCommand { get; }

        public IContextCommand ToggleDataEditCommand { get; }

        public IContextCommand AddDataCommand { get; }

        public IContextCommand RemoveDataCommand { get; }

        public IContextCommand UndoDataChangesCommand { get; }

        public IContextCommand RedoDataChangesCommand { get; }

        public IContextCommand ApplyDataChangesCommand { get; }

        public MainContext()
        {
            GetDataCommand = new GetDataCommandImpl(this);
            ToggleDataEditCommand = new ToggleDataEditCommandImpl(this);
            AddDataCommand = new AddDataCommandImpl(this);
            RemoveDataCommand = new RemoveDataCommandImpl(this);
            UndoDataChangesCommand = new UndoDataChangesCommandImpl(this);
            RedoDataChangesCommand = new RedoDataChangesCommandImpl(this);
            ApplyDataChangesCommand = new ApplyDataChangesCommandImpl(this);

            _data = new ObservableList<NamedObject>();
            _dataChangeTracker = new ListChangeTracker<NamedObject>(_data);
            _dataChangeTracker.TrackerUpdated += _dataChangeTracker_TrackerUpdated;
            _data.CollectionChanged += _data_CollectionChanged;
        }

        ~MainContext()
        {
            _dataChangeTracker.TrackerUpdated -= _dataChangeTracker_TrackerUpdated;
            _data.CollectionChanged -= _data_CollectionChanged;
        }

        private void _dataChangeTracker_TrackerUpdated(object? sender, System.EventArgs e)
        {
        }

        private void _data_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshModelState();
            NotifyCollectionChangedEventHandler? h = CollectionChanged;
            h?.Invoke(this, e);
        }

        public override void RefreshModelState()
        {
            base.RefreshModelState();
            OnPropertyChanged(nameof(SelectedItem));
            OnPropertyChanged(nameof(HasSelectedItem));

            GetDataCommand.RaiseCanExecuteChanged();
            ToggleEditingCommand.RaiseCanExecuteChanged();
            AddDataCommand.RaiseCanExecuteChanged();
            RemoveDataCommand.RaiseCanExecuteChanged();
            UndoDataChangesCommand.RaiseCanExecuteChanged();
            RedoDataChangesCommand.RaiseCanExecuteChanged();
            ApplyDataChangesCommand.RaiseCanExecuteChanged();
        }

        public IEnumerator<NamedObject> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
