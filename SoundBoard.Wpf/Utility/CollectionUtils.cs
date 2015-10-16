namespace SoundBoard.Wpf.Utility
{
    #region Namespaces

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    #endregion

    public static class CollectionUtils
    {
        #region  Private helper functions

        private static bool DefaultCompareFunc<T>(T obj1, T obj2)
        {
            return obj1.Equals(obj2);
        }

        private static T DefaultCreateFunc<T>(T obj)
        {
            return obj;
        }

        private static void MoveItem<T>(IList<T> list, int oldIndex, int newIndex)
        {
            var observableCollection = list as ObservableCollection<T>;
            if (observableCollection != null)
            {
                observableCollection.Move(oldIndex, newIndex);
            }
            else
            {
                var item = list[oldIndex];
                list.RemoveAt(oldIndex);
                list.Insert((oldIndex < newIndex) ? (newIndex - 1) : newIndex, item);
            }
        }

        #endregion

        #region Public methods

        public static bool ScrambledEquals<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var deletedItems = list1.Except(list2).Any();
            var newItems = list2.Except(list1).Any();
            return !newItems && !deletedItems;
        }

        public static bool UpdateFrom<T>(this IList<T> updateList, IEnumerable<T> source)
        {
            return updateList.UpdateFrom<T, T>(source, DefaultCreateFunc, DefaultCompareFunc, null, null, null);
        }

        public static bool UpdateFrom<T>(this IList<T> updateList, IEnumerable<T> source, Action<T> addFunc, Action<T> removeFunc)
        {
            return updateList.UpdateFrom<T, T>(source, DefaultCreateFunc, DefaultCompareFunc, null, addFunc, removeFunc);
        }

        public static bool UpdateFrom<T>(this IList<T> updateList, IEnumerable<T> source, Func<T, T, bool> compareFunc)
        {
            return updateList.UpdateFrom<T, T>(source, DefaultCreateFunc, compareFunc, null, null, null);
        }

        public static bool UpdateFrom<T>(this IList<T> updateList, IEnumerable<T> source, Action<T, T> updateFunc)
        {
            return updateList.UpdateFrom<T, T>(source, DefaultCreateFunc, DefaultCompareFunc, updateFunc, null, null);
        }

        public static bool UpdateFrom<T>(this IList<T> updateList, IEnumerable<T> source, Action<T, T> updateFunc, Func<T, T, bool> compareFunc)
        {
            return updateList.UpdateFrom<T, T>(source, DefaultCreateFunc, compareFunc, updateFunc, null, null);
        }

        public static bool UpdateFrom<TS, TD>(this IList<TD> updateList, IEnumerable<TS> source, Func<TS, TD> createFunc, Func<TS, TD, bool> compareFunc, Action<TS, TD> updateFunc)
        {
            return updateList.UpdateFrom(source, createFunc, compareFunc, updateFunc, null, null);
        }

        public static bool UpdateFrom<TS, TD>(this IList<TD> updateList, IEnumerable<TS> source, Func<TS, TD> createFunc, Func<TS, TD, bool> compareFunc, Action<TS, TD> updateFunc, Action<TD> addFunc, Action<TD> removeFunc)
        {
            // Check for NULL arguments
            if (updateList == null)
                throw new ArgumentNullException("updateList");
            if (source == null)
                throw new ArgumentNullException("source");
            if (createFunc == null)
                throw new ArgumentNullException("createFunc");
            if (compareFunc == null)
                throw new ArgumentNullException("compareFunc");

            // Track updates
            var update = false;

            // Scan through all items in the source array
            var index = 0;
            foreach (var item in source)
            {
                // Determine the number of items in the updated list
                var updatedCount = updateList.Count;

                // If the list also has the item, then we don't need to do anything
                if ((index >= updatedCount) || !compareFunc(item, updateList[index]))
                {
                    var foundItem = false;

                    // Check if the source item exists in the original on another position
                    for (var scanIndex = index + 1; (scanIndex < updatedCount) && !foundItem; ++scanIndex)
                    {
                        if (compareFunc(item, updateList[scanIndex]))
                        {
                            // Move the item
                            if (scanIndex != index)
                            {
                                // Move the item
                                MoveItem(updateList, scanIndex, index);

                                // List is updated
                                update = true;
                            }

                            // Update the object with the source object
                            if (updateFunc != null)
                                updateFunc(item, updateList[index]);

                            // Found item
                            foundItem = true;
                        }
                    }

                    // Insert the item, if it couldn't be found in the list
                    if (!foundItem)
                    {
                        // Create the new item
                        var newItem = createFunc(item);

                        // Add item
                        updateList.Insert(index, newItem);

                        // Add item to the newItems list
                        if (addFunc != null)
                            addFunc(newItem);

                        // List is updated
                        update = true;
                    }
                }
                else if (updateFunc != null)
                {
                    // Update the object with the source object
                    updateFunc(item, updateList[index]);
                }

                // Increment index
                ++index;
            }

            // If source is empty, then we clear the entire list and return
            if (index == 0)
            {
                // Check if the update list does contain items
                if (!updateList.Any())
                    return false;

                // Add all items to the removed item list
                if (removeFunc != null)
                {
                    foreach (var item in updateList)
                        removeFunc(item);
                }

                // Clear the collection and return
                updateList.Clear();
                return true;
            }

            // All remaining items were not in the source, so they should be removed
            while (index < updateList.Count)
            {
                // Add item to the removed item list
                if (removeFunc != null)
                    removeFunc(updateList[index]);

                // Remove item
                updateList.RemoveAt(index);

                // List is updated
                update = true;
            }

            // Return update flag
            return update;
        }

        public static bool UpdateFrom<TS, TK, TD>(this IDictionary<TK, TD> updateDictionary, IEnumerable<TS> source, Func<TS, TK> getKeyFunc, Func<TS, TK, TD> createFunc, Action<TS, TK, TD> updateFunc)
        {
            return updateDictionary.UpdateFrom(source, getKeyFunc, createFunc, updateFunc, null, null);
        }

        public static bool UpdateFrom<TS, TK, TD>(this IDictionary<TK, TD> updateDictionary, IEnumerable<TS> source, Func<TS, TK> getKeyFunc, Func<TS, TK, TD> createFunc, Action<TS, TK, TD> updateFunc, Action<TK, TD> removeFunc)
        {
            return updateDictionary.UpdateFrom(source, getKeyFunc, createFunc, updateFunc, removeFunc, null);
        }

        public static bool UpdateFrom<TS, TK, TD>(this IDictionary<TK, TD> updateDictionary, IEnumerable<TS> source, Func<TS, TK> getKeyFunc, Func<TS, TK, TD> createFunc, Action<TS, TK, TD> updateFunc, Func<TS, TK, TD, bool> compareFunc)
        {
            return updateDictionary.UpdateFrom(source, getKeyFunc, createFunc, updateFunc, null, compareFunc);
        }

        public static bool UpdateFrom<TS, TK, TD>(this IDictionary<TK, TD> updateDictionary, IEnumerable<TS> source, Func<TS, TK> getKeyFunc, Func<TS, TK, TD> createFunc, Action<TS, TK, TD> updateFunc, Action<TK, TD> removeFunc, Func<TS, TK, TD, bool> compareFunc)
        {
            // Check for NULL arguments
            if (updateDictionary == null)
                throw new ArgumentNullException("updateDictionary");
            if (source == null)
                throw new ArgumentNullException("source");
            if (getKeyFunc == null)
                throw new ArgumentNullException("getKeyFunc");
            if (createFunc == null)
                throw new ArgumentNullException("createFunc");

            // Update flag
            var update = false;

            // Create a list of all keys
            var keys = new HashSet<TK>();

            // Add all items in the source array
            foreach (var item in source)
            {
                // Determine the key for this value
                var key = getKeyFunc(item);

                // If the dictionary also has the item, then we don't need to do anything
                TD value;
                if (updateDictionary.TryGetValue(key, out value))
                {
                    // Check if a compare method has been specified
                    if ((compareFunc != null) && !compareFunc(item, key, value))
                    {
                        // Delete the old value
                        if (removeFunc != null)
                            removeFunc(key, value);

                        // Create a new value
                        value = createFunc(item, key);

                        // Replace the value
                        updateDictionary[key] = value;
                    }
                }
                else
                {
                    // Create the new function
                    value = createFunc(item, key);
                    updateDictionary.Add(key, value);

                    // Updated
                    update = true;
                }

                // Add the key
                keys.Add(key);
            }

            // Scan if there are dictionary keys that are not used anymore
            var deleteKeys = new HashSet<TK>();
            foreach (var existingKey in updateDictionary.Keys)
            {
                if (!keys.Contains(existingKey))
                    deleteKeys.Add(existingKey);
            }

            // Remove all keys
            if (deleteKeys.Any())
            {
                foreach (var deleteKey in deleteKeys)
                {
                    if (removeFunc != null)
                        removeFunc(deleteKey, updateDictionary[deleteKey]);
                    updateDictionary.Remove(deleteKey);

                    // Updated
                    update = true;
                }
            }

            // Return update flag
            return update;
        }

        #endregion
    }
}