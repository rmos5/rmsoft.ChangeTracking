using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace rmsoft.ChangeTracking
{
    /// <summary>
    /// Represents a list that raises property and collection change notifications and supports selection-aware mutations.
    /// </summary>
    /// <typeparam name="T">The type of items in the list.</typeparam>
    public interface INotifyListChanged<T> : IList<T>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        /// <summary>
        /// Adds an item and optionally selects it.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="select"><see langword="true" /> to select the added item; otherwise, <see langword="false" />.</param>
        void Add(T item, bool select);

        /// <summary>
        /// Inserts an item at the specified index and optionally selects it.
        /// </summary>
        /// <param name="index">The zero-based index at which to insert the item.</param>
        /// <param name="item">The item to insert.</param>
        /// <param name="select"><see langword="true" /> to select the inserted item; otherwise, <see langword="false" />.</param>
        void Insert(int index, T item, bool select);

        /// <summary>
        /// Removes an item and optionally selects a neighboring item.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="select"><see langword="true" /> to update selection after removal; otherwise, <see langword="false" />.</param>
        void Remove(T item, bool select);

        /// <summary>
        /// Moves an item and optionally keeps it selected.
        /// </summary>
        /// <param name="oldIndex">The current zero-based item index.</param>
        /// <param name="newIndex">The destination zero-based item index.</param>
        /// <param name="select"><see langword="true" /> to select the moved item; otherwise, <see langword="false" />.</param>
        void Move(int oldIndex, int newIndex, bool select);

        /// <summary>
        /// Replaces an item at the specified index and optionally selects the replacement.
        /// </summary>
        /// <param name="index">The zero-based index of the item to replace.</param>
        /// <param name="item">The replacement item.</param>
        /// <param name="select"><see langword="true" /> to select the replacement item; otherwise, <see langword="false" />.</param>
        void ReplaceAt(int index, T item, bool select);
    }

    /// <summary>
    /// Represents a tracked collection with a selected item.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    public interface ITrackedCollection<T> : ITrackedObject, INotifyListChanged<T>
    {
        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        T? SelectedItem { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="SelectedItem" /> is not <see langword="null" />.
        /// </summary>
        bool HasSelectedItem { get; }
    }
}
