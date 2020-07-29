using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FormatterTools
{
    /// <summary>
    /// Textbox which formats text on paste.
    /// </summary>
    public class FormatterTextBox : TextBox, IStyleable
    {
        Type IStyleable.StyleKey => typeof(TextBox);

        public void Format()
        {
            string text = FormatText(Text);
            SelectionStart = 0;
            SelectionEnd = Text?.Length ?? 0;
            CaretIndex = SelectionEnd;
            InsertText(text);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            var keymap = AvaloniaLocator.Current.GetService<PlatformHotkeyConfiguration>();

            bool Match(List<KeyGesture> gestures) => gestures.Any(g => g.Matches(e));

            if (Match(keymap.Paste))
            {
                PasteAsync();
                e.Handled = true;
            }
            else
            {
                base.OnKeyDown(e);
            }
        }

        private async void PasteAsync()
        {
            var clipboard = (IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard));

            var text = await clipboard.GetTextAsync();

            if (text != null)
            {
                text = FormatText(text);
                InsertText(text);
            }
        }

        private void InsertText(string text)
        {
            // see https://github.com/AvaloniaUI/Avalonia/blob/master/src/Avalonia.Controls/TextBox.cs#L432

            object undoRedoHelper = typeof(TextBox).GetField("_undoRedoHelper", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);

            var snapshotMethod = undoRedoHelper.GetType().GetMethod("Snapshot");

            snapshotMethod.Invoke(undoRedoHelper, new object[0]);

            typeof(TextBox).GetMethod("HandleTextInput", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this, new[] { text });

            snapshotMethod.Invoke(undoRedoHelper, new object[0]);
        }

        private string FormatText(string text)
        {
            try
            {
                if (text.StartsWith("<"))
                {
                    text = FormatXml(text);
                }
                else
                {
                    text = FormatJson(text);
                }
            }
            catch
            {
                // ignore - don't change the text
            }
            return text;
        }

        private string FormatJson(string jsonString)
        {
            using var stream = new MemoryStream();
            var document = JsonDocument.Parse(jsonString);
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            document.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private string FormatXml(string xml)
        {
            XDocument doc = XDocument.Parse(xml);
            return doc.ToString();
        }
    }
}
