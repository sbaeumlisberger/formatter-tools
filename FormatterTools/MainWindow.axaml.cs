using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FormatterTools
{
    public class MainWindow : Window
    {
        private FormatterTextBox textBox;
        private CheckBox textWrappingCheckbox;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            textBox = this.Find<FormatterTextBox>("textBox");
            textWrappingCheckbox = this.Find<CheckBox>("textWrappingCheckbox");
        }

        private void FormatButton_Click(object sender, RoutedEventArgs e)
        {
            textBox.Format();
        }

        private void WrappingItem_Click(object sender, RoutedEventArgs e)
        {
            textBox.TextWrapping = (textBox.TextWrapping == TextWrapping.NoWrap) ? TextWrapping.Wrap : TextWrapping.NoWrap;
            textWrappingCheckbox.IsChecked = textBox.TextWrapping == TextWrapping.Wrap;
        }
    }
}
