using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace rmsoft.ChangeTracking.TestApp.Converters
{
    public class NotifyPropertyChangedEventArgsConverter : IValueConverter
    {
        private object GetItemsString(IList? items)
        {
            if (items == null
                || items.Count == 0)
                return "(None)";

            StringBuilder sb = new StringBuilder();
            foreach(object obj in items)
            {
                sb.Append(obj.ToString());
                sb.Append(' ');
            }

            return $"{{{sb.ToString()}}}";
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            NotifyCollectionChangedEventArgs val = (NotifyCollectionChangedEventArgs)value;

            return $"{val.Action} {val.NewStartingIndex} {GetItemsString(val.NewItems)} {val.OldStartingIndex} {GetItemsString(val.OldItems)}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
