using Eto.Forms;
using Eto.Drawing;
using System.Net;
using System.Collections.Generic;
using Speedberg.Discord;
using Presence = Dec.DiscordIPC.Entities.Presence;

namespace IPCApp
{
    public static class RichPresencePage
    {
		#region Activity Properties
        //State / details
        private static TextBox e_state;
        private static TextBox e_details;

        //Large Image
        private static CheckBox e_largeImageCheckbox;
        private static TextBox e_largeImageURL;
        private static TextBox e_largeImageText;

        //Small Image
        private static CheckBox e_smallImageCheckbox;
        private static TextBox e_smallImageURL;
        private static TextBox e_smallImageText;

		//Buttons
		private static CheckBox e_buttonOneCheck;
		private static TextBox e_buttonOneURL;
		private static TextBox e_buttonOneText;
		private static CheckBox e_buttonTwoCheck;
		private static TextBox e_buttonTwoURL;
		private static TextBox e_buttonTwoText;

		//Custom app id
		private static TextBox e_customApplicationID;
		private static Button e_customAppHelp;

        //Activity Button
        private static Button e_setActivityButton;
		private static Button e_clearActivityButton;

		#endregion

		//Client parent
		private static DynamicLayout e_clientParent;
		private static Label e_clientLoadingLabel;
		private static List<ClientUser> e_clients;
		
		private static bool _loadingClient = false;
		private static int _selectedPipeline = 0;
		public static bool _switchingClient = false;

		private static bool _created = false;
        
		public static void CreateStyles()
		{
			Eto.Style.Add<Control>(null,control => {
				control.BackgroundColor = Stylesheet.ColourServer;
			});
			Eto.Style.Add<TextControl>(null,text => {
				text.Font = new Font(FontFamilies.MonospaceFamilyName,12f,FontStyle.None,FontDecoration.None);
				text.TextColor = Stylesheet.ColourWhite;
			});
			Eto.Style.Add<TextBox>(null,text => {
				text.Font = new Font(FontFamilies.MonospaceFamilyName,12f,FontStyle.None,FontDecoration.None);
				text.TextColor = Stylesheet.ColourWhite;
				text.BackgroundColor = Stylesheet.ColourChannel;
			});
            Eto.Style.Add<TextBox>("textBox",text => {
				text.Font = new Font(FontFamilies.MonospaceFamilyName,12f,FontStyle.None,FontDecoration.None);
				text.TextColor = Stylesheet.ColourWhite;
				text.BackgroundColor = Stylesheet.ColourChannel;
			});
			Eto.Style.Add<Button>(null, button => {
				button.BackgroundColor = Stylesheet.ColourDiscordOld;
			});
			Eto.Style.Add<Button>("button-confirm", button => {
				button.BackgroundColor = Stylesheet.ColourOnline;
				button.TextColor = Stylesheet.ColourWhite;
			});
            Eto.Style.Add<Control>("hidden",control => {
                control.BackgroundColor = Stylesheet.ColourInvisible;
            });
            Eto.Style.Add<TextControl>("disabled",control => {
                control.BackgroundColor = Stylesheet.ColourDisable;
                control.TextColor = Stylesheet.ColourDisable;
            });
		}

		public static void CreateLoadingPage(Form form)
		{
			Label label = new Label(){
				Text="Loading...\n\n(This normally takes a minute)",
				TextColor=Stylesheet.ColourDiscordOld,
				VerticalAlignment = VerticalAlignment.Center,
				TextAlignment = TextAlignment.Center
			};
			form.Content = label;
		}

