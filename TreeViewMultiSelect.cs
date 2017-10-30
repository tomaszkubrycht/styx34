/*
Copyright (c) 2007, John Stewien

All rights reserved. Modified BSD License (removed some restrictions):

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

Redistributions of source code must retain the above copyright notice, this list
of conditions and the following disclaimer.

Redistributions in binary form are unrestricted.

Neither the name of the project nor the names of its contributors may be
used to endorse or promote products derived from this software without specific
prior written permission. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Collections.Specialized;
using System.Collections;

namespace Styx
{
    /// <summary>
    /// This is a TreeViewMultiSelect class I wrote. I have in my head an idea
    /// for a better one that is based on a ListView, but I haven't had time to
    /// write it. The idea for the ListView involves having a sub list of all
    /// the tree structure passed in. Items are added and removed as nodes are
    /// opened and closed. The sub list wraps the original objects with an object
    /// that has an indentation property and the item styling indents according
    /// to this property.
    /// </summary>
    public class TreeViewMultiSelect : TreeView
    {
        // ********************************************************************
        // Private Fields
        // ********************************************************************
        #region Private Fields

        /// <summary>
        /// Used for selecting a node by item
        /// </summary>
        private ObservableCollection<object> itemToSelectByPath = null;
        private ItemsControl lastItemToSelectControl = null;
        /// <summary>
        /// The previous item selected in multi-select
        /// </summary>
        private TreeViewItemMultiSelect previousItem = null;
        /// <summary>
        /// The collection of selected TreeViewItems
        /// </summary>
        private ObservableCollection<TreeViewItemMultiSelect> selectedTreeViewItems;
        private bool updatingFromSelectedTreeViewItems = false;
        /// <summary>
        /// The collection of selected data items
        /// </summary>
        private ObservableCollection<object> selectedItems;
        private bool updatingFromSelectedItems = false;
        /// <summary>
        /// Reflection property for setting a private property
        /// </summary>
        private PropertyInfo isSelectionChangeActiveProperty;

        private Dictionary<object, TreeViewItemMultiSelect> itemToTreeViewItem;

        #endregion Private Fields

        // ********************************************************************
        // Public Methods
        // ********************************************************************
        #region Public Methods

        /// <summary>
        /// Constructor. Initializes class fields.
        /// </summary>
        public TreeViewMultiSelect()
        {
            itemToSelectByPath = new ObservableCollection<object>();

            isSelectionChangeActiveProperty = typeof(TreeView).GetProperty("IsSelectionChangeActive", BindingFlags.NonPublic | BindingFlags.Instance);

            // Set up collections
            itemToTreeViewItem = new Dictionary<object, TreeViewItemMultiSelect>();

            selectedItems = new ObservableCollection<object>();
            selectedTreeViewItems = new ObservableCollection<TreeViewItemMultiSelect>();

            // Hook into events

            selectedItems.CollectionChanged += new NotifyCollectionChangedEventHandler(selectedItems_CollectionChanged);
            selectedTreeViewItems.CollectionChanged += new NotifyCollectionChangedEventHandler(selectedTreeViewItems_CollectionChanged);
        }

        /// <summary>
        /// Not implemented yet.
        /// Better to use select item by path-chain, but this can be used to
        /// select a data item. Does it really slowly, scan the data for the item,
        /// gets the tree node chain to it, and selects it.
        /// </summary>
        /// <param name="userItemToSelect"></param>
        public void SelectItem(object userItemToSelect)
        {
            if (ItemsSource != null)
            {
                ArrayList userItemPathToSelect = new ArrayList();
                Stack<IEnumerator> enumeratorStack = new Stack<IEnumerator>();
                IEnumerator currentEnumerator = ItemsSource.GetEnumerator();

                // TODO work out how get the next collection down from the binding
                //NotFinished:
                //goto NotFinished;

                if (userItemPathToSelect.Count > 0)
                    SelectItemByPath(userItemPathToSelect);
            }
        }

        /// <summary>
        /// Removes the item passed in to from the selected items
        /// </summary>
        /// <param name="userItemToDeselect"></param>
        public void DeselectItem(object itemToDeselect)
        {
            if (itemToTreeViewItem.ContainsKey(itemToDeselect))
            {
                TreeViewItemMultiSelect treeViewItem = itemToTreeViewItem[itemToDeselect];
                treeViewItem.IsSelected = false;
                if (selectedTreeViewItems.Contains(treeViewItem))
                {
                    selectedTreeViewItems.Remove(treeViewItem);
                }
            }
        }

        public void SelectItemByPath(ArrayList userItemToSelectByPath)
        {
            itemToSelectByPath.Clear();
            foreach (object item in userItemToSelectByPath)
            {
                itemToSelectByPath.Add(item);
            }
            lastItemToSelectControl = this;
            if (!SelectItemByPath(ref lastItemToSelectControl, itemToSelectByPath))
                base.ItemContainerGenerator.StatusChanged += new EventHandler(ItemContainerGenerator_StatusChanged);
        }

        /// <summary>
        /// Selects the first node in the tree view returns true if the
        /// selection was successful or false if this needs to be called
        /// again when the base.ItemContainerGenerator.StatusChanged event
        /// is fired. Note if false is returned the same items control (which
        /// will be modified as it'a a ref) needs to be passed in next time
        /// to complete the selection.
        /// </summary>
        /// <returns></returns>
        internal static bool SelectItemByPath(ref ItemsControl itemsControl, IList itemToSelectByPath)
        {
            IList source = itemsControl.ItemsSource as IList;

            if (source == null || itemToSelectByPath.Count < 1)
            {
                itemToSelectByPath.Clear();
                return true;
            }
            if (!source.Contains(itemToSelectByPath[0]))
            {
                itemToSelectByPath.Clear();
                return true;
            }

            if (itemsControl.HasItems)
            {
                // Check that the items container generator has been started.
                // If so then just select the item, if not then return false;

                TreeViewItem item = itemsControl.ItemContainerGenerator.ContainerFromItem(itemToSelectByPath[0]) as TreeViewItem;
                if (item != null)
                {
                    if (itemToSelectByPath.Count == 1)
                    {
                        itemToSelectByPath.RemoveAt(0);
                        item.IsSelected = true;
                        item.BringIntoView();
                        return true;
                    }
                    else
                    {
                        itemToSelectByPath.RemoveAt(0);
                        item.IsExpanded = true;
                        itemsControl = item;
                        return SelectItemByPath(ref itemsControl, itemToSelectByPath);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Adds a tree view item to the multi-selection without going through
        /// the normal event chain. Used only by TreeViewItemMultiSelect.
        /// </summary>
        /// <param name="newItem"></param>
        internal void AddTreeViewItemToSelection(TreeViewItemMultiSelect newItem)
        {
            if (!selectedTreeViewItems.Contains(newItem))
            {
                selectedTreeViewItems.Add(newItem);
            }
            previousItem = newItem;
        }

        #endregion Public Methods

        // ********************************************************************
        // Protected Methods
        // ********************************************************************
        #region Protected Methods

        /// <summary>
        /// Gets an empty container for puting in the tree.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeViewItemMultiSelect(this);
        }

        /// <summary>
        /// Handles when the selected item changes. Handles the multi-select functionality.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItemMultiSelect newItem = (TreeViewItemMultiSelect)typeof(TreeView).GetField("_selectedContainer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue((TreeView)this);

            // Turn off updating from TreeViewItems
            isSelectionChangeActiveProperty.SetValue((TreeView)this, true, null);
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // We are either :
                // a ) Adding a new item so newItem!=null & !selectedTreeViewItems.Contains(newItem)
                // b ) Removing the last selection so newItem==null;
                // c ) Removing a previous selection selectedTreeViewItems.Contains(newItem)


                if (newItem != null && !selectedTreeViewItems.Contains(newItem))
                {
                    // Adding a new item
                    if (previousItem != null)
                    {
                        previousItem.IsSelected = true;
                    }
                    selectedTreeViewItems.Add(newItem);
                    previousItem = newItem;
                }
                else if (newItem == null && previousItem != null)
                {
                    // Removing the last selected item

                    selectedTreeViewItems.Remove(previousItem);
                    previousItem = null;
                }
                else if (selectedTreeViewItems.Contains(newItem))
                {
                    // Removing a previously selected item

                    newItem.IsSelected = false;
                    selectedTreeViewItems.Remove(newItem);

                    // The selection needs to be put back on the last selected item
                    if (previousItem != null)
                    {
                        previousItem.IsSelected = true;
                    }
                    previousItem = null;
                }
            }
            else
            {
                while (selectedTreeViewItems.Count > 0)
                {
                    TreeViewItemMultiSelect firstItem = selectedTreeViewItems[0];
                    if (firstItem.IsSelected && firstItem != newItem)
                    {
                        firstItem.IsSelected = false;
                    }
                    selectedTreeViewItems.RemoveAt(0);
                }
                if (newItem != null)
                {
                    selectedTreeViewItems.Add(newItem);
                }

                previousItem = newItem;
            }
            // Turn back on updating from TreeViewItems
            isSelectionChangeActiveProperty.SetValue((TreeView)this, false, null);

            // when the newItem is null that's when the last item was deselected.
            base.OnSelectedItemChanged(e);
        }

        #endregion Protected Methods

        // ********************************************************************
        // Properties
        // ********************************************************************
        #region Properties

        /// <summary>
        /// Gets the collection of selected items
        /// </summary>
        public ObservableCollection<object> SelectedItems
        {
            get
            {
                return selectedItems;
            }
        }

        /// <summary>
        /// Gets the collection of selected items
        /// </summary>
        public ObservableCollection<TreeViewItemMultiSelect> SelectedTreeViewItems
        {
            get
            {
                return selectedTreeViewItems;
            }
        }

        #endregion Properties

        // ********************************************************************
        // Events And Event Handlers
        // ********************************************************************

        /// <summary>
        /// Handles when a node gets expanded when we are waiting to select
        /// a lower node.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            lastItemToSelectControl.ItemContainerGenerator.StatusChanged -=
                new EventHandler(ItemContainerGenerator_StatusChanged);
            if (!SelectItemByPath(ref lastItemToSelectControl, itemToSelectByPath))
                lastItemToSelectControl.ItemContainerGenerator.StatusChanged +=
                    new EventHandler(ItemContainerGenerator_StatusChanged);
        }

        /// <summary>
        /// Handles when the selected tree view items collections changes. Updates
        /// the data item collection accordingly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void selectedTreeViewItems_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            updatingFromSelectedTreeViewItems = true;

            List<object> userItems = new List<object>();

            List<TreeViewItemMultiSelect> treeViewitems =
                new List<TreeViewItemMultiSelect>(selectedTreeViewItems);

            foreach (TreeViewItemMultiSelect treeViewItem in treeViewitems)
            {
                if (treeViewItem.Header != null)
                {
                    if (!updatingFromSelectedItems)
                    {
                        userItems.Add(treeViewItem.Header);
                        if (!selectedItems.Contains(treeViewItem.Header))
                        {
                            selectedItems.Add(treeViewItem.Header);
                            itemToTreeViewItem.Add(treeViewItem.Header, treeViewItem);
                        }
                    }
                }
                else
                {
                    selectedTreeViewItems.Remove(treeViewItem);
                }
            }

            if (!updatingFromSelectedItems)
            {
                List<object> testItems = new List<object>(selectedItems);
                foreach (object item in testItems)
                {
                    if (!userItems.Contains(item))
                    {
                        selectedItems.Remove(item);
                    }
                }
            }

            updatingFromSelectedTreeViewItems = false;
        }

        void selectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            updatingFromSelectedItems = true;
            if (!updatingFromSelectedTreeViewItems)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (object item in e.NewItems)
                        {
                            SelectItem(item);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (object item in e.OldItems)
                        {
                            DeselectItem(item);

                        }
                        break;
                    default:
                        break;
                }
            }

            // Not real happy with this solution.
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    foreach (object item in e.OldItems)
                    {
                        itemToTreeViewItem.Remove(item);
                    }
                    break;
                default:
                    break;
            }
            updatingFromSelectedItems = false;
        }
    }
}
