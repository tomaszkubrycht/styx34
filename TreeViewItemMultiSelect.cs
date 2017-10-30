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
using System.Reflection;

namespace Styx
{
    public class TreeViewItemMultiSelect : TreeViewItem
    {
        /// <summary>
        /// The parent tree view control
        /// </summary>
        private TreeViewMultiSelect parentTreeView;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentTreeView"></param>
        public TreeViewItemMultiSelect(TreeViewMultiSelect parentTreeView)
        {
            this.parentTreeView = parentTreeView;
        }

        /// <summary>
        /// Generates controls that hold data in the treeview
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeViewItemMultiSelect(parentTreeView);
        }

        /// <summary>
        /// Handles when the mouse down is clicked
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (this.IsSelected && ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
            {
                this.IsSelected = false;
            }
            else if (!IsSelected && IsFocused)
            {
                this.IsSelected = true;

                // This overcomes a corner case where the parent tree list isn't updated
                parentTreeView.AddTreeViewItemToSelection(this);
            }
            base.OnMouseLeftButtonDown(e);
        }
    }
}
