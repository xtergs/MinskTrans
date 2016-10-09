using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using MinskTrans.Context;

namespace UniversalMinskTransRelease.Helpers
{
    public class TextBlockFormatter
    {
        public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.RegisterAttached(
        "FormattedText",
        typeof(string),
        typeof(TextBlockFormatter),
        new PropertyMetadata(null, FormattedTextPropertyChanged));

        public static void SetFormattedText(DependencyObject textBlock, string value)
        {
            textBlock.SetValue(FormattedTextProperty, value);
        }

        public static string GetFormattedText(DependencyObject textBlock)
        {
            return (string)textBlock.GetValue(FormattedTextProperty);
        }

        private static void FormattedTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Clear current textBlock
            TextBlock textBlock = d as TextBlock;
            textBlock.ClearValue(TextBlock.TextProperty);
            textBlock.Inlines.Clear();
            // Create new formatted text
            if (e.NewValue is string)
            {
                textBlock.Text = (string) e.NewValue ?? "";
                return;
            }
            var formattedText = (StopSearchResult)e.NewValue;
            if (formattedText.MatchLength == 0)
            {
                textBlock.Text = formattedText.Stop.Name;
                return;
            }
            string @namespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            var text =
                $@"<Span xml:space=""preserve"" xmlns=""{@namespace}"">{formattedText.Stop.Name.Substring(0,
                    formattedText.StartMatch)}</Span>";
            // Inject to inlines
            var result = (Span)XamlReader.Load(text);
            textBlock.Inlines.Add(result);
            text = $@"<Span xml:space=""preserve"" xmlns=""{@namespace}"" Foreground=""Red"">{formattedText.Stop.Name.Substring(formattedText.StartMatch, formattedText.MatchLength)}</Span>";

            result = (Span)XamlReader.Load(text);
            textBlock.Inlines.Add(result);

            text = $@"<Span xml:space=""preserve"" xmlns=""{@namespace}"">{formattedText.Stop.Name.Substring(formattedText.StartMatch + formattedText.MatchLength)}</Span>";
            result = (Span)XamlReader.Load(text);
            textBlock.Inlines.Add(result);
        }

    }
}
