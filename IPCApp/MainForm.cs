using Eto.Forms;
using Eto.Drawing;
using Speedberg.Discord;
using System.IO;
using System.Reflection;

namespace IPCApp
{
	public partial class MainForm : Form
	{
		private bool _doClose;
		private TrayIndicator _tray;

		public MainForm()
		{
			Assembly myAssembly = Assembly.GetExecutingAssembly();
			Title = "Custom Status App - made by Speedberg!";
			MinimumSize = new Size(700, 700);

			using(Stream myStream = myAssembly.GetManifestResourceStream("IPCApp.Icons.Logo.png"))
			{
				Icon = new Icon(myStream);
			}

			this.Styles.Clear();
			this.Opacity = 1;
			this.BackgroundColor = Stylesheet.ColourServer;
			this.BringToFront();
			_doClose = false;

			using(Stream myStream = myAssembly.GetManifestResourceStream("IPCApp.Icons.Logo.png"))
			{
				_tray = new TrayIndicator(){
					Title="Custom Status App",
					Image = new Bitmap(myStream),
				};
			}
			ButtonMenuItem quit = new ButtonMenuItem();
			quit.Text = "Quit";
			quit.Click +=(sender, e) => {
				_doClose = true;
				this.Close();
			};

			ButtonMenuItem about = new ButtonMenuItem();
			about.Text = "About";
			about.Click += (sender, e) => {
				App.OpenPath("https://speedberg.github.io/customstatus");
			};

			ButtonMenuItem open = new ButtonMenuItem();
			open.Text = "Open";
			open.Click += (sender, e) => {
				if(this.Visible)
				{
					this.BringToFront();
				} else {
					this.Visible = true;
				}
			};
			
			_tray.Menu = new ContextMenu()
			{
				Items = {
					open,
					about,
					quit,
				}
			};
			_tray.Activated += (s,e) => {
				if(this.Visible)
				{
					this.BringToFront();
				} else {
					this.Visible = true;
				}
			};
			_tray.Show();

			this.Closing += (sender, e) => {
				if(_doClose)
				{
					IPC.Dispose();
					RichPresencePage.Dipose();
					_tray.Dispose();
					e.Cancel = false;
				} else {
					this.Visible = false;
					e.Cancel = true;
					Notification alert = new Notification();
					alert.Title = "Custom Status App";
					using(Stream myStream = myAssembly.GetManifestResourceStream("IPCApp.Icons.Logo.png"))
					{
						alert.ContentImage = new Bitmap(myStream);
					}
					alert.Message = "App minimised to system tray";
					alert.Show();
					alert.Dispose();
				}
			};

			this.Shown += (sender ,e) =>
			{
				this.Resizable = false;
				Maximizable = false;
			};

			App.OnNotificationClicked += OnNotificationClicked;

			RichPresencePage.CreateStyles();
			RichPresencePage.CreateLoadingPage(this);

			IPC.Initialize("709762089895460886",null,(result) => {
				if(!result)
				{
					MessageBox.Show("Error launching the app","Contact https://speedberg.github.io/ for support",MessageBoxType.Error);
					App.CurrentApp.Quit();
				}
				IPC.LoginAsClient(0,(success) => {
					if(success)
					{
						RichPresencePage.CreateRichPresencePage(this);
					} else {
						MessageBox.Show("Error launching the app","Contact https://speedberg.github.io/ for support",MessageBoxType.Error);
						App.CurrentApp.Quit();
					}
				});
			});
		}

		private void OnNotificationClicked(object sender, NotificationEventArgs args)
		{
			if(this.Visible)
			{
				this.BringToFront();
			} else {
				this.Visible = true;
			}
		}
	}
}
