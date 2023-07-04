﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using vrcosc_magicchatbox.Classes;
using vrcosc_magicchatbox.Classes.DataAndSecurity;
using vrcosc_magicchatbox.DataAndSecurity;
using vrcosc_magicchatbox.ViewModels;

namespace vrcosc_magicchatbox
{
    public partial class MainWindow : Window
    {
        public float samplingTime = 1;
        public static readonly DependencyProperty ShadowOpacityProperty = DependencyProperty.Register("ShadowOpacity", typeof(double), typeof(MainWindow), new PropertyMetadata(0.0));

        DispatcherTimer backgroundCheck = new DispatcherTimer();
        private System.Timers.Timer pauseTimer;
        private System.Timers.Timer typingTimer;
        private static List<CancellationTokenSource> _activeCancellationTokens = new List<CancellationTokenSource>();
        private static double _shadowOpacity;
        public static double ShadowOpacity
        {
            get => _shadowOpacity;
            set
            {
                if (_shadowOpacity != value)
                {
                    _shadowOpacity = value;
                    ShadowOpacityChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        public static event EventHandler ShadowOpacityChanged;



        public MainWindow()
        {
            Closing += SaveDataToDisk;
            InitializeComponent();

            DispatcherTimer backgroundCheck = new DispatcherTimer();

            Closing += SaveDataToDisk;
            InitializeComponent();

            backgroundCheck = new DispatcherTimer();
            backgroundCheck.Tick += Timer;

            // Here we set the interval by multiplying ScanInterval by 1000 to get milliseconds
            backgroundCheck.Interval = TimeSpan.FromMilliseconds(ViewModel.Instance.ScanningInterval * 1000);

            backgroundCheck.Start();

            SelectTTS();
            SelectTTSOutput();
            ChangeMenuItem(ViewModel.Instance.CurrentMenuItem);
            scantick();
        }

        public void SelectTTS()
        {
            foreach (var voice in TikTokTTSVoices_combo.Items)
            {
                if (voice is Voice && (voice as Voice).ApiName == ViewModel.Instance.SelectedTikTokTTSVoice?.ApiName)
                {
                    TikTokTTSVoices_combo.SelectedItem = voice;
                    break;
                }
            }
        }

        public void SelectTTSOutput()
        {
            foreach (var AudioDevice in PlaybackOutputDeviceComboBox.Items)
            {
                if (AudioDevice is AudioDevice && (AudioDevice as AudioDevice).FriendlyName == ViewModel.Instance.SelectedPlaybackOutputDevice?.FriendlyName)
                {
                    PlaybackOutputDeviceComboBox.SelectedItem = AudioDevice;
                    break;
                }
            }
        }



        private void SaveDataToDisk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                this.Hide();
                DataController.ManageSettingsXML(true);
                DataController.SaveAppList();
                DataController.SaveMediaSessions();
                System.Environment.Exit(1);
            }
            catch (Exception ex)
            {

                Logging.WriteException(ex, makeVMDump: true, MSGBox: true, exitapp: true);
            }

        }

        private void Timer(object sender, EventArgs e)
        {
            if (ViewModel.Instance.ScanPause)
            {
                if (pauseTimer == null)
                {
                    ViewModel.Instance.CountDownUI = false;
                    pauseTimer = new System.Timers.Timer();
                    pauseTimer.Interval = 1000;
                    pauseTimer.Elapsed += PauseTimer_Tick;
                    pauseTimer.Start();
                }
            }
            else
            {
                if (pauseTimer != null)
                {
                    pauseTimer.Stop();
                    pauseTimer = null;
                }

                ViewModel.Instance.CountDownUI = true;
                scantick();
            }

        }

        private void PauseTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                ViewModel.Instance.ScanPauseCountDown--;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ViewModel.Instance.ScanPauseCountDown = ViewModel.Instance.ScanPauseCountDown;
                });

