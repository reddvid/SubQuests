using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SubQuests
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HistoryPage : Page
    {
        IsolatedStorageFile _storage = IsolatedStorageFile.GetUserStoreForApplication();       
        ObservableCollection<History> _historylist = new ObservableCollection<History>();

        public HistoryPage()
        {
            this.InitializeComponent();

            // Load history
            CheckHistory();

            SystemNavigationManager.GetForCurrentView().BackRequested += HistoryPage_BackRequested;
        }

        private void HistoryPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                e.Handled = true;
                Frame.GoBack();
            }
        }

        private void CheckHistory()
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
            lv_history.ItemsSource = _historylist;

            if (lv_history.Items.Count == 0)
            {
                this.FindName("tb_emptylist");
            }
            else
            {
                this.UnloadObject(tb_emptylist);
            }
        }
    }
}
