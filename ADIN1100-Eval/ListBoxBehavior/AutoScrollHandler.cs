// <copyright file="AutoScrollHandler.cs" company="Ivan Derevianko">
//     Copyright 2013 Ivan Derevianko
//     Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
//     to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
//     sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//     The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
//     WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Data;

namespace ADIN1100_Eval.ListBoxBehavior
{
    /// <summary>
    /// Handle auto scroll functionality
    /// </summary>
    public class AutoScrollHandler : DependencyObject, IDisposable
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource",
            typeof(IEnumerable),
            typeof(AutoScrollHandler),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None,
                ItemsSourcePropertyChanged));

        private System.Windows.Controls.ListBox target;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoScrollHandler"/> class.
        /// </summary>
        /// <param name="target">Listbox</param>
        public AutoScrollHandler(System.Windows.Controls.ListBox target)
        {
            this.target = target;
            var binding = new Binding("ItemsSource") { Source = this.target };
            BindingOperations.SetBinding(this, ItemsSourceProperty, binding);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            BindingOperations.ClearBinding(this, ItemsSourceProperty);
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        private static void ItemsSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((AutoScrollHandler)o).ItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
        }

        private void ItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            var collection = oldValue as INotifyCollectionChanged;
            if (collection != null)
            {
                collection.CollectionChanged -= this.CollectionChangedEventHandler;
            }

            collection = newValue as INotifyCollectionChanged;
            if (collection != null)
            {
                collection.CollectionChanged += this.CollectionChangedEventHandler;
            }
        }

        private void CollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add || e.NewItems == null || e.NewItems.Count < 1)
            {
                return;
            }

            this.target.ScrollIntoView(e.NewItems[e.NewItems.Count - 1]);
        }
    }
}