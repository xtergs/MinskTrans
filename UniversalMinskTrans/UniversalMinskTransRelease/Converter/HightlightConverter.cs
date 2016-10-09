using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;

namespace UniversalMinskTransRelease.Converter
{
    /// <summary>
    /// Converts a string containing valid XAML into WPF objects.
    /// </summary>
    
    public sealed class StringToXamlConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string str = value as string;
            if (str == null)
                return null;
            TextBlock textBlock = parameter as TextBlock;
            if (textBlock == null)
                return value;
            // Create new formatted text
            string formattedText = str ?? string.Empty;
            string @namespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            formattedText = $@"<Span xml:space=""preserve"" xmlns=""{@namespace}"" Foreground=""Red"">{formattedText}</Span>";
            // Inject to inlines
            var result = (Span)XamlReader.Load(formattedText);
            return result;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
