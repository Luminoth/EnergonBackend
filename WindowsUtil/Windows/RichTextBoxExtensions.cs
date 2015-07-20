﻿using System.Windows.Documents;
using System.Windows.Media;

// ReSharper disable once CheckNamespace
namespace System.Windows.Controls
{
    /// <summary>
    /// Useful extension so the ystem.Windows.Controls.RichTextBox class
    /// </summary>
    public static class RichTextBoxExtensions
    {
        /// <summary>
        /// Appends colored text to the RichTextBox.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        /// <exception cref="System.ArgumentNullException">box</exception>
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
