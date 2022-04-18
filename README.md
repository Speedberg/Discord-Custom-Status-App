# Discord Custom Status App
[Website](https://speedberg.github.io/apps/customstatus)

An app for Discord which allows for the creation of custom rich presence statuses.

## Download
[Latest Release](https://github.com/SpeedbergDragonFire/Discord-Custom-Status-App/releases/latest)

I have only tested Windows builds so far, but it should be possible to build for Mac and Linux via compiling the source code - see [Installation](#Installation).

## FAQ

### Why doesn't my status appear?
Make sure that **Activity Settings** -> **Activity Status** -> **Display current activity as a status message** is set to true in your Discord settings.

### How do I close the app?
When you close the window, the app minimizes itself to the system tray - right click and select **Quit** to close the app.

### How do I set a custom application ID?
1. Go to the [Discord developers page](https://discord.com/developers/applications) and create a new application - the name you put will be the name displayed on your status, e.g. Playing *your app name here*.
2. Copy the number underneath **APPLICATION ID**.
3. Go back to the Custom Status App, and paste the number you copied into the **Custom Application ID** text box and hit **Set Activity** - it might take a while to change to the new application.
4. It should now display your app on your profile!

### How do I set a custom image?
Custom images require a url, for example https://speedberg.github.io/images/sus.png

### Does this work on mobile?
No - this use's Discord's IPC library which only works on Desktop.

### Why is your code so messy?!
I'm sorry :(

## Installation
Dependencies:
- Dec.DiscordIPC 1.2.2
- Eto.Forms 2.6.1
- Newtonsoft.Json 13.0.1
- RestSharp 107.1.2
- System.Text.Encodings.Web 5.0.0.1

Download source code, then compile for which ever build you are targeting in the specified folder (IPCApp.Wpf for Windows, IPCApp.Gtk for Linux, IPCApp.Mac for MacOS).
For compiling on platforms other than Windows, please read [this article](https://github.com/picoe/Eto/wiki/Publishing-your-App) on publishing Eto.Forms apps.
