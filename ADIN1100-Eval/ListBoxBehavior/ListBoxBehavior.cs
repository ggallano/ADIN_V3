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

using System.Windows;

namespace ADIN1100_Eval.ListBoxBehavior
{
    /// <summary>
    /// ListBox AutoScroll attached properties
    /// </summary>
    public static class ListBoxBehavior
    {
        public static readonly DependencyProperty AutoScrollProperty = DependencyProperty.RegisterAttached(
            "AutoScroll",
            typeof(bool),
            typeof(System.Windows.Controls.ListBox),
            new PropertyMetadata(false));

        public static readonly DependencyProperty AutoScrollHandlerProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollHandler",
                typeof(AutoScrollHandler),
                typeof(System.Windows.Controls.ListBox));

        public static bool GetAutoScroll(System.Windows.Controls.ListBox instance)
        {
            return (bool)instance.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScroll(System.Windows.Controls.ListBox instance, bool value)
        {
            AutoScrollHandler OldHandler = (AutoScrollHandler)instance.GetValue(AutoScrollHandlerProperty);
            if (OldHandler != null)
            {
                OldHandler.Dispose();
                instance.SetValue(AutoScrollHandlerProperty, null);
            }

            instance.SetValue(AutoScrollProperty, value);
            if (value)
            {
                instance.SetValue(AutoScrollHandlerProperty, new AutoScrollHandler(instance));
            }
        }
    }
}