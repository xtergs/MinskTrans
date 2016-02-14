﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;
using CommonLibrary.Notify;
using GalaSoft.MvvmLight.Command;

namespace UniversalMinskTransRelease.Nofity
{
    class NotifyHelperUniversal : INotifyHelper
    {
        private bool isShowingDialog = false;
        private Queue<MessageDialog> pendingDialogs = new Queue<MessageDialog>();
        public async Task ReportErrorAsync(string errorString)
        {
            
            Windows.UI.Popups.MessageDialog dialog = new MessageDialog("При обновлении новостей произошла ошибка, попробуйте обновить позже");
            pendingDialogs.Enqueue(dialog);
            await ShowPendingMessaagesAsync();
        }

        public async Task ShowMessageAsync(string text, List<KeyValuePair<string, RelayCommand>> commands = null)
        {
            Windows.UI.Popups.MessageDialog dialog = new MessageDialog(text);
            if (commands != null)
                foreach (KeyValuePair<string, RelayCommand> t in commands)
                {
                    dialog.Commands.Add(new UICommand(t.Key, command =>
                    {
                        if (t.Value.CanExecute(null))
                            t.Value.Execute(null);
                    }));
                }
            pendingDialogs.Enqueue(dialog);
            await ShowPendingMessaagesAsync();
        }

        public KeyValuePair<string, RelayCommand> CreateCommand(string text, RelayCommand command, object parametr)
        {
            return new KeyValuePair<string, RelayCommand>(text, new RelayCommand(() =>
            {
                command.Execute(parametr);
            }));
        }

        async Task ShowPendingMessaagesAsync()
        {
            if (isShowingDialog)
                return;
            isShowingDialog = true;
            try
            {
                while (pendingDialogs.Count > 0)
                {
                    await pendingDialogs.Dequeue().ShowAsync();
                }
            }
            finally
            {
                isShowingDialog = false;
            }
        }
    }
}
