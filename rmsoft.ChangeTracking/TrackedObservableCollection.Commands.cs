using System;
using System.ComponentModel;

namespace rmsoft.ChangeTracking
{
    public partial class TrackedObservableCollection<T>
    {
        public abstract class TrackedCollectionCommandBase : ContextCommandBase<TrackedObservableCollection<T>>
        {
            protected TrackedCollectionCommandBase(TrackedObservableCollection<T> context) : base(context)
            {
            }

            public override bool CanExecute(object parameter)
            {
                return Context.CanExecuteCollectionModifyCommand(parameter);
            }
        }

        protected virtual bool CanExecuteCollectionModifyCommand(object parameter)
        {
            return IsTracking;
        }

        public class RemoveItemCommandImpl : TrackedCollectionCommandBase
        {
            public RemoveItemCommandImpl(TrackedObservableCollection<T> context)
                : base(context)
            {
            }

            public override bool CanExecute(object parameter)
            {
                return base.CanExecute(parameter)
                    && Context.CanExecuteRemoveItemCommand(parameter);
                
            }

            public override void Execute(object parameter)
            {
                Context.ExecuteRemoveItemCommand(parameter);
            }
        }

        protected virtual bool CanExecuteRemoveItemCommand(object parameter)
        {
            return Count > 0
                   && HasSelectedItem;
        }

        private void ExecuteRemoveItemCommand(object parameter)
        {
            Remove(SelectedItem);            
        }

        public class MoveItemCommandImpl : TrackedCollectionCommandBase
        {
            public MoveItemCommandImpl(TrackedObservableCollection<T> context)
                : base(context)
            {
            }

            public override bool CanExecute(object parameter)
            {
                return base.CanExecute(parameter)
                    && Context.CanExecuteMoveItemCommand(parameter);

            }

            public override void Execute(object parameter)
            {
                Context.ExecuteMoveItemCommand(parameter);
            }
        }

        protected virtual bool CanExecuteMoveItemCommand(object parameter)
        {
            int index = -1;
            if (parameter is string s)
                index = int.Parse(s);
            else if (parameter is int i)
                index = i;

            return Count > 0
                && HasSelectedItem
                && IndexOf(SelectedItem) != index;
        }

        private void ExecuteMoveItemCommand(object parameter)
        {
            int index = -1;
            if (parameter is string s)
                index = int.Parse(s);
            else if (parameter is int i)
                index = i;

            int idx = IndexOf(SelectedItem);
            MoveItem(idx, index);
        }

        public class ReplaceItemCommandImpl : TrackedCollectionCommandBase
        {
            public ReplaceItemCommandImpl(TrackedObservableCollection<T> context)
                : base(context)
            {
            }

            public override bool CanExecute(object parameter)
            {
                return base.CanExecute(parameter)
                    && Context.CanExecuteReplaceItemCommand(parameter);

            }

            public override void Execute(object parameter)
            {
                Context.ExecuteReplaceItemCommand(parameter);
            }
        }

        protected virtual bool CanExecuteReplaceItemCommand(object parameter)
        {
            int index = -1;
            if (parameter is string s)
                index = int.Parse(s);
            else if (parameter is int i)
                index = i;

            return Count > 0
                && HasSelectedItem
                && IndexOf(SelectedItem) != index;
        }

        private void ExecuteReplaceItemCommand(object parameter)
        {
            int index = -1;
            if (parameter is string s)
                index = int.Parse(s);
            else if (parameter is int i)
                index = i;
            
            T item2 = this[index];
            int index2 = IndexOf(SelectedItem);
            MoveItem(index2, index);
            index = IndexOf(item2);
            MoveItem(index, index2);
        }

        public class ClearItemsCommandImpl : TrackedCollectionCommandBase
        {
            public ClearItemsCommandImpl(TrackedObservableCollection<T> context)
                : base(context)
            {
            }

            public override bool CanExecute(object parameter)
            {
                return base.CanExecute(parameter)
                    && Context.CanExecuteClearItemsCommand(parameter);
            }

            public override void Execute(object parameter)
            {
                Context.ExecuteClearItemsCommand(parameter);
            }
        }

        protected virtual bool CanExecuteClearItemsCommand(object parameter)
        {
            return Count > 0;
        }

        private void ExecuteClearItemsCommand(object parameter)
        {
            Clear();
        }
    }
}
