using System.Linq;

namespace rmsoft.ChangeTracking.TestApp
{
    public partial class MainContext : TrackedObservableCollection<NamedObject>
    {
        private bool isInsertData;

        public bool IsInsertData
        {
            get => isInsertData;
            set
            {
                if (value != isInsertData)
                {
                    isInsertData = value;
                    OnPropertyChanged(nameof(IsInsertData));
                }
            }
        }

        public IContextCommand GetDataCommand { get; }

        public IContextCommand AddNewDataCommand { get; }

        public MainContext()
        {
            GetDataCommand = new GetDataCommandImpl(this);
            AddNewDataCommand = new AddNewDataCommandImpl(this);
            IsTrackingEnabled = true;
        }

        public override void RefreshModelState()
        {
            base.RefreshModelState();
            OnPropertyChanged(nameof(SelectedItem));
            OnPropertyChanged(nameof(HasSelectedItem));

            GetDataCommand.RaiseCanExecuteChanged();
            AddNewDataCommand.RaiseCanExecuteChanged();
        }

        protected NamedObject CreateNewData(int number)
        {
            NamedObject result = new NamedObject($"Name{number}", $"Description{number}");
            result.IsTrackingEnabled = true;
            return result;
        }

        protected void AddData(NamedObject item, bool select)
        {
            if (IsInsertData)
                Insert(0, item, select);
            else
                Add(item, select);
        }
    }
}
