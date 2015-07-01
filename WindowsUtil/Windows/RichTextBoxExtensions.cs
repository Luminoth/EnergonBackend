using System.Windows.Documents;
using System.Windows.Media;

// ReSharper disable once CheckNamespace
namespace System.Windows.Controls
{
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            if(null == box) {
                throw new ArgumentNullException("box");
            }

            TextRange range = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd)
            {
                Text = text,
            };
            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
        }
    }
}
