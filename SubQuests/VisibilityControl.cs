using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace SubQuests.UWP
{
    class VisibilityControl : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var p = value as string;
            return p;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
