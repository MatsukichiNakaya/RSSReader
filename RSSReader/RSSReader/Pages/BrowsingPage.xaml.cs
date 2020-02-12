using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace RSSReader.Pages
{
    /// <summary>
    /// BrowsingPage.xaml の相互作用ロジック
    /// </summary>
    public partial class BrowsingPage : Page
    {
        private readonly String COMMAND_NAME = "callMe";
        public BrowsingPage()
        {
            InitializeComponent();

            var axIWebBrowser2 = typeof(WebBrowser).GetProperty("AxIWebBrowser2",
                                    BindingFlags.Instance | BindingFlags.NonPublic);
            var comObj = axIWebBrowser2.GetValue(this.RSSBrowser, null);
            comObj.GetType().InvokeMember("Silent", BindingFlags.SetProperty,
                                            null, comObj, new Object[] { true });

            this.RSSBrowser.MessageHook += this.RSSBrowser_MessageHook;

            if(null != App.BrowseURL) {
                this.URLBlock.Text = App.BrowseURL.AbsoluteUri;
                String html = String.Empty;
                var wc = new WebClient();
                try {
                    wc.Encoding = Encoding.UTF8;
                    html = wc.DownloadString(this.URLBlock.Text);
                }
                catch (WebException exc) {
                    Console.WriteLine(exc.Message);
                }
                if (String.IsNullOrEmpty(html)) {
                    return;
                }

                Int32 index = html.IndexOf("</head>");
                if (0 < index) {
                    var script = Project.IO.TextFile.Read(@".\Dat\script.js");
                    if (String.IsNullOrEmpty(script)) { return; }
                    html = html.Insert(index,
                        $@"<script>function {COMMAND_NAME}(){script} document.myfunc={COMMAND_NAME};</script>");
                    //html = html.Insert(index,
                    //    "<Style type=\"text/css\">*{bcolor:#333333 !important:background:#FFFFFF !important:}img,embed,iframe,object{display:none:}</Style>");
                }
                this.RSSBrowser.NavigateToString(html);
            }
        }

        private IntPtr RSSBrowser_MessageHook(IntPtr hwnd, Int32 msg, IntPtr wParam,
                                                IntPtr lParam, ref Boolean handled)
        {
            return IntPtr.Zero;
        }

        private void RSSBrowser_Navigating(Object sender, NavigatingCancelEventArgs e)
        {

        }

        private void RSSBrowser_LoadCompleted(Object sender, NavigationEventArgs e)
        {
            this.ScriptButton.IsEnabled = true;
        }

        private void ScriptButton_Click(Object sender, RoutedEventArgs e)
        {
            try {
                this.RSSBrowser.InvokeScript(COMMAND_NAME);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
