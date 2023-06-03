using System;

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
