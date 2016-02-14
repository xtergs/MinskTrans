using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using CommonLibrary.Notify;
using GalaSoft.MvvmLight.CommandWpf;
using RelayCommand = GalaSoft.MvvmLight.Command.RelayCommand;

public class NotifyHelperDesctop : INotifyHelper
{
    public async Task ReportErrorAsync(string errorString)
    {
        MessageBox.Show(errorString);
    }

    public async Task ShowMessageAsync(string text, List<KeyValuePair<string, RelayCommand>> commands = null)
    {
        var res = MessageBox.Show(text, text, MessageBoxButton.OK);
        if (commands != null && commands.Count > 0)
        {
            commands[0].Value.Execute(null);
        }
    }

    public KeyValuePair<string, RelayCommand> CreateCommand(string text, RelayCommand command, object parametr)
    {
        throw new NotImplementedException();
    }
}