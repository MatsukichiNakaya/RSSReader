using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Project.DataBase;
using RSSReader.Model;
using Project.Serialization.Xml;

namespace RSSReader.Pages
{
    /// <summary>
    /// ConfigurePage.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfigurePage : Page
    {
        private FeedViewPage InnerViewPage { get; set; }

        public ConfigurePage(FeedViewPage inner)
        {
            InitializeComponent();

            var conf = XmlSerializer.Load<RssConfigure>(Define.XML_PATH);
            this.ConfGrid.DataContext = conf;


            this.InnerViewPage = inner;
        }

        private void ChashDelButton_Click(Object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Cancel 
                == MessageBox.Show("Delete unnecessary files?")) { return; }

        }

        private void ReturnButton_Click(Object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(this.InnerViewPage);
        }

        private void AppendButton_Click(Object sender, RoutedEventArgs e)
        {
            if (!Int32.TryParse(this.UpdateBox.Text, out Int32 span)) { return; }
            if (span < 5) { return; }

            var conf = new RssConfigure() {
                BrowserOption = this.BrowsBox.Text,
                UpdateSpan = span,
                IsShowImage = false,
            };
            XmlSerializer.Save(conf, XML_PATH);
        }
    }
}
