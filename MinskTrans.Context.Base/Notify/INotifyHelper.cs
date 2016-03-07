using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;

namespace CommonLibrary.Notify
{
    public interface INotifyHelper
    {
        Task ReportErrorAsync(string errorString);
        Task ShowMessageAsync(string text, List<KeyValuePair<string, RelayCommand>> commands = null);
        KeyValuePair<string, RelayCommand> CreateCommand(string text, RelayCommand command, object parametr);
        void ShowNotificaton(string text);

    }
}
