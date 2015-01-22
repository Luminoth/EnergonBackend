using System;
using System.Windows.Documents;
using System.Windows.Media;

namespace System.Windows.Controls
{
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            TextRange range = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            range.Text = text;
            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
        }
    }
}
