namespace rmsoft.ChangeTracking
{
    public partial class TrackedObservableCollection<T>
    {
        public abstract class TrackedCollectionCommandBase : ContextCommandBase<TrackedObservableCollection<T>>
        {
            protected TrackedCollectionCommandBase(TrackedObservableCollection<T> context) : base(context)
            {
            }

            public override bool CanExecute(object? parameter)
            {
                return Context.CanExecuteCollectionModifyCommand(parameter);
            }
        }

        protected virtual bool CanExecuteCollectionModifyCommand(object? parameter)
        {
            return IsTracking;
        }

        public class RemoveItemCommandImpl : TrackedCollectionCommandBase
        {
            public RemoveItemCommandImpl(TrackedObservableCollection<T> context)
                : base(context)
            {
            }

            public override bool CanExecute(object? parameter)
            {
                return base.CanExecute(parameter)
                    && Context.CanExecuteRemoveItemCommand(parameter);
            }

            public override void Execute(object? parameter)
            {
                Context.ExecuteRemoveItemCommand(parameter);
            }
        }

        protected virtual bool CanExecuteRemoveItemCommand(object? parameter)
        {
            return Count > 0
                   && HasSelectedItem;
        }

        protected virtual void ExecuteRemoveItemCommand(object? parameter)
        {
            if (SelectedItem is T selectedItem)
                Remove(selectedItem, true);
        }

        public class MoveItemCommandImpl : TrackedCollectionCommandBase
        {
            public MoveItemCommandImpl(TrackedObservableCollection<T> context)
                : base(context)
            {
            }

            public override bool CanExecute(object? parameter)
            {
                return base.CanExecute(parameter)
                    && Context.CanExecuteMoveItemCommand(parameter);
            }

            public override void Execute(object? parameter)
            {
                Context.ExecuteMoveItemCommand(parameter);
            }
        }

        protected virtual bool CanExecuteMoveItemCommand(object? parameter)
        {
            return TryGetIndex(parameter, out int index)
                && index >= 0
                && index < Count
                && Count > 0
                && HasSelectedItem
                && IndexOf(SelectedItem!) != index;
        }

        protected virtual void ExecuteMoveItemCommand(object? parameter)
        {
            if (!TryGetIndex(parameter, out int index) || SelectedItem is not T selectedItem)
                return;

            int idx = IndexOf(selectedItem);
            if (idx >= 0 && index >= 0 && index < Count)
                Move(idx, index, true);
        }

        public class ReplaceItemCommandImpl : TrackedCollectionCommandBase
        {
            public ReplaceItemCommandImpl(TrackedObservableCollection<T> context)
                : base(context)
            {
            }

            public override bool CanExecute(object? parameter)
            {
                return base.CanExecute(parameter)
                    && Context.CanExecuteReplaceItemCommand(parameter);
            }

            public override void Execute(object? parameter)
            {
                Context.ExecuteReplaceItemCommand(parameter);
            }
        }

        protected virtual bool CanExecuteReplaceItemCommand(object? parameter)
        {
            return TryGetIndex(parameter, out int index)
                && index >= 0
                && index < Count
                && Count > 0
                && HasSelectedItem
                && IndexOf(SelectedItem!) != index;
        }

        protected virtual void ExecuteReplaceItemCommand(object? parameter)
        {
            if (!TryGetIndex(parameter, out int index) || SelectedItem is not T selectedItem)
                return;

            ReplaceAt(index, selectedItem, true);
        }

        public class ClearItemsCommandImpl : TrackedCollectionCommandBase
        {
            public ClearItemsCommandImpl(TrackedObservableCollection<T> context)
                : base(context)
            {
            }

            public override bool CanExecute(object? parameter)
            {
                return base.CanExecute(parameter)
                    && Context.CanExecuteClearItemsCommand(parameter);
            }

            public override void Execute(object? parameter)
            {
                Context.ExecuteClearItemsCommand(parameter);
            }
        }

        protected virtual bool CanExecuteClearItemsCommand(object? parameter)
        {
            return Count > 0;
        }

        private void ExecuteClearItemsCommand(object? parameter)
        {
            Clear();
        }

        private static bool TryGetIndex(object? parameter, out int index)
        {
            switch (parameter)
            {
                case int i:
                    index = i;
                    return true;
                case string s:
                    return int.TryParse(s, out index);
                default:
                    index = -1;
                    return false;
            }
        }
    }
}
