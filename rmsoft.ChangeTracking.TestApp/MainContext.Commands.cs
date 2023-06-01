using System;
using System.Linq;

namespace rmsoft.ChangeTracking.TestApp
{
    public partial class MainContext
    {
        public class GetDataCommandImpl : ContextCommandBase<MainContext>
        {
            public GetDataCommandImpl(MainContext context)
                : base(context)
            {
            }

            public override bool CanExecute(object? parameter)
            {
                return Context.CanExecuteGetDataCommand(parameter);
            }

            public override void Execute(object? parameter)
            {
                Context.ExecuteGetDataCommand(parameter);
            }
        }

        private bool CanExecuteGetDataCommand(object? parameter)
        {
            return !IsTracking;
        }

        private readonly Random _random = new Random();
        private void ExecuteGetDataCommand(object? parameter)
        {
            _data.Clear();
            int num = _random.Next(5, 20);
            int num1 = 1;
            NamedObject obj;
            for (int i = 0; i < num; i++)
            {
                obj = new NamedObject($"Name{num1}", $"Description{num1}");
                obj.TrackerUpdated += (s, e) => RefreshModelState();
                _data.Add(obj);
                num1++;
            }

            SelectedItem = _data.Last();
        }


        public class AddDataCommandImpl : ContextCommandBase<MainContext>
        {
            public AddDataCommandImpl(MainContext context) : base(context)
            {
            }

            public override bool CanExecute(object parameter)
            {
                return Context.CanExecuteAddDataCommand(parameter);
            }

            public override void Execute(object parameter)
            {
                Context.ExecuteAddDataCommand(parameter);
            }
        }

        private bool CanExecuteAddDataCommand(object parameter)
        {
            return _dataChangeTracker.IsTracking;
        }

        private void ExecuteAddDataCommand(object parameter)
        {
            int num = _data.Count + 1;
            NamedObject obj = new NamedObject($"Name{num}", $"Description{num}");
            if (IsAppendData)
                _data.Add(obj);
            else
                _data.Insert(0, obj);
            SelectedItem = obj;
        }

        public class RemoveDataCommandImpl : ContextCommandBase<MainContext>
        {
            public RemoveDataCommandImpl(MainContext context) : base(context)
            {
            }

            public override bool CanExecute(object parameter)
            {
                return Context.CanExecuteRemoveDataCommand(parameter);
            }

            public override void Execute(object parameter)
            {
                Context.ExecuteRemoveDataCommand(parameter);
            }
        }

        private bool CanExecuteRemoveDataCommand(object parameter)
        {
            return _dataChangeTracker.IsTracking
                && HasSelectedItem
                && !SelectedItem.HasChanges;
        }

        private void ExecuteRemoveDataCommand(object parameter)
        {
            int index = _data.IndexOf(SelectedItem);
            _data.RemoveAt(index);
            index--;

            SelectedItem = index >= 0 ? _data[index] : null;
        }

        public class ToggleDataEditCommandImpl : ContextCommandBase<MainContext>
        {
            public ToggleDataEditCommandImpl(MainContext context)
                : base(context)
            {
            }

            public override bool CanExecute(object parameter)
            {
                return Context.CanExecuteToggleDataEditCommand(parameter);
            }

            public override void Execute(object parameter)
            {
                Context.ExecuteToggleDataEditCommand(parameter);
            }
        }

        private bool CanExecuteToggleDataEditCommand(object parameter)
        {
            return true;
        }

        private void ExecuteToggleDataEditCommand(object parameter)
        {
            if (_dataChangeTracker.IsTracking)
                _dataChangeTracker.StopTracking(true);
            else
                _dataChangeTracker.StartTracking();

            RefreshModelState();
        }

        public class UndoDataChangesCommandImpl : ContextCommandBase<MainContext>
        {
            public UndoDataChangesCommandImpl(MainContext context)
                : base(context)
            {
            }

            public override bool CanExecute(object parameter)
            {
                return Context.CanExecuteUndoDataChangesCommand(parameter);
            }

            public override void Execute(object parameter)
            {
                Context.ExecuteUndoDataChangesCommand(parameter);
            }
        }

        private bool CanExecuteUndoDataChangesCommand(object parameter)
        {
            return _dataChangeTracker.CanUndo();
        }

        private void ExecuteUndoDataChangesCommand(object parameter)
        {
            _dataChangeTracker.Undo();
            RefreshModelState();
        }

        public class RedoDataChangesCommandImpl : ContextCommandBase<MainContext>
        {
            public RedoDataChangesCommandImpl(MainContext context)
                : base(context)
            {
            }

            public override bool CanExecute(object parameter)
            {
                return Context.CanExecuteRedoDataChangesCommand(parameter);
            }

            public override void Execute(object parameter)
            {
                Context.ExecuteRedoDataChangesCommand(parameter);
            }
        }

        private bool CanExecuteRedoDataChangesCommand(object parameter)
        {
            return _dataChangeTracker.CanRedo();
        }

        private void ExecuteRedoDataChangesCommand(object parameter)
        {
            _dataChangeTracker.Redo();
            RefreshModelState();
        }

        public class ApplyDataChangesCommandImpl : ContextCommandBase<MainContext>
        {
            public ApplyDataChangesCommandImpl(MainContext context)
                : base(context)
            {
            }

            public override bool CanExecute(object parameter)
            {
                return Context.CanExecuteApplyDataChangesCommand(parameter);
            }

            public override void Execute(object parameter)
            {
                Context.ExecuteApplyDataChangesCommand(parameter);
            }
        }

        private bool CanExecuteApplyDataChangesCommand(object parameter)
        {
            return _dataChangeTracker.CanUndo();
        }

        private void ExecuteApplyDataChangesCommand(object parameter)
        {
            _dataChangeTracker.StopTracking(false);
            RefreshModelState();
        }
    }
}
