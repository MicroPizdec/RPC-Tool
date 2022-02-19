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
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using DiscordRPC;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;

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
                SetTitleBar(AppTitleBar);
            }
            else
            {
                AppTitleBar.Visibility = Visibility.Collapsed;
            }

            Activated += MainWindow_Activated;
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == WindowActivationState.Deactivated)
            {
                TitleTextBlock.Foreground =
                    (SolidColorBrush)App.Current.Resources["WindowCaptionForegroundDisabled"];
            }
            else
            {
                TitleTextBlock.Foreground =
                    (SolidColorBrush)App.Current.Resources["WindowCaptionForeground"];
            }
        }

        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }

        public bool ValidateInput()
        {
            if (string.IsNullOrEmpty(ClientIDTextBox.Text) || 
                string.IsNullOrEmpty(DetailsTextBox.Text) || 
                string.IsNullOrEmpty(StateTextBox.Text))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void GoButton_Click(object sender, RoutedEventArgs args)
        {
            if (!ValidateInput())
            {
                NoInput.IsOpen = true;
                return;
            }

            client = new DiscordRpcClient(ClientIDTextBox.Text);

            /* client.OnReady += (sender, e) =>
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
            }; */

            GoButton.IsEnabled = false;
            StopButton.IsEnabled = true;

            client.Initialize();

            ulong startTimestamp = 0;
            ulong endTimestamp = 0;

            ulong.TryParse(StartTextBox.Text, out startTimestamp);
            ulong.TryParse(EndTextBox.Text, out endTimestamp);

            RichPresence presence = new RichPresence()
            {
                Details = DetailsTextBox.Text,
                State = StateTextBox.Text,
                Assets = new Assets()
                {
                    LargeImageKey = LargeImageKeyTextBox.Text,
                    LargeImageText = LargeImageTextBox.Text,
                    SmallImageKey = SmallImageKeyTextBox.Text,
                    SmallImageText = SmallImageTextBox.Text
                }
            };

            if (startTimestamp != 0 && endTimestamp != 0)
            {
                presence.Timestamps = new Timestamps()
                {
                    StartUnixMilliseconds = startTimestamp,
                    EndUnixMilliseconds = endTimestamp
                };
            }

            if (!string.IsNullOrEmpty(PartyIDTextBox.Text) && !string.IsNullOrEmpty(PartySizeTextBox.Text) &&
                !string.IsNullOrEmpty(PartyMaxTextBox.Text))
            {
                presence.Party = new Party()
                {
                    ID = PartyIDTextBox.Text,
                    Size = int.Parse(PartySizeTextBox.Text),
                    Max = int.Parse(PartyMaxTextBox.Text)
                };
            }

            if (!string.IsNullOrEmpty(ButtonTextBox.Text) && !string.IsNullOrEmpty(ButtonURLTextBox.Text))
            {
                presence.Buttons = new DiscordRPC.Button[]
                {
                    new DiscordRPC.Button()
                    {
                        Label = ButtonTextBox.Text,
                        Url = ButtonURLTextBox.Text,
                    }
                };
            }

            client.SetPresence(presence);

            StartSuccess.IsOpen = true;
        }

        public void StopButton_Click(object sender, RoutedEventArgs args)
        {
            client.Dispose();
  
            StopSuccess.IsOpen = true;

            GoButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

        public void AddButton_Click(object sender, RoutedEventArgs args)
        {
            // idk
        }

        public async void SaveButton_Click(object sender, RoutedEventArgs args)
        {
            FileSavePicker savePicker = new FileSavePicker();

            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.FileTypeChoices.Add("JSON config", new List<string>() { ".json" });
            savePicker.SuggestedFileName = "rpc-config";

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);

                ulong startTimestamp = 0;
                ulong endTimestamp = 0;

                ulong.TryParse(StartTextBox.Text, out startTimestamp);
                ulong.TryParse(EndTextBox.Text, out endTimestamp);

                JObject o = JObject.FromObject(new
                {
                    clientID = ClientIDTextBox.Text,
                    details = DetailsTextBox.Text,
                    state = StateTextBox.Text,
                    assets = new
                    {
                        largeImageKey = LargeImageKeyTextBox.Text,
                        largeImageText = LargeImageTextBox.Text,
                        smallImageKey = SmallImageKeyTextBox.Text,
                        smallImageText = SmallImageTextBox.Text
                    },
                    party = new
                    {
                        id = PartyIDTextBox.Text,
                        size = int.Parse(PartySizeTextBox.Text),
                        max = int.Parse(PartyMaxTextBox.Text)
                    },
                    timestamps = new
                    {
                        start = startTimestamp,
                        end = endTimestamp
                    }
                });

                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);

                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    o.WriteTo(writer);
                }

                await FileIO.WriteTextAsync(file, sb.ToString());
                await CachedFileManager.CompleteUpdatesAsync(file);
            }
            else
            {
                return;
            }
        }

        public async void OpenButton_Click(object sender, RoutedEventArgs args) 
        {
            FileOpenPicker picker = new FileOpenPicker();

            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(picker, hwnd);

            picker.ViewMode = PickerViewMode.List;
            picker.FileTypeFilter.Add(".json");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                string json = await FileIO.ReadTextAsync(file);
                JObject o;
                try
                {
                    o = JObject.Parse(json);

                    ClientIDTextBox.Text = (string)o.SelectToken("clientID");
                    DetailsTextBox.Text = (string)o.SelectToken("details");
                    StateTextBox.Text = (string)o.SelectToken("state");
                    StartTextBox.Text = (string)o.SelectToken("timestamps.start");
                    EndTextBox.Text = (string)o.SelectToken("timestamps.end");
                    LargeImageKeyTextBox.Text = (string)o.SelectToken("assets.largeImageKey");
                    LargeImageTextBox.Text = (string)o.SelectToken("assets.largeImage");
                    SmallImageKeyTextBox.Text = (string)o.SelectToken("assets.smallImageKey");
                    SmallImageTextBox.Text = (string)o.SelectToken("assets.smallImageKey");
                    PartyIDTextBox.Text = (string)o.SelectToken("party.id");
                    PartySizeTextBox.Text = (string)o.SelectToken("party.size");
                    PartyMaxTextBox.Text = (string)o.SelectToken("party.max");
                }
                catch
                {
                    ContentDialog dialog = new ContentDialog();
                    dialog.Content = "Failed to load config.";
                    dialog.PrimaryButtonText = "OK";

                    await dialog.ShowAsync();
                }
            } 
            else
            {
                return;
            }
        }
    }
}