		public static void CreateRichPresencePage(Form form)
		{
			_loadingClient = false;
			_selectedPipeline = 0;

			e_clients = new List<ClientUser>();

            e_details = new TextBox(){PlaceholderText="Details"};
			e_state = new TextBox(){PlaceholderText="State"};

			e_largeImageCheckbox = new CheckBox(){Text="Large Image"};
			e_largeImageCheckbox.CheckedChanged += (sender,e) => {
				ToggleLargeImage();
			};
            e_largeImageURL = new TextBox(){PlaceholderText="Large Image URL"};
			e_largeImageText = new TextBox(){PlaceholderText="Large Image Text"};

            e_smallImageCheckbox = new CheckBox(){Text="Small Image"};
            e_smallImageCheckbox.CheckedChanged += (sender,e) => {
				ToggleSmallImage();
			};
			e_smallImageURL = new TextBox(){PlaceholderText="Small Image URL"};
			e_smallImageText = new TextBox(){PlaceholderText="Small Image Text"};


            e_setActivityButton = new Button(){Text="Set Activity",Style="button-confirm"};
			e_setActivityButton.Click += (sender, e) => {
                OnSetActivity();
            };

			e_clearActivityButton = new Button(){Text="Clear Activity"};
			e_clearActivityButton.BackgroundColor = Stylesheet.ColourDND;
			e_clearActivityButton.Click += (sender, e) => {
				try
				{
					IPC.UpdatePresence(null, null);
				} catch
				{

				}
			};

			e_buttonOneCheck = new CheckBox(){Text="Button 1"};
			e_buttonOneCheck.CheckedChanged += (sender, e) => {
				ToggleButtons(true);
			};
			e_buttonOneURL = new TextBox(){PlaceholderText="Button 1 URL"};
			e_buttonOneText = new TextBox(){PlaceholderText="Button 1 Text"};

			e_buttonTwoCheck = new CheckBox(){Text="Button 2"};
			e_buttonTwoCheck.CheckedChanged += (sender, e) => {
				ToggleButtons(false);
			};
			e_buttonTwoURL = new TextBox(){PlaceholderText="Button 2 URL"};
			e_buttonTwoText = new TextBox(){PlaceholderText="Button 2 Text"};

			e_clientLoadingLabel = new Label(){Text="Loading clients...",TextAlignment=TextAlignment.Center,VerticalAlignment=VerticalAlignment.Center};
			var refreshClientsBtn = new Button(){Text="Refresh",Size=new Size(-1,25)};
			refreshClientsBtn.Click += (sender, e) => {
				CreateClients();
			};

			e_customApplicationID = new TextBox(){PlaceholderText="Custom Application ID"};
			e_customAppHelp = new Button(){Text="Help!",Size=new Size(-1,25)};
			e_customAppHelp.Click += (sender, e) => {
				App.OpenPath("https://speedberg.github.io/customstatushelp");
			};

			e_clientParent = new DynamicLayout()
			{
				Size = new Size(300,-1),
				Spacing=new Size(0,0),
				Rows=
				{
					new Label(){Text="Client Selector",TextAlignment=TextAlignment.Center},
					refreshClientsBtn,
					e_clientLoadingLabel,
				}
			};
			DynamicLayout layout = new DynamicLayout()
			{
				Rows={
					new DynamicRow()
					{
						new DynamicLayout()
						{
							Size = new Size(-1, 10),
						}
					},
					new DynamicRow()
					{
						new DynamicLayout()
						{
							Size = new Size(10,-1),
						},
						new DynamicLayout()
						{
							Size = new Size(240,-1),
							Spacing = new Size(-1,10),
							Rows=
							{
								new Label(){Text="Settings",TextAlignment=TextAlignment.Center},
								e_details,
                                e_state,
								e_largeImageCheckbox,
									e_largeImageURL,
                                    e_largeImageText,
								e_smallImageCheckbox,
                                    e_smallImageURL,
                                    e_smallImageText,
								e_buttonOneCheck,
									e_buttonOneURL,
									e_buttonOneText,
								e_buttonTwoCheck,
									e_buttonTwoURL,
									e_buttonTwoText,
								e_setActivityButton,
								e_clearActivityButton,
								e_customApplicationID,
								e_customAppHelp,
							}
						},
						new DynamicLayout()
						{
							Size = new Size(100,-1),
						},
						e_clientParent,
						new DynamicLayout()
						{
							Size = new Size(10,-1),
						}
					},
					new DynamicRow()
					{
						new DynamicLayout()
						{
							Size = new Size(-1, 20)
						}
					},	
				}
			};

			Scrollable container = new Scrollable(){
				Border = BorderType.None,
				Content = layout,
			};

			form.Content = container;

            ToggleLargeImage();
            ToggleSmallImage();
			ToggleButtons(true);
			ToggleButtons(false);
			
			CreateClientGraphics();

			CreateClients(false);

			_created = true;
		}
	
		public static void Dipose()
		{
			if(!_created) return;
			if(e_state != null)
			{
				e_state.Dispose();
				e_state = null;
			}
			if(e_details != null)
			{
				e_details.Dispose();
				e_details = null;
			}
			if(e_largeImageCheckbox != null)
			{
				e_largeImageCheckbox.Dispose();
				e_largeImageCheckbox = null;
			}
			if(e_largeImageURL != null)
			{
				e_largeImageURL.Dispose();
				e_largeImageURL = null;
			}
			if(e_largeImageText != null)
			{
				e_largeImageText.Dispose();
				e_largeImageText = null;
			}
			if(e_smallImageCheckbox != null)
			{
				e_smallImageCheckbox.Dispose();
				e_smallImageCheckbox = null;
			}
			if(e_smallImageURL != null)
			{
				e_smallImageURL.Dispose();
				e_smallImageURL = null;
			}
			if(e_smallImageText != null)
			{
				e_smallImageText.Dispose();
				e_smallImageText = null;
			}
			if(e_buttonOneCheck != null)
			{
				e_buttonOneCheck.Dispose();
				e_buttonOneCheck = null;
			}
			if(e_buttonOneURL != null)
			{
				e_buttonOneURL.Dispose();
				e_buttonOneURL = null;
			}
			if(e_buttonOneText != null)
			{
				e_buttonOneText.Dispose();
				e_buttonOneText = null;
			}
			if(e_buttonTwoCheck != null)
			{
				e_buttonTwoCheck.Dispose();
				e_buttonTwoCheck = null;
			}
			if(e_buttonTwoURL != null)
			{
				e_buttonTwoURL.Dispose();
				e_buttonTwoURL = null;
			}
			if(e_buttonTwoText != null)
			{
				e_buttonTwoText.Dispose();
				e_buttonTwoText = null;
			}
			if(e_setActivityButton != null)
			{
				e_setActivityButton.Dispose();
				e_setActivityButton = null;
			}
			if(e_clearActivityButton != null)
			{
				e_clearActivityButton.Dispose();
				e_clearActivityButton = null;
			}
			if(e_customApplicationID != null)
			{
				e_customApplicationID.Dispose();
				e_customApplicationID = null;
			}
			if(e_customAppHelp != null)
			{
				e_customAppHelp.Dispose();
				e_customAppHelp = null;
			}
			if(e_clientLoadingLabel != null)
			{
				e_clientLoadingLabel.Dispose();
				e_clientLoadingLabel = null;
			}
			for(int i = 0; i < e_clients.Count; i++)
			{
				e_clients[i].Dispose();
				e_clients[i] = null;
			}
			e_clients.Clear();
			if(e_clientParent != null)
			{
				e_clientParent.Dispose();
				e_clientParent = null;
			}
		}

		private static void ToggleLargeImage()
		{
            if(e_largeImageCheckbox.Checked == true)
            {
                e_largeImageURL.Enabled = true;
                e_largeImageURL.Style = "textBox";
                e_largeImageText.Enabled = true;
                e_largeImageText.Style = "textBox";
            } else {
                e_largeImageURL.Enabled = false;
                e_largeImageURL.Style = "hidden";
                e_largeImageText.Enabled = false;
                e_largeImageText.Style = "hidden";
            }
		}

        private static void ToggleSmallImage()
		{
            if(e_smallImageCheckbox.Checked == true)
            {
                e_smallImageURL.Enabled = true;
                e_smallImageURL.Style = "textBox";
                e_smallImageText.Enabled = true;
                e_smallImageText.Style = "textBox";
            } else {
                e_smallImageURL.Enabled = false;
                e_smallImageURL.Style = "hidden";
                e_smallImageText.Enabled = false;
                e_smallImageText.Style = "hidden";
            }
		}
    
		private static void ToggleButtons(bool one)
		{
			if(one)
			{
				e_buttonOneURL.Enabled = (bool)e_buttonOneCheck.Checked;
				e_buttonOneText.Enabled = (bool)e_buttonOneCheck.Checked;
				e_buttonOneURL.Style = (e_buttonOneURL.Enabled == true) ? "textBox" : "hidden";
				e_buttonOneText.Style = (e_buttonOneText.Enabled == true) ? "textBox" : "hidden";
			} else {
				e_buttonTwoURL.Enabled = (bool)e_buttonTwoCheck.Checked;
				e_buttonTwoText.Enabled = (bool)e_buttonTwoCheck.Checked;
				e_buttonTwoURL.Style = (e_buttonTwoURL.Enabled == true) ? "textBox" : "hidden";
				e_buttonTwoText.Style = (e_buttonTwoText.Enabled == true) ? "textBox" : "hidden";
			}
		}

		private static void CreateClients(bool detect = true)
		{
			if(_loadingClient) return;
			HideClientGraphics();
			e_clientLoadingLabel.Visible = true;

			e_clientParent.Create();

			if(detect)
			{
				_loadingClient = true;
				IPC.DetectClients((result) => {
					if(result)
					{
						e_clientLoadingLabel.Visible = false;
						_loadingClient = false;
						UpdateClientGraphics();
					} else {
						e_clientLoadingLabel.Text = "Failed to detect any clients!";
					}
				});
			} else {
				e_clientLoadingLabel.Visible = false;
				UpdateClientGraphics();
			}
		}

		private static void HideClientGraphics()
		{
			for(int i = 0; i < e_clients.Count; i++)
			{
				if(e_clients[i].button != null) e_clients[i].button.Visible = false;
			}
		}

		private static void CreateClientGraphics()
		{
			for(int i = 0; i < IPC.CLIENT_LIMIT; i++)
			{
				ClientUser user = new ClientUser();
				e_clients.Add(user);
				e_clientParent.Rows.Add(user.control);
			}
			e_clients[0].button.BackgroundColor = Stylesheet.ColourOnline;
			e_clientParent.Create();
		}

		private static void UpdateClientGraphics()
		{
			for(int i = 0; i < IPC.ActiveClients.Length; i++)
			{
				e_clients[i].UpdateData(IPC.ActiveClients[i]);
				e_clients[i].button.Visible = true;
			}
		}

        private static void OnSetActivity()
        {
			e_setActivityButton.Enabled = false;
			e_setActivityButton.BackgroundColor = Stylesheet.ColourIdle;
			try
			{
				var activity = new Dec.DiscordIPC.Entities.Presence.Activity();
				if(e_details.Text != "") activity.details = e_details.Text;
				if(e_state.Text != "") activity.state = e_state.Text;
				//Images
				if(e_largeImageCheckbox.Checked == true || e_smallImageCheckbox.Checked == true)
				{
					var assets = new Presence.Activity.Assets();
					if(e_largeImageCheckbox.Checked == true)
					{
						if(e_largeImageURL.Text != "") assets.large_image = e_largeImageURL.Text;
						if(e_largeImageText.Text != "") assets.large_text = e_largeImageText.Text;
					}
					if(e_smallImageCheckbox.Checked == true)
					{
						if(e_smallImageURL.Text != "") assets.small_image = e_smallImageURL.Text;
						if(e_smallImageText.Text != "") assets.small_text = e_smallImageText.Text;
					}
					activity.assets = assets;
				}
				//Buttons
				if(e_buttonOneCheck.Checked == true || e_buttonTwoCheck.Checked == true)
				{
					activity.buttons = new System.Collections.Generic.List<Presence.Activity.Button>();

					if(e_buttonOneCheck.Checked == true)
					{
						activity.buttons.Add(new Presence.Activity.Button(){
							url = e_buttonOneURL.Text,
							label = e_buttonOneText.Text
						});
					}

					if(e_buttonTwoCheck.Checked == true)
					{
						activity.buttons.Add(new Presence.Activity.Button(){
							url = e_buttonTwoURL.Text,
							label = e_buttonTwoText.Text
						});
					}
				}

				if(e_customApplicationID.Text != "" && !(e_customApplicationID.Text == IPC.CLIENT_ID))
				{
					long customID = 0;
					if(!long.TryParse(e_customApplicationID.Text,out customID))
					{
						throw new System.Exception("Incorrect application ID");
					}

					IPC.Dispose();

					IPC.Initialize(customID.ToString(),null,(login) => {
						if(login == false)
						{
							//Relogin as og
							IPC.Initialize("709762089895460886",null,(result) => {
								IPC.LoginAsClient(0, (loginResult) => {
									MessageBox.Show($"Error setting the activity: That app does not exist!","Error",MessageBoxType.Error);
									e_setActivityButton.Enabled = true;
									e_setActivityButton.BackgroundColor = Stylesheet.ColourOnline;
								});
							});
							return;
						}

						IPC.LoginAsClient(_selectedPipeline,(success) => {
							if(success)
							{
								IPC.UpdatePresence(activity, (result) => {
									if(result == null)
									{
										MessageBox.Show("Successfully set activity!","Success",MessageBoxType.Information);
										e_setActivityButton.Enabled = true;
										e_setActivityButton.BackgroundColor = Stylesheet.ColourOnline;
									} else {
										MessageBox.Show($"Error setting the activity: {result}","Error",MessageBoxType.Error);
										e_setActivityButton.Enabled = true;
										e_setActivityButton.BackgroundColor = Stylesheet.ColourOnline;
										return;
									}
								});
							} else {
								//Relogin as og
								IPC.Initialize("709762089895460886",null,(result) => {
									IPC.LoginAsClient(0,null);
									MessageBox.Show($"Error setting the activity: Failed to login as client","Error",MessageBoxType.Error);
									e_setActivityButton.Enabled = true;
									e_setActivityButton.BackgroundColor = Stylesheet.ColourOnline;
								});
								return;
							}
						});
					});
				} else if(e_customApplicationID.Text == "" && !("709762089895460886" == IPC.CLIENT_ID))
				{
					IPC.Initialize("709762089895460886",null,(result) => {
						IPC.LoginAsClient(0, (loginResult) => {
							IPC.UpdatePresence(activity, (activityResult) => {
								if(activityResult == null)
								{
									MessageBox.Show("Successfully set activity!","Success",MessageBoxType.Information);
								} else {
									MessageBox.Show($"Error setting the activity: {result}","Error",MessageBoxType.Error);
								}
								e_setActivityButton.Enabled = true;
								e_setActivityButton.BackgroundColor = Stylesheet.ColourOnline;
							});
						});
					});
				} else {
					IPC.UpdatePresence(activity, (result) => {
						if(result == null)
						{
							MessageBox.Show("Successfully set activity!","Success",MessageBoxType.Information);
						} else {
							MessageBox.Show($"Error setting the activity: {result}","Error",MessageBoxType.Error);
						}
						e_setActivityButton.Enabled = true;
						e_setActivityButton.BackgroundColor = Stylesheet.ColourOnline;
					});
				}
			} catch (System.Exception e)
			{
				MessageBox.Show($"Error setting the activity: {e.Message ?? "Unknown"}","Error",MessageBoxType.Error);
				e_setActivityButton.Enabled = true;
				e_setActivityButton.BackgroundColor = Stylesheet.ColourOnline;
			}
        }
    