                if (ViewModel.Instance.ScanPauseCountDown <= 0 || !ViewModel.Instance.ScanPause)
                {
                    ViewModel.Instance.ScanPause = false;
                    pauseTimer.Stop();
                    pauseTimer = null;
                    if (ViewModel.Instance.ScanPauseCountDown != 0)
                    {
                        ViewModel.Instance.ScanPauseCountDown = 0;
                    }
                    OSCController.ClearChat();
                    OSCSender.SendOSCMessage(false);
                    Timer(null, null);
                }
            }
            catch (Exception ex) { Logging.WriteException(ex, makeVMDump: false, MSGBox: false); }

        }

        public void scantick()
        {
            try
            {
                if (ViewModel.Instance.IntgrScanSpotify_OLD == true)
                {
                    ViewModel.Instance.PlayingSongTitle = SpotifyActivity.CurrentPlayingSong();
                    ViewModel.Instance.SpotifyActive = SpotifyActivity.SpotifyIsRunning();
                }
                if (ViewModel.Instance.IntgrScanWindowActivity == true)
                {
                    ViewModel.Instance.FocusedWindow = WindowActivity.GetForegroundProcessName();
                }

                ViewModel.Instance.IsVRRunning = WindowActivity.IsVRRunning();
                if (ViewModel.Instance.IntgrScanWindowTime == true)
                    ViewModel.Instance.CurrentTime = SystemStats.GetTime();
                //if (ViewModel.Instance.CheckOSCConnection == true)
                //    Task.Run(() => OscReceiver.CheckOSCConnection());
                ViewModel.Instance.ChatFeedbackTxt = "";
                OSCController.BuildOSC();
                OSCSender.SendOSCMessage(false);
            }
            catch (Exception ex)
            {
                Logging.WriteException(ex, makeVMDump: false, MSGBox: false);
            }

        }

        public static void ChangeMenuItem(int changeINT)
        {
            ViewModel.Instance.CurrentMenuItem = changeINT;
            ViewModel.Instance.MenuItem_0_Visibility = "Hidden";
            ViewModel.Instance.MenuItem_1_Visibility = "Hidden";
            ViewModel.Instance.MenuItem_2_Visibility = "Hidden";
            ViewModel.Instance.MenuItem_3_Visibility = "Hidden";
            if (ViewModel.Instance.CurrentMenuItem == 0)
            {
                ViewModel.Instance.MenuItem_0_Visibility = "Visible";
                return;
            }
            else if (ViewModel.Instance.CurrentMenuItem == 1)
            {
                ViewModel.Instance.MenuItem_1_Visibility = "Visible";
                return;
            }
            else if (ViewModel.Instance.CurrentMenuItem == 2)
            {
                ViewModel.Instance.MenuItem_2_Visibility = "Visible";
                return;
            }
            else if (ViewModel.Instance.CurrentMenuItem == 3)
            {
                ViewModel.Instance.MenuItem_3_Visibility = "Visible";
                return;
            }
            ChangeMenuItem(0);
        }


        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Instance.ScanPause != true)
            {
                OSCController.BuildOSC();
            }
        }

        private void Drag_area_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }


        private void Button_close_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            this.Close();
        }

        private void Button_minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void NewVersion_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.Instance.CanUpdate)
            {
                ViewModel.Instance.CanUpdate = false;
                Task.Run(() => UpdateApp.PrepareUpdate());
            }
            else
            {
                Process.Start("explorer", "http://github.com/BoiHanny/vrcosc-magicchatbox/releases");
            }

        }

        private void MasterSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Instance.MasterSwitch == true)
            {
                backgroundCheck.Start();
            }
            else
            {
                backgroundCheck.Stop();
            }

        }

        private void MenuButton_0_Click(object sender, RoutedEventArgs e)
        {
            ChangeMenuItem(0);
        }

        private void MenuButton_1_Click(object sender, RoutedEventArgs e)
        {
            ChangeMenuItem(1);
        }

        private void MenuButton_2_Click(object sender, RoutedEventArgs e)
        {
            ChangeMenuItem(2);
        }

        private void MenuButton_3_Click(object sender, RoutedEventArgs e)
        {
            ChangeMenuItem(3);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var item = button.Tag as StatusItem;
                if (item.msg.ToLower() == "sr4 series" || item.msg.ToLower() == "boihanny")
                {
                    ViewModel.Instance.Egg_Dev = false;
                    MessageBox.Show("damn u left the dev egggmoooodeee", "Egg", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                ViewModel.Instance.StatusList.Remove(item);
                ViewModel.SaveStatusList();
            }
            catch (Exception)
            {

            }
        }

        private void SortUsed_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Instance.StatusList = new ObservableCollection<StatusItem>(ViewModel.Instance.StatusList.OrderByDescending(x => x.LastUsed));
        }

        private void SortFav_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Instance.StatusList = new ObservableCollection<StatusItem>(ViewModel.Instance.StatusList.OrderByDescending(x => x.IsFavorite).ThenBy(x => x.LastUsed));
        }

        private void SortDate_Click(object sender, RoutedEventArgs e)
        {
            {
                ViewModel.Instance.StatusList = new ObservableCollection<StatusItem>(ViewModel.Instance.StatusList.OrderByDescending(x => x.CreationDate));
            }
        }

        private void FavBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    AddFav_Click(sender, e);
                }
                if (e.Key == Key.Escape)
                {
                    ViewModel.Instance.NewStatusItemTxt = "";
                }
            }
            catch (Exception ex)
            {
                Logging.WriteException(ex, makeVMDump: false, MSGBox: false);
            }
        }

        private void NewFavText_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            int count = textBox.Text.Count();
            ViewModel.Instance.StatusBoxCount = $"{count.ToString()}/140";
            if (count > 140)
            {
                int overmax = count - 140;
                ViewModel.Instance.StatusBoxColor = "#FFFF9393";
                ViewModel.Instance.StatusTopBarTxt = $"You're soaring past the 140 char limit by {overmax}. Reign in that message!";
            }
            else if (count == 0)
            {
                ViewModel.Instance.StatusBoxColor = "#FF504767";
                ViewModel.Instance.StatusTopBarTxt = $"";
            }
            else
            {
                ViewModel.Instance.StatusBoxColor = "#FF2C2148";
                if (count > 22)
                {
                    ViewModel.Instance.StatusTopBarTxt = $"Buckle up! Keep it tight to 20-25 or integrations may suffer.";
                }
                else
                {
                    ViewModel.Instance.StatusTopBarTxt = $"";
                }
            }
        }

        private void AddFav_Click(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            int randomId = random.Next(10, 99999999);
            bool IsActive = false;
            if (ViewModel.Instance.StatusList.Count() == 0)
            {
                IsActive = true;
            }

            if (ViewModel.Instance.NewStatusItemTxt.Count() > 0 && ViewModel.Instance.NewStatusItemTxt.Count() < 141)
            {
                ViewModel.Instance.StatusList.Add(new StatusItem { CreationDate = DateTime.Now, IsActive = IsActive, IsFavorite = false, msg = ViewModel.Instance.NewStatusItemTxt, MSGLenght = ViewModel.Instance.NewStatusItemTxt.Count(), MSGID = randomId });
                ViewModel.Instance.StatusList = new ObservableCollection<StatusItem>(ViewModel.Instance.StatusList.OrderByDescending(x => x.CreationDate));
                if (ViewModel.Instance.NewStatusItemTxt.ToLower() == "sr4 series" || ViewModel.Instance.NewStatusItemTxt.ToLower() == "boihanny")
                {
                    ViewModel.Instance.Egg_Dev = true;
                    MessageBox.Show("u found the dev egggmoooodeee", "Egg", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                ViewModel.Instance.NewStatusItemTxt = "";
                ViewModel.SaveStatusList();

            }
        }

        private void Favbutton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveStatusList();
        }

        private void ResetFavorites_Click(object sender, RoutedEventArgs e)
        {
            string xml = System.IO.Path.Combine(ViewModel.Instance.DataPath, "StatusList.xml");
            if (File.Exists(xml))
            {
                File.Delete(xml);
            }
            ViewModel.Instance.StatusList.Clear();
            DataAndSecurity.DataController.LoadStatusList();
            ViewModel.SaveStatusList();
            ChangeMenuItem(1);
        }

        private void NewChattingTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            int count = textBox.Text.Count();
            ViewModel.Instance.ChatBoxCount = $"{count.ToString()}/140";
            if (count > 140)
            {
                int overmax = count - 140;
                ViewModel.Instance.ChatBoxColor = "#FFFF9393";
                ViewModel.Instance.ChatTopBarTxt = $"You're soaring past the 140 char limit by {overmax}.";
            }
            else if (count == 0)
            {
                ViewModel.Instance.ChatBoxColor = "#FF504767";
                ViewModel.Instance.ChatTopBarTxt = $"";
            }
            else
            {
                ViewModel.Instance.ChatBoxColor = "#FF2C2148";
                ViewModel.Instance.ChatTopBarTxt = $"";

            }

            OSCSender.TypingIndicatorAsync(true);


            if (typingTimer != null)
            {
                typingTimer.Stop();
                typingTimer.Start();
            }
            else
            {
                typingTimer = new System.Timers.Timer(2000);
                typingTimer.Elapsed += (s, args) => OSCSender.TypingIndicatorAsync(false);
                typingTimer.AutoReset = false;
                typingTimer.Enabled = true;
            }

        }

        private void NewChattingTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ButtonChattingTxt_Click(sender, e);
            }
            if (e.Key == Key.Escape)
            {
                ViewModel.Instance.NewChattingTxt = "";
            }
        }

        private void ButtonChattingTxt_Click(object sender, RoutedEventArgs e)
        {
            string chat = ViewModel.Instance.NewChattingTxt;
            if (chat.Length > 0 && chat.Length <= 141 && ViewModel.Instance.MasterSwitch)
            {
                OSCController.CreateChat(true);
                OSCSender.SendOSCMessage(ViewModel.Instance.ChatFX);
                DataController.SaveChatList();
                if (ViewModel.Instance.TTSTikTokEnabled == true)
                {
                    if (DataAndSecurity.DataController.PopulateOutputDevices(true))
                    {
                        ViewModel.Instance.ChatFeedbackTxt = "Requesting TTS...";
                        TTSGOAsync(chat);
                    }
                    else
                    {
                        ViewModel.Instance.ChatFeedbackTxt = "Error setting output device.";
                    }

                }

                Timer(null, null);
                RecentScroll.ScrollToEnd();
            }
        }

        public static async Task TTSGOAsync(string chat, bool resent = false)
        {
            try
            {
                if (ViewModel.Instance.TTSCutOff)
                {
                    foreach (var tokenSource in _activeCancellationTokens)
                    {
                        tokenSource.Cancel();
                    }
                    _activeCancellationTokens.Clear();
                }


                byte[] audioFromApi = await TTSController.GetAudioBytesFromTikTokAPI(chat);
                if (audioFromApi != null)
                {
                    var cancellationTokenSource = new CancellationTokenSource();
                    _activeCancellationTokens.Add(cancellationTokenSource);
                    ViewModel.Instance.ChatFeedbackTxt = "TTS is playing...";
                    await TTSController.PlayTikTokAudioAsSpeech(cancellationTokenSource.Token, audioFromApi, ViewModel.Instance.SelectedPlaybackOutputDevice.DeviceNumber);
                    if (resent)
                    {
                        ViewModel.Instance.ChatFeedbackTxt = "Chat was sent again with TTS.";
                    }
                    else
                    {
                        ViewModel.Instance.ChatFeedbackTxt = "Chat was sent with TTS.";
                    }


                    _activeCancellationTokens.Remove(cancellationTokenSource);
                }
                else
                {
                    ViewModel.Instance.ChatFeedbackTxt = "Error getting TTS from online servers.";
                }

            }
            catch (OperationCanceledException ex)
            {
                ViewModel.Instance.ChatFeedbackTxt = "TTS cancelled";
                Logging.WriteException(ex, makeVMDump: true, MSGBox: false);
            }
            catch (Exception ex)
            {
                ViewModel.Instance.ChatFeedbackTxt = "Error sending a chat with TTS";
            }
        }




        private void StopChat_Click(object sender, RoutedEventArgs e)
        {
            OSCController.ClearChat();
            OSCSender.SendOSCMessage(false);
            Timer(null, null);
            foreach (var token in _activeCancellationTokens)
            {
                token.Cancel();
            }
        }

        private void ClearChat_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Instance.LastMessages.Clear();
            DataController.SaveChatList();
            StopChat_Click(null, null);
        }

        private void TikTokTTSVoices_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                var selectedVoice = comboBox.SelectedItem as Voice;
                if (selectedVoice != null)
                {
                    ViewModel.Instance.SelectedTikTokTTSVoice = selectedVoice;
                    ViewModel.Instance.RecentTikTokTTSVoice = selectedVoice.ApiName;
                }
            }

        }

        private void PlaybackOutputDeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.Instance.RecentPlayBackOutput = ViewModel.Instance.SelectedPlaybackOutputDevice.FriendlyName;
        }

        private void OpenLogFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataController.CreateIfMissing(ViewModel.Instance.LogPath))
                    Process.Start("explorer.exe", ViewModel.Instance.LogPath);
            }
            catch (Exception ex)
            {
                Logging.WriteException(ex);
            }
        }

        private void OpenConfigFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataController.CreateIfMissing(ViewModel.Instance.DataPath))
                    Process.Start("explorer.exe", ViewModel.Instance.DataPath);
            }
            catch (Exception ex)
            {

                Logging.WriteException(ex);
            }
        }

        private void MakeDataDump_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logging.ViewModelDump();
                Process.Start("explorer.exe", ViewModel.Instance.LogPath);
            }
            catch (Exception ex)
            {

                Logging.WriteException(ex);
            }
        }

        private void GitHubChanges_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.Instance.tagURL == null)
            {
                Process.Start("explorer", "http://github.com/BoiHanny/vrcosc-magicchatbox/releases");
            }
            else
            {
                Process.Start("explorer", ViewModel.Instance.tagURL);
            }

        }

        private void ToggleVoicebtn_Click(object sender, RoutedEventArgs e)
        {
            OSCSender.ToggleVoice(true);
        }

        private void LearnMoreAboutTTSbtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer", "https://github.com/BoiHanny/vrcosc-magicchatbox/wiki/Play-TTS-Output-of-MagicChatbox-to-Main-Audio-Device-and-Microphone-in-VRChat-Using-VB-Audio-Cable-(Simple-Setup)");
        }

        private void LearnMoreAboutIntelliChatAI_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer", "https://openai.com/product");
        }

        private void IntelliChatAIapiPage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer", "https://platform.openai.com/account/api-keys");
        }

        private async void OpenAIAPITestConnection_ClickAsync(object sender, RoutedEventArgs e)
        {
            //ChatModelMsg action = ViewModel.Instance.OpenAIAPIBuiltInActions.FirstOrDefault(a => a.FriendlyName == "Add Emojis");
            //ChatModelMsg response = await OpenAIClient.ExecuteActionAsync(action, ViewModel.Instance.NewChattingTxt);
            //ViewModel.Instance.NewChattingTxt = response.Content;
            ViewModel.Instance.OpenAIAPITestResponse = await OpenAIClient.TestAPIConnection();
        }


        private void OpenAITerms_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer", "https://openai.com/policies/plugin-terms");

        }

        private void IntgrIntelliWing_btn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Instance.IntgrIntelliWing = !ViewModel.Instance.IntgrIntelliWing;
        }

        private void LearnMoreAboutSpotifybtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer", "https://github.com/BoiHanny/vrcosc-magicchatbox/wiki/How-to-Use-the-Spotify-Integration-%F0%9F%8E%B5");
        }

        private void SmartClearnup_Click(object sender, RoutedEventArgs e)
        {
            int ItemRemoved = WindowActivity.SmartCleanup();
            if (ItemRemoved > 0)
            {
                ViewModel.Instance.DeletedAppslabel = $"Removed {ItemRemoved} apps from history";

            }
            else
            {
                ViewModel.Instance.DeletedAppslabel = $"No apps removed from history";
            }
        }

        private void ClearnupKeepSettings_Click(object sender, RoutedEventArgs e)
        {
            int ItemRemoved = WindowActivity.CleanAndKeepAppsWithSettings();
            if (ItemRemoved > 0)
            {
                ViewModel.Instance.DeletedAppslabel = $"Removed {ItemRemoved} apps from history";
            }
            else
            {
                ViewModel.Instance.DeletedAppslabel = $"No apps removed from history";
            }
        }

        private void ResetWindowActivity_Click(object sender, RoutedEventArgs e)
        {
            int ItemRemoved = WindowActivity.ResetWindowActivity();
            if (ItemRemoved > 0)
            {
                ViewModel.Instance.DeletedAppslabel = "All apps from history";
            }
        }

        private async Task ManualUpdateCheckAsync()
        {
            await Task.Run(() => DataController.CheckForUpdateAndWait(true));
        }

        private void CheckUpdateBtnn_Click(object sender, RoutedEventArgs e)
        {
            ManualUpdateCheckAsync();
        }

        private void Discord_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", "https://discord.gg/ZaSFwBfhvG");
        }

        private void Github_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", "https://github.com/BoiHanny/vrcosc-magicchatbox");
        }

        private void LearnMoreAboutHeartbtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer", "https://github.com/BoiHanny/vrcosc-magicchatbox/wiki/How-to-Set-Up-MagicChatbox-with-Pulsoid-for-VRChat-%F0%9F%92%9C");
        }
    }
}