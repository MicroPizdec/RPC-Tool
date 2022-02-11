using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using DiscordRPC;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DimkaCrash.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreatePage : Page
    {
        public DiscordRpcClient client;

        public CreatePage()
        {
            this.InitializeComponent();
        }

        public void GoButton_Click(object sender, RoutedEventArgs eventArgs)
        {
            client = new DiscordRpcClient(ClientIDTextBox.Text);

            client.OnReady += (sender, e) =>
            {
                //StartSuccess.IsOpen = true; 
            };

            client.OnPresenceUpdate += (sender, e) =>
            {
                StartSuccess.IsOpen = true;
            };

            client.Initialize();

            client.SetPresence(new RichPresence()
            {
                Details = DetailsTextBox.Text,
                State = StateTextBox.Text,
                Assets = new Assets()
                {
                    LargeImageKey = LargeImageKeyTextBox.Text,
                    LargeImageText = LargeImageTextBox.Text, // пробуйте
                    SmallImageKey = SmallImageKeyTextBox.Text,
                    SmallImageText = SmallImageTextBox.Text,
                }
            });
        }
    }
}
