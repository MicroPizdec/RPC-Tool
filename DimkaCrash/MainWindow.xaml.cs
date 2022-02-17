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
using System.Timers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using DiscordRPC;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DimkaCrash
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public DiscordRpcClient client;
        public Timer timer;

        private AppWindow window;

        public MainWindow()
        {
            this.InitializeComponent();

            window = GetAppWindowForCurrentWindow();
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = window.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
            }
            else
            {
                AppTitleBar.Visibility = Visibility.Collapsed;
            }
        }

        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }

        public void GoButton_Click(object sender, RoutedEventArgs eventArgs)
        {
            client = new DiscordRpcClient(ClientIDTextBox.Text);

            client.OnReady += (sender, e) =>
            {
                StartSuccess.IsOpen = true;
            };

            client.OnConnectionFailed += (sender, e) =>
            {
                StartFailed.IsOpen = true;
                GoButton.IsEnabled = true;
                StopButton.IsEnabled = false;
            };

            client.OnError += (sender, e) =>
            {
                StartFailed.IsOpen = true;
                GoButton.IsEnabled = true;
                StopButton.IsEnabled = false;
            };

            timer = new Timer(100);
            timer.Elapsed += (sender, e) => { client.Invoke(); };
            timer.Start();

            GoButton.IsEnabled = false;
            StopButton.IsEnabled = true;

            client.Initialize();

            client.SetPresence(new RichPresence()
            {
                Details = DetailsTextBox.Text,
                State = StateTextBox.Text,
                Assets = new Assets()
                {
                    LargeImageKey = LargeImageKeyTextBox.Text,
                    LargeImageText = LargeImageTextBox.Text,
                    SmallImageKey = SmallImageKeyTextBox.Text,
                    SmallImageText = SmallImageTextBox.Text
                },
                Timestamps = new Timestamps()
                {
                    StartUnixMilliseconds = ulong.Parse(StartTextBox.Text),
                    EndUnixMilliseconds = ulong.Parse(EndTextBox.Text)
                }
            });
        }

        public void StopButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            client.Dispose();
  
            StopSuccess.IsOpen = true;

            GoButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }
    }
}
