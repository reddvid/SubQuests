using Microsoft.Services.Store.Engagement;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SubQuests.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        IsolatedStorageFile _storage = IsolatedStorageFile.GetUserStoreForApplication();
        public string quote, jsonstring;
        ObservableCollection<History> _historylist = new ObservableCollection<History>();
        int xBack = 0;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        Warning cd = new Warning();

        public MainPage()
        {
            this.InitializeComponent();

            MaximizeApp();

            btn_refresh.Visibility = Visibility.Collapsed;

            // Load History
            LoadHistory();

            // LOAD SUBNET QUESTION --- http://subnettingquestions.com/
            if (tb_q.Text.Contains("Sample"))
                LoadQuestion("http://subnettingquestions.com/");
        }

        private void MaximizeApp()
        {
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(600, 700));
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Maximized;

        }

        private async void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            xBack++;

            if (xBack == 1)
            {
                dispatcherTimer.Start();
                e.Handled = true;

                await cd.ShowAsync();
            }
            else
            {
                e.Handled = true;
                App.Current.Exit();
            }
        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            dispatcherTimer.Stop();
            xBack = 0;
            cd.Hide();
        }

        private void CheckHistory()
        {
            if (lv_history.Items.Count == 0)
            {
                this.FindName("tb_emptylist");
            }
            else
            {
                this.UnloadObject(tb_emptylist);
            }
        }

        private void LoadHistory()
        {
            // Load saved list
            if (_storage.FileExists("savedhistory"))
            {
                using (IsolatedStorageFileStream fileStream = _storage.OpenFile("savedhistory", FileMode.Open))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(ObservableCollection<History>));
                    _historylist = (ObservableCollection<History>)serializer.ReadObject(fileStream);
                }
            }

            // Uncomment this to delete any previous history - especially when blank or unwanted history is saved
            // Delete History 
            // _historylist.Clear();
            // Check if not mobile
            if (!IsMobile)
            {
                lv_history.ItemsSource = _historylist; // new ObservableCollection<History>(_historylist.GroupBy(p => p.q).Select(g => g.First()).ToList());
                CheckHistory();
            }
        }

        public static bool IsMobile
        {
            get
            {
                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
                return (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile");
            }
        }

        private async void LoadQuestion(string v)
        {
            btn_next.IsEnabled = false;
            var client = new HttpClient();

            try
            {
                HttpResponseMessage response = await client.GetAsync(v);
                HttpContent responseContent = response.Content;
                using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                {
                    // Write the output
                    jsonstring = await reader.ReadToEndAsync();
                }

                string v1 = "<p class=\"question\"><b>Question: </b><i>";
                string v2 = "</i></p>";

                string x1 = "<p><b>Answer: </b>";
                string x2 = "</p>";

                tb_q.Text = getBetween(jsonstring, v1, v2).Replace("&nbsp;", " "); // + "\" - " + getBetween(_jsonString, x1, x2);
                tb_ans.Text = getBetween(jsonstring, x1, x2);

                // Enable buttons
                ShowButtons();

            }
            catch (Exception ex)
            {
                // Make your own quote              
                tb_q.Text = "Something went wrong: " + ex.Message.ToString();
            }

            g_answer.Visibility = Visibility.Collapsed;

            if (tb_q.Text.Contains("wrong"))
            {
                // hide buttons
                HideButtons();
            }
            else
            {
                // show buttons
                ShowButtons();
            }
        }

        private void HideButtons()
        {
            // btn_showans.IsEnabled = btn_next.IsEnabled = false;
            btn_showans.Visibility = btn_next.Visibility = Visibility.Collapsed;
            btn_refresh.Visibility = Visibility.Visible;
        }

        private void ShowButtons()
        {
            btn_showans.IsEnabled = btn_next.IsEnabled = true;
            btn_showans.Visibility = btn_next.Visibility = Visibility.Visible;
            btn_refresh.Visibility = Visibility.Collapsed;
        }

        private void AddandSaveList(string questionString, string answerString)
        {
            try
            {
                Debug.WriteLine("Saving item...");
                if (!questionString.Contains("Something went wrong") || !questionString.Contains("Sample question"))
                {
                    if (!_historylist.Any(x => x.q.Equals(questionString)))
                    {
                        Debug.WriteLine(questionString);

                        _historylist.Insert(0, new History() { q = questionString, a = answerString });

                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (!IsMobile)
                lv_history.ItemsSource = _historylist;  // new ObservableCollection<History>(_historylist.GroupBy(p => p.q).Select(g => g.First()).ToList());

            // Now save the list
            if (_historylist.Count != 0)
            {
                if (_storage.FileExists("savedhistory"))
                {
                    _storage.DeleteFile("savedhistory");
                }

                using (IsolatedStorageFileStream fileStream = _storage.OpenFile("savedhistory", FileMode.Create))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(ObservableCollection<History>));
                    serializer.WriteObject(fileStream, _historylist);
                }
            }


        }

        private string getBetween(string jsonString, string v1, string v2)
        {
            int _start, _end;
            if (jsonstring.Contains(v1) && jsonstring.Contains(v2))
            {
                _start = jsonstring.IndexOf(v1, 0) + v1.Length;
                _end = jsonstring.IndexOf(v2, _start);
                return jsonString.Substring(_start, _end - _start);
            }
            else
            {
                return "";
            }
        }

        private void btn_showans_Click(object sender, RoutedEventArgs e)
        {
            if (btn_showans.Content.ToString().Contains("Show"))
            {
                Debug.WriteLine("Showed answer...");

                // Save to history
                AddandSaveList(tb_q.Text, tb_ans.Text);

                // Show answer panel then change content to hide
                g_answer.Visibility = Visibility.Visible;
                btn_showans.Content = "Hide answer";
            }
            else
            {
                // Hide answer then change back to show
                g_answer.Visibility = Visibility.Collapsed;
                btn_showans.Content = "Show answer";
            }
        }

        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Text changed...");

            // Save to history
            AddandSaveList(tb_q.Text, tb_ans.Text);

            // Refresh question 
            // LOAD SUBNET QUESTION --- http://subnettingquestions.com/
            LoadQuestion("http://subnettingquestions.com/");

            // Hide answer then change back to show
            g_answer.Visibility = Visibility.Collapsed;
            btn_showans.Content = "Show answer";
        }

        private async void btn_website_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://subnettingquestions.com/"));
        }

        private async void btn_websiteauthor_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://www.nobay.co.uk/"));
        }

        private async void btn_dev_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:Publisher?name=Red David"));
        }

        private async void btn_twitter_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.twitter.com/reddvidapps"));
        }

        private async void btn_outlook_Click(object sender, RoutedEventArgs e)
        {
            EmailMessage em = new EmailMessage();
            em.To.Add(new EmailRecipient("redappsupport@outlook.com"));
            em.Subject = "[FEEDBACK] SubQuests UWP";
            await EmailManager.ShowComposeNewEmailAsync(em);
        }

        private async void btn_fb_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.facebook.com/reddvidapps/"));
        }

        private async void btn_donate_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.buymeacoffee.com/redDavid"));
        }

        private async void btn_feedback_Click(object sender, RoutedEventArgs e)
        {
            if (StoreServicesFeedbackLauncher.IsSupported())
            {
                // Launch feedback
                var launcher = StoreServicesFeedbackLauncher.GetDefault();
                await launcher.LaunchAsync();
            }
            else
            {
                DeviceInfo _di = new DeviceInfo();
                // via email
                EmailMessage emailMessage = new EmailMessage();
                emailMessage.To.Add(new EmailRecipient("redappsupport@outlook.com"));
                emailMessage.Body = "SubQuests " + _di.appversion + "\n" + _di.devname + "\n" + _di.devmanufacturer + " " + _di.devmodel + "\n" + _di.sysfam + " " + _di.sysversion + " " + _di.sysarch + "\n\n\n";
                emailMessage.Subject = "[FEEDBACK] SubQuests";
                await EmailManager.ShowComposeNewEmailAsync(emailMessage);
            }
        }

        private void lv_history_LayoutUpdated(object sender, object e)
        {

        }

        private void btn_history_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HistoryPage));
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width >= 1008)
            {
                HideHistoryBtn.Margin = new Thickness(0, 40, 64, 0);
                lv_history.Margin = new Thickness(12, 0, 64, 64);
                HistoryHeader.Margin = new Thickness(12, 64, 0, 32);
                ContentView.Padding = AboutPanel.Margin = new Thickness(64);
                MainSplitView.DisplayMode = SplitViewDisplayMode.Inline;
                ToggleHistoryBtn.IsChecked = true;
                HideHistoryBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                HideHistoryBtn.Margin = new Thickness(0, 32, 24, 0);
                lv_history.Margin = new Thickness(12, 0, 24, 24);
                HistoryHeader.Margin = new Thickness(12, 32, 0, 24);
                AboutPanel.Margin = new Thickness(24);
                ContentView.Padding = new Thickness(24, 32, 24, 24);
                ToggleHistoryBtn.IsChecked = false;
                MainSplitView.DisplayMode = SplitViewDisplayMode.Overlay;
            }

            if (e.NewSize.Height >= 720 && e.NewSize.Width >= 1008)
            {
                HideHistoryBtn.Margin = new Thickness(0, 40, 64, 0);
                lv_history.Margin = new Thickness(12, 0, 64, 64);
                HistoryHeader.Margin = new Thickness(12, 64, 0, 32);
                ContentView.Padding = AboutPanel.Margin = new Thickness(64);
            }
            else
            {
                HideHistoryBtn.Margin = new Thickness(0, 32, 24, 0);
                lv_history.Margin = new Thickness(12, 0, 24, 24);
                HistoryHeader.Margin = new Thickness(12, 32, 0, 24);
                AboutPanel.Margin = new Thickness(24);
                ContentView.Padding = new Thickness(24, 32, 24, 24);
            }
        }

        private void ToggleHistoryBtn_Checked(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = true;
            
            if (RootGrid.ActualWidth >= 1200)
            {
                MainSplitView.OpenPaneLength = 480;
                MainSplitView.DisplayMode = SplitViewDisplayMode.Inline;

                HideHistoryBtn.Visibility = Visibility.Collapsed;
            }
            else if (RootGrid.ActualWidth >= 1008 && RootGrid.ActualWidth < 1200)
            {
                MainSplitView.OpenPaneLength = 320;
                MainSplitView.DisplayMode = SplitViewDisplayMode.Inline;

                HideHistoryBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                MainSplitView.OpenPaneLength = 320;
                HideHistoryBtn.Visibility = Visibility.Visible;

                MainSplitView.DisplayMode = SplitViewDisplayMode.Overlay;
            }
        }

        private void ToggleHistoryBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = false;
        }

        private void Lv_history_Loaded(object sender, RoutedEventArgs e)
        {
            CheckHistory();
        }

        private void Tb_q_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Lv_history_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as History;
            if (item != null)
            {
                tb_q.Text = item.q;
                tb_ans.Text = item.a;
            }

            // Show answer panel
            g_answer.Visibility = Visibility.Visible;
            btn_showans.Content = "Hide answer";
        }

        private void HideHistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            ToggleHistoryBtn.IsChecked = false;
        }

        private void MainSplitView_PaneClosed(SplitView sender, object args)
        {
            ToggleHistoryBtn.IsChecked = false;
        }

        private async void btn_git_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://reddvid.github.io"));
        }

        private void btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            // Refresh question
            // LOAD SUBNET QUESTION --- http://subnettingquestions.com/
            LoadQuestion("http://subnettingquestions.com/");
        }
    }
}
