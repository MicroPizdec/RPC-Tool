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
        private DiscordRpcClient client;
        private AppWindow window;
        private StorageFile openedFile;
        
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

        private bool ValidateInput()
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

        private void GoButton_Click(object sender, RoutedEventArgs args)
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

            if (!string.IsNullOrEmpty(ButtonTextBox.Text) && !string.IsNullOrEmpty(ButtonURLTextBox.Text) && !string.IsNullOrEmpty(Button2TextBox.Text) && !string.IsNullOrEmpty(Button2URLTextBox.Text))
            {
                presence.Buttons = new DiscordRPC.Button[]
                {
                    new DiscordRPC.Button()
                    {
                        Label = ButtonTextBox.Text,
                        Url = ButtonURLTextBox.Text,
                    },
                    new DiscordRPC.Button()
                    {
                        Label = Button2TextBox.Text,
                        Url = Button2URLTextBox.Text,
                    }
                };
            }

            client.SetPresence(presence);

            StartSuccess.IsOpen = true;
        }

        private void StopButton_Click(object sender, RoutedEventArgs args)
        {
            client.Dispose();
  
            StopSuccess.IsOpen = true;

            GoButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

        private string ToJson()
        {
            ulong startTimestamp = 0;
            ulong endTimestamp = 0;

            ulong.TryParse(StartTextBox.Text, out startTimestamp);
            ulong.TryParse(EndTextBox.Text, out endTimestamp);

            int partySize = 0;
            int partyMax = 0;

            int.TryParse(PartySizeTextBox.Text, out partySize);
            int.TryParse(PartyMaxTextBox.Text, out partyMax);

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
                    size = partySize,
                    max = partyMax
                },
                timestamps = new
                {
                    start = startTimestamp,
                    end = endTimestamp
                },
                button1 = new
                {
                    b1text = ButtonTextBox.Text,
                    b1url = ButtonURLTextBox.Text
                },
                button2 = new
                {
                    b2text = Button2TextBox.Text,
                    b2url = Button2URLTextBox.Text
                }
            });

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                o.WriteTo(writer);
            }

            return sb.ToString();
        }

        private async void NewButton_Click(object sender, RoutedEventArgs args)
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = "Just don't...",
                Content = "Leave me alone! (for now)",
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private async void SaveAsButton_Click(object sender, RoutedEventArgs args)
        {
            FileSavePicker savePicker = new FileSavePicker();

            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.FileTypeChoices.Add("JSON config", new List<string>() { ".json" });
            savePicker.SuggestedFileName = "rpc-config";

            openedFile = await savePicker.PickSaveFileAsync();
            if (openedFile != null)
            {
                CachedFileManager.DeferUpdates(openedFile);

                await FileIO.WriteTextAsync(openedFile, ToJson());
                await CachedFileManager.CompleteUpdatesAsync(openedFile);
            }
        }

        private async void OpenButton_Click(object sender, RoutedEventArgs args) 
        {
            FileOpenPicker picker = new FileOpenPicker();

            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(picker, hwnd);

            picker.ViewMode = PickerViewMode.List;
            picker.FileTypeFilter.Add(".json");

            openedFile = await picker.PickSingleFileAsync();
            if (openedFile != null)
            {
                string json = await FileIO.ReadTextAsync(openedFile);
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
                    LargeImageTextBox.Text = (string)o.SelectToken("assets.largeImageText");
                    SmallImageKeyTextBox.Text = (string)o.SelectToken("assets.smallImageKey");
                    SmallImageTextBox.Text = (string)o.SelectToken("assets.smallImageText");
                    PartyIDTextBox.Text = (string)o.SelectToken("party.id");
                    PartySizeTextBox.Text = (string)o.SelectToken("party.size");
                    PartyMaxTextBox.Text = (string)o.SelectToken("party.max");
                    ButtonTextBox.Text = (string)o.SelectToken("button1.b1text");
                    ButtonURLTextBox.Text = (string)o.SelectToken("button1.b1url");
                    Button2TextBox.Text = (string)o.SelectToken("button2.b2text");
                    Button2URLTextBox.Text = (string)o.SelectToken("button2.b2url");
                }
                catch (JsonReaderException)
                {
                    ContentDialog dialog = new ContentDialog();
                    dialog.Content = "Failed to load config.";
                    dialog.PrimaryButtonText = "OK";
                    dialog.XamlRoot = Content.XamlRoot;

                    await dialog.ShowAsync();
                }
            }
        }

        private async void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = "About RPC-Tool",
                Content = new AboutContentDialogContent(),
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (openedFile == null)
            {
                SaveAsButton_Click(sender, e);
                return;
            }

            CachedFileManager.DeferUpdates(openedFile);
            await FileIO.WriteTextAsync(openedFile, ToJson());
            await CachedFileManager.CompleteUpdatesAsync(openedFile);
        }
    }
}
