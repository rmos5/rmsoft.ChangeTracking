namespace rmsoft.ChangeTracking
{
    public partial class TrackedObservableCollection<T>
    {
        /// <summary>
        /// Provides a base implementation for commands that modify a tracked collection.
        /// </summary>
        public abstract class TrackedCollectionCommandBase : ContextCommandBase<TrackedObservableCollection<T>>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TrackedCollectionCommandBase" /> class.
            /// </summary>
            /// <param name="context">The collection controlled by the command.</param>
            protected TrackedCollectionCommandBase(TrackedObservableCollection<T> context) : base(context)
            {
            }

            /// <inheritdoc />
            public override bool CanExecute(object? parameter)
            {
                return Context.CanExecuteCollectionModifyCommand(parameter);
            }
        }

        /// <summary>
        /// Returns whether collection modification commands can execute.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns><see langword="true" /> when modification commands can execute; otherwise, <see langword="false" />.</returns>
        protected virtual bool CanExecuteCollectionModifyCommand(object? parameter)
        {
            return IsTracking;
        }

        /// <summary>
        /// Removes the selected item from a tracked collection.
        /// </summary>
        public class RemoveItemCommandImpl : TrackedCollectionCommandBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RemoveItemCommandImpl" /> class.
            /// </summary>
            /// <param name="context">The collection controlled by the command.</param>
            public RemoveItemCommandImpl(TrackedObservableCollection<T> context)
                : base(context)
            {
            }

            /// <inheritdoc />
            public override bool CanExecute(object? parameter)
            {
                return base.CanExecute(parameter)
                    && Context.CanExecuteRemoveItemCommand(parameter);
            }

            /// <inheritdoc />
            public override void Execute(object? parameter)
            {
                Context.ExecuteRemoveItemCommand(parameter);
            }
        }

        /// <summary>
        /// Returns whether <see cref="RemoveItemCommand" /> can execute.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns><see langword="true" /> when the selected item can be removed; otherwise, <see langword="false" />.</returns>
        protected virtual bool CanExecuteRemoveItemCommand(object? parameter)
        {
            return Count > 0
                   && HasSelectedItem;
        }

        /// <summary>
        /// Executes <see cref="RemoveItemCommand" />.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        protected virtual void ExecuteRemoveItemCommand(object? parameter)
        {
            if (SelectedItem is T selectedItem)
                Remove(selectedItem, true);
        }

        /// <summary>
        /// Moves the selected item to an index supplied as the command parameter.
        /// </summary>
        public class MoveItemCommandImpl : TrackedCollectionCommandBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MoveItemCommandImpl" /> class.
            /// </summary>
            /// <param name="context">The collection controlled by the command.</param>
            public MoveItemCommandImpl(TrackedObservableCollection<T> context)
                : base(context)
            {
            }

            /// <inheritdoc />
            public override bool CanExecute(object? parameter)
            {
                return base.CanExecute(parameter)
                    && Context.CanExecuteMoveItemCommand(parameter);
            }

            /// <inheritdoc />
            public override void Execute(object? parameter)
            {
                Context.ExecuteMoveItemCommand(parameter);
            }
        }

        /// <summary>
        /// Returns whether <see cref="MoveItemCommand" /> can execute.
        /// </summary>
        /// <param name="parameter">The destination index as an <see cref="int" /> or numeric string.</param>
        /// <returns><see langword="true" /> when the selected item can be moved; otherwise, <see langword="false" />.</returns>
        protected virtual bool CanExecuteMoveItemCommand(object? parameter)
        {
            return TryGetIndex(parameter, out int index)
                && index >= 0
                && index < Count
                && Count > 0
                && HasSelectedItem
                && IndexOf(SelectedItem!) != index;
        }

        /// <summary>
        /// Executes <see cref="MoveItemCommand" />.
        /// </summary>
        /// <param name="parameter">The destination index as an <see cref="int" /> or numeric string.</param>
        protected virtual void ExecuteMoveItemCommand(object? parameter)
        {
            if (!TryGetIndex(parameter, out int index) || SelectedItem is not T selectedItem)
                return;

            int idx = IndexOf(selectedItem);
            if (idx >= 0 && index >= 0 && index < Count)
                Move(idx, index, true);
        }

        /// <summary>
        /// Replaces an item at an index supplied as the command parameter with the selected item.
        /// </summary>
        public class ReplaceItemCommandImpl : TrackedCollectionCommandBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ReplaceItemCommandImpl" /> class.
            /// </summary>
            /// <param name="context">The collection controlled by the command.</param>
            public ReplaceItemCommandImpl(TrackedObservableCollection<T> context)
                : base(context)
            {
            }

            /// <inheritdoc />
            public override bool CanExecute(object? parameter)
            {
                return base.CanExecute(parameter)
                    && Context.CanExecuteReplaceItemCommand(parameter);
            }

            /// <inheritdoc />
            public override void Execute(object? parameter)
            {
                Context.ExecuteReplaceItemCommand(parameter);
            }
        }

        /// <summary>
        /// Returns whether <see cref="ReplaceItemCommand" /> can execute.
        /// </summary>
        /// <param name="parameter">The target index as an <see cref="int" /> or numeric string.</param>
        /// <returns><see langword="true" /> when the selected item can replace the target item; otherwise, <see langword="false" />.</returns>
        protected virtual bool CanExecuteReplaceItemCommand(object? parameter)
        {
            return TryGetIndex(parameter, out int index)
                && index >= 0
                && index < Count
                && Count > 0
                && HasSelectedItem
                && IndexOf(SelectedItem!) != index;
        }

        /// <summary>
        /// Executes <see cref="ReplaceItemCommand" />.
        /// </summary>
        /// <param name="parameter">The target index as an <see cref="int" /> or numeric string.</param>
        protected virtual void ExecuteReplaceItemCommand(object? parameter)
        {
            if (!TryGetIndex(parameter, out int index) || SelectedItem is not T selectedItem)
                return;

            ReplaceAt(index, selectedItem, true);
        }

        /// <summary>
        /// Clears a tracked collection.
        /// </summary>
        public class ClearItemsCommandImpl : TrackedCollectionCommandBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClearItemsCommandImpl" /> class.
            /// </summary>
            /// <param name="context">The collection controlled by the command.</param>
            public ClearItemsCommandImpl(TrackedObservableCollection<T> context)
                : base(context)
            {
            }

            /// <inheritdoc />
            public override bool CanExecute(object? parameter)
            {
                return base.CanExecute(parameter)
                    && Context.CanExecuteClearItemsCommand(parameter);
            }

            /// <inheritdoc />
            public override void Execute(object? parameter)
            {
                Context.ExecuteClearItemsCommand(parameter);
            }
        }

        /// <summary>
        /// Returns whether <see cref="ClearItemsCommand" /> can execute.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns><see langword="true" /> when the collection can be cleared; otherwise, <see langword="false" />.</returns>
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