		private static void SwitchClient(int pipelineID)
		{
			if(_switchingClient) return;
			_switchingClient = true;

			IPC.Logout((result) => {
				if(result)
				{
					IPC.LoginAsClient(pipelineID,(success) => {
						if(success)
						{
							_selectedPipeline = pipelineID;

							foreach(ClientUser client in e_clients)
							{
								if(client.data.pipelineID == pipelineID)
								{
									client.button.BackgroundColor = Stylesheet.ColourOnline;
								} else {
									client.button.BackgroundColor = Stylesheet.ColourDiscordOld;
								}
							}
							_switchingClient = false;
						} else {
							MessageBox.Show("Failed to connect to client.","Error",MessageBoxType.Error);
							_switchingClient = false;
							foreach(ClientUser client in e_clients)
							{
								client.button.BackgroundColor = Stylesheet.ColourDND;
							}
						}
					});
				} else {
					_switchingClient = false;
				}
			});
		}

		public class ClientUser
		{
			public IPC.Client data;
			public DynamicRow control;
			public Button button;

			private Bitmap _image;

			public ClientUser()
			{
				button = new Button()
							{
								Size=new Size(100,100),
								BackgroundColor = Stylesheet.ColourDiscordOld,
								Text = "Unknown",
							};
				
				button.Click += OnClick;
				
				this.control = new DynamicRow()
				{
					new DynamicLayout()
					{
						Rows=
						{
							button,
							new Splitter()
						}
					},
				};
			}

			public void UpdateData(IPC.Client data)
			{
				this.data = data;
				
				if(_image != null)
				{
					_image.Dispose();
				}
				_image = null;
				if(button.Image != null)
				{
					if(!button.Image.IsDisposed) button.Image.Dispose();
				}
				button.Image = null;

				try
				{
					button.Text = data.username;
					using (WebClient webClient = new WebClient()) 
					{
						byte [] imageData = webClient.DownloadData(data.GetAvatarURL());
						_image = new Bitmap(imageData);
						button.Image = _image;	
					}
				} catch {
					
				}
			}

			private void OnClick(object sender, System.EventArgs e)
			{
				SwitchClient(data.pipelineID);
			}
		
			public void Dispose()
			{
				if(control != null)
				{
					control.Clear();
				}
				control = null;
				if(_image != null)
				{
					_image.Dispose();
				}
				if(button != null)
				{
					button.Click -= OnClick;
					if(button.Image != null)
					{
						if(!button.Image.IsDisposed) button.Image.Dispose();
					}
					button.Image = null;
					button.Dispose();
				}
				_image = null;
				button = null;
			}
		}
	}
}