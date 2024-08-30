using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ADIN.WPF.Components
{
    /// <summary>
    /// Interaction logic for MACAddress.xaml
    /// </summary>
    public partial class MACAddress : UserControl
    {
        private static readonly List<Key> HexKeys = new List<Key> { Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9,
                                                                      Key.A, Key.B, Key.C, Key.D, Key.E, Key.F,
                                                                      Key.NumPad0, Key.NumPad1, Key.NumPad2, Key.NumPad3, Key.NumPad4, Key.NumPad5, Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9 };

        private static readonly List<Key> RightKey = new List<Key> { Key.Right };
        private static readonly List<Key> LeftKey = new List<Key> { Key.Left };
        private static readonly List<Key> OtherKeysAlsoAllowed = new List<Key> { Key.Tab, Key.Delete };

        private readonly List<TextBox> _parts = new List<TextBox>();

        private bool _isToSupprsMacAdrsUpd = false;

        public MACAddress()
        {
            InitializeComponent();
            _parts.Add(FirstSegment);
            _parts.Add(SecondSegment);
            _parts.Add(ThirdSegment);
            _parts.Add(FourthSegment);
            _parts.Add(FifthSegment);
            _parts.Add(LastSegment);
        }

        public static readonly DependencyProperty MacAdrsProperty = DependencyProperty.Register(
            "MacAdrs", typeof(string), typeof(MACAddress), new FrameworkPropertyMetadata(default(string), MacAdrsChanged)
            {
                BindsTwoWayByDefault = true
            });

        private static void MacAdrsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var macAdrsBox = dependencyObject as MACAddress;
            var text = e.NewValue as string;

            if (text != null && macAdrsBox != null)
            {
                int counter = 0;
                macAdrsBox._isToSupprsMacAdrsUpd = true;
                foreach (var parts in text.Split(':'))
                {
                    macAdrsBox._parts[counter].Text = parts;
                    counter++;
                }
                macAdrsBox._isToSupprsMacAdrsUpd = false;
            }
        }

        public string MacAdrs
        {
            get { return (string)GetValue(MacAdrsProperty); }
            set { SetValue(MacAdrsProperty, value); }
        }

        private void MacAdrsUI_OnPreviewForKeyDown(object sender, KeyEventArgs e)
        {
            if (HexKeys.Contains(e.Key))
            {
                e.Handled = IsNumKeyPressedToCancel();
                NumPressedHandling();
            }
            else if (LeftKey.Contains(e.Key))
            {
                e.Handled = IsLeftKeyPressedToCancel();
                LeftKeyPressedHandling();
            }
            else if (RightKey.Contains(e.Key))
            {
                e.Handled = IsRightKeyPressedToCancel();
                RightKeyPressedHandling();
            }
            else if (e.Key == Key.Back)
            {
                BackspacePressedHandling();
            }
            else if (e.Key == Key.OemPeriod)
            {
                e.Handled = true;
                PeriodKeyPressedHandling();
            }
            else
            {
                e.Handled = !HasPressedOtherAllowedKeys(e);
            }
        }

        private bool IsNumKeyPressedToCancel()
        {
            var currentTextBox = FocusManager.GetFocusedElement(this) as TextBox;
            return currentTextBox != null &&
                   currentTextBox.Text.Length == 2 &&
                   currentTextBox.CaretIndex == 2 &&
                   currentTextBox.SelectedText.Length == 0;
        }

        private void NumPressedHandling()
        {
            var currentTextBox = FocusManager.GetFocusedElement(this) as TextBox;

            if (currentTextBox != null && currentTextBox.Text.Length == 2 &&
                currentTextBox.CaretIndex == 2 && currentTextBox.SelectedText.Length == 0)
            {
                ChangeFocusToNextPart(currentTextBox);
            }
        }

        private bool IsLeftKeyPressedToCancel()
        {
            var currentTextBox = FocusManager.GetFocusedElement(this) as TextBox;
            return currentTextBox != null && currentTextBox.CaretIndex == 0;
        }

        private void LeftKeyPressedHandling()
        {
            var currentTextBox = FocusManager.GetFocusedElement(this) as TextBox;

            if (currentTextBox != null && currentTextBox.CaretIndex == 0)
            {
                ChangeFocusToPrevPart(currentTextBox);
            }
        }

        private bool IsRightKeyPressedToCancel()
        {
            var currentTextBox = FocusManager.GetFocusedElement(this) as TextBox;
            return currentTextBox != null && currentTextBox.CaretIndex == 2;
        }

        private void RightKeyPressedHandling()
        {
            var currentTextBox = FocusManager.GetFocusedElement(this) as TextBox;

            if (currentTextBox != null && currentTextBox.CaretIndex == currentTextBox.Text.Length)
            {
                ChangeFocusToNextPart(currentTextBox);
            }
        }

        private void BackspacePressedHandling()
        {
            var currentTextBox = FocusManager.GetFocusedElement(this) as TextBox;

            if (currentTextBox != null && currentTextBox.CaretIndex == 0 && currentTextBox.SelectedText.Length == 0)
            {
                ChangeFocusToPrevPart(currentTextBox);
            }
        }

        private void PeriodKeyPressedHandling()
        {
            var currentTextBox = FocusManager.GetFocusedElement(this) as TextBox;

            if (currentTextBox != null && currentTextBox.Text.Length > 0 && currentTextBox.CaretIndex == currentTextBox.Text.Length)
            {
                ChangeFocusToNextPart(currentTextBox);
            }
        }

        private bool HasPressedOtherAllowedKeys(KeyEventArgs e)
        {
            if (((e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0 &&
                 (e.Key == Key.A ||
                  e.Key == Key.C ||
                  e.Key == Key.V ||
                  e.Key == Key.X)) ||
                OtherKeysAlsoAllowed.Contains(e.Key))
                return true;
            else
                return false;
        }

        private void OnChangedTextOnTextBox(object sender, TextChangedEventArgs e)
        {
            if (!_isToSupprsMacAdrsUpd)
            {
                MacAdrs = string.Format($"{FirstSegment.Text}:{SecondSegment.Text}:{ThirdSegment.Text}:{FourthSegment.Text}:{FifthSegment.Text}:{LastSegment.Text}");
            }

            var selectedTextBox = FocusManager.GetFocusedElement(this) as TextBox;

            if (selectedTextBox != null && selectedTextBox.Text.Length == 2 && selectedTextBox.CaretIndex == 2)
            {
                ChangeFocusToNextPart(selectedTextBox);
            }
        }

        private void ChangeFocusToPrevPart(TextBox selectedTextBox)
        {
            if (!ReferenceEquals(selectedTextBox, FirstSegment))
            {
                var previousPartIndex = _parts.FindIndex(box => ReferenceEquals(box, selectedTextBox)) - 1;
                selectedTextBox.SelectionLength = 0;
                selectedTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                _parts[previousPartIndex].CaretIndex = _parts[previousPartIndex].Text.Length;
            }
        }

        private void ChangeFocusToNextPart(TextBox selectedTextBox)
        {
            if (!ReferenceEquals(selectedTextBox, LastSegment))
            {
                selectedTextBox.SelectionLength = 0;
                selectedTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void OnPastingDataObject(object sender, DataObjectPastingEventArgs e)
        {
            var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
            if (!isText)
            {
                e.CancelCommand();
                return;
            }

            var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;

            int num;

            if (!int.TryParse(text, out num))
            {
                e.CancelCommand();
            }
        }
    }
}
