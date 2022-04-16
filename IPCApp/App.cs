using Eto.Forms;
using System;
using System.Collections.Generic;

namespace IPCApp
{
    public static class App
    {
        public static event EventHandler<NotificationEventArgs> OnNotificationClicked;
        public static Application CurrentApp;

        public static void OpenPath(string path)
        {
            if(CurrentApp == null) return;
            CurrentApp.Open(path);
        }

        public static void OnNotificationListener(object sender, NotificationEventArgs args)
        {
            OnNotificationClicked?.Invoke(CurrentApp,args);
        }
    }
}