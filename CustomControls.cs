ousing System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RPC_Integration
{
    internal class LinkedTB : TextBlock
    {
        public string BaseText { get; set; } = "";
        private TextBlock? linkedTextBlock;
        private Action<string>? linkedCallback;
        internal static bool ignoreCallback = false;

        public string Value => base.Text.Replace(BaseText, "");

        public new string Text
        {
            get => Value;
            set
            {
                linkedTextBlock?.SetValue(TextProperty, Value);
                if (!ignoreCallback)
                {
                    linkedCallback?.Invoke(Value);
                }
                base.Text = BaseText + value;
            }
        }

        public string RawText
        {
            get => base.Text;
            set => base.Text = value;
        }

        internal void Link(TextBlock tb) => linkedTextBlock = tb;

        internal void Link(Action<string> callback) => linkedCallback = callback;

        internal void Link(TextBlock tb, Action<string> callback)
        {
            linkedTextBlock = tb;
            linkedCallback = callback;
        }
    }

    public class TextInputButton() : InputButtonBase<string>
    {
        private int? maxLength;
        private int? minLength;

        internal void Link(Func<string> getOriginalValue, Action<string> onConfirm, int? maxLength = null, int? minLength = null)
        {
            base.Link(getOriginalValue, onConfirm);
            this.maxLength = maxLength;
            this.minLength = minLength;
        }

        protected override string? ShowInputDialog(string title, string originalValue)
        {
            // Create a new window for input
            Window inputWindow = new Window
            {
                Title = title,
                Width = 400,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                BorderThickness = new Thickness(1)
            };

            StackPanel stackPanel = new StackPanel { Margin = new Thickness(20) };

            TextBlock promptText = new TextBlock
            {
                Text = "Please enter text",
                FontSize = 14,
                Foreground = Brushes.Black,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBlock errorText = new TextBlock
            {
                Text = $"Input must be between {minLength ?? 1} and {maxLength ?? 255} characters.",
                FontSize = 12,
                Foreground = Brushes.Red,
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox inputBox = new TextBox
            {
                MaxLength = maxLength ?? 255,
                FontSize = 16,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                MinWidth = 350,
                Margin = new Thickness(0, 0, 0, 5),
                Text = originalValue
            };

            TextBlock originalTextDisplay = new TextBlock
            {
                Text = $"Original: {originalValue}",
                FontSize = 12,
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                Margin = new Thickness(0, 0, 0, 5)
            };

            Button okButton = new Button
            {
                Content = "OK",
                IsDefault = true,
                Width = 80,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = new SolidColorBrush(Color.FromRgb(0, 122, 204)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };

            inputBox.TextChanged += (sender, e) =>
            {
                if (inputBox.Text.Length < (minLength ?? 1) || inputBox.Text.Length > (maxLength ?? 255))
                {
                    errorText.Visibility = Visibility.Visible;
                }
                else
                {
                    errorText.Visibility = Visibility.Collapsed;
                }
            };

            okButton.Click += (sender, e) =>
            {
                if (inputBox.Text.Length >= (minLength ?? 1) && inputBox.Text.Length <= (maxLength ?? 255))
                {
                    inputWindow.DialogResult = true;
                    inputWindow.Close();
                }
            };

            stackPanel.Children.Add(promptText);
            stackPanel.Children.Add(errorText);
            stackPanel.Children.Add(inputBox);
            stackPanel.Children.Add(originalTextDisplay);
            stackPanel.Children.Add(okButton);
            inputWindow.Content = stackPanel;

            bool? result = inputWindow.ShowDialog();
            return result == true ? inputBox.Text : null;
        }
    }

    public abstract class InputButtonBase<T> : Button
    {
        public string Title { get; set; } = "Input";
        public Func<T> GetOriginalValue { get; set; }
        public Action<T> OnConfirm { get; set; }

        internal virtual void Link(Func<T> getOriginalValue, Action<T> onConfirm)
        {
            GetOriginalValue = getOriginalValue;
            OnConfirm = onConfirm;
        }

        protected override void OnClick()
        {
            base.OnClick();
            T? input = ShowInputDialog(Title, GetOriginalValue());
            if (input != null)
            {
                OnConfirm(input);
            }
        }

        // Method that derived classes will implement to provide specific input types.
        protected abstract T? ShowInputDialog(string title, T originalValue);
    }

    public class NumericInputButton() : InputButtonBase<int>
    {
        private int? minValue;
        private int? maxValue;

        internal delegate (int, int) UpdateRange();
        internal UpdateRange? Updater;

        internal void Link(Func<int> getOriginalValue, Action<int> onConfirm, int? maxLength = null, int? minLength = null, UpdateRange? update = null)
        {
            Updater = update;
            base.Link(getOriginalValue, onConfirm);
            this.minValue = minLength;
            this.maxValue = maxLength;
        }

        protected override int ShowInputDialog(string title, int originalValue)
        {
            if (Updater != null)
            {
                (int a, int b) = Updater();
                maxValue = a;
                minValue = b;
            }
            Window inputWindow = new Window
            {
                Title = title,
                Width = 400,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                BorderThickness = new Thickness(1)
            };

            StackPanel stackPanel = new StackPanel { Margin = new Thickness(20) };

            TextBlock promptText = new TextBlock
            {
                Text = $"Please enter a number between {minValue} and {maxValue}",
                FontSize = 14,
                Foreground = Brushes.Black,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBlock errorText = new TextBlock
            {
                Text = $"Input must be between {minValue} and {maxValue}.",
                FontSize = 12,
                Foreground = Brushes.Red,
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox inputBox = new TextBox
            {
                Text = originalValue.ToString(),
                FontSize = 16,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                MinWidth = 350,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBlock originalTextDisplay = new TextBlock
            {
                Text = $"Original: {originalValue}",
                FontSize = 12,
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                Margin = new Thickness(0, 0, 0, 5)
            };

            Button okButton = new Button
            {
                Content = "OK",
                IsDefault = true,
                Width = 80,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = new SolidColorBrush(Color.FromRgb(0, 122, 204)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };

            inputBox.TextChanged += (sender, e) =>
            {
                if (int.TryParse(inputBox.Text, out int result))
                {
                    if (result < (minValue ?? int.MinValue) || result > (maxValue ?? int.MaxValue))
                    {
                        errorText.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        errorText.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    errorText.Visibility = Visibility.Visible;
                }
            };

            okButton.Click += (sender, e) =>
            {
                if (int.TryParse(inputBox.Text, out int result) && result >= (minValue ?? int.MinValue) && result <= (maxValue ?? int.MaxValue))
                {
                    inputWindow.DialogResult = true;
                    inputWindow.Close();
                }
            };

            stackPanel.Children.Add(promptText);
            stackPanel.Children.Add(errorText);
            stackPanel.Children.Add(inputBox);
            stackPanel.Children.Add(originalTextDisplay);
            stackPanel.Children.Add(okButton);
            inputWindow.Content = stackPanel;

            bool? dialogResult = inputWindow.ShowDialog();
            return dialogResult == true && int.TryParse(inputBox.Text, out int finalResult) ? finalResult : 0;
        }
    }

    public class SelectionInputButton() : InputButtonBase<string>
    {
        private string[] choices = [];

        internal void Link(Func<string> getOriginalValue, Action<string> onConfirm, string[] choices)
        {
            base.Link(getOriginalValue, onConfirm);
            this.choices = choices;
        }

        protected override string? ShowInputDialog(string title, string originalValue)
        {
            // Create a new window for input
            Window inputWindow = new Window
            {
                Title = title,
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                BorderThickness = new Thickness(1)
            };

            StackPanel stackPanel = new StackPanel { Margin = new Thickness(20) };

            ComboBox selectionBox = new ComboBox
            {
                ItemsSource = choices,
                SelectedItem = originalValue,
                FontSize = 16,
                MinWidth = 350,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBlock originalTextDisplay = new TextBlock
            {
                Text = $"Original: {originalValue}",
                FontSize = 12,
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                Margin = new Thickness(0, 0, 0, 5)
            };

            Button okButton = new Button
            {
                Content = "OK",
                IsDefault = true,
                Width = 80,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = new SolidColorBrush(Color.FromRgb(0, 122, 204)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };

            okButton.Click += (sender, e) =>
            {
                if (selectionBox.SelectedItem != null)
                {
                    inputWindow.DialogResult = true;
                    inputWindow.Close();
                }
            };

            stackPanel.Children.Add(selectionBox);
            stackPanel.Children.Add(originalTextDisplay);
            stackPanel.Children.Add(okButton);
            inputWindow.Content = stackPanel;

            bool? result = inputWindow.ShowDialog();
            return result == true ? (string)selectionBox.SelectedItem : null;
        }
    }

}
