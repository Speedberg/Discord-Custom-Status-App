using System;
using Eto.Forms;

namespace IPCApp.Wpf
{
	class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Application application = null;

			try
			{
				application = new Application(Eto.Platforms.Wpf);
				IPCApp.App.CurrentApp = application;
				application.Name = "Custom Status App";

				application.BadgeLabel = "Custom Status App";

				application.NotificationActivated += App.OnNotificationListener;

				application.Run(new MainForm());

			} catch(Exception e)
			{
				MessageBox.Show($"An error occured when launching the app: {e.Message}","Contact https://speedberg.github.io/ for support",MessageBoxType.Error);
				Speedberg.Discord.IPC.Dispose();
				application?.Quit();
			}
		}
	}
}
