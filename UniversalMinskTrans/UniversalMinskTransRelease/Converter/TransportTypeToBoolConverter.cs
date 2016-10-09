using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using MinskTrans.Context.Base.BaseModel;

namespace UniversalMinskTransRelease.Converter
{
    public class TransportTypeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = (TransportType)value;
            var param = (TransportType) parameter;
            return val == param;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }

    public class TransportTypeToNoBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = (TransportType)value;
            var param = (TransportType)parameter;
            return val != param;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }
}
