using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using RSSReader.Model;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RSSReader.Pages
{
    /// <summary>
    /// DriveManage.xaml の相互作用ロジック
    /// </summary>
    public partial class DriveManagePage : Page
    {
        const String resource = "https://graph.microsoft.com";
        const String clientid = "b3d24589-6b41-40af-92ce-43fb505ad4ea";
        const String redirecturi = "http://localhost";
        // ADFS 環境で SSO ドメイン以外のテナントのユーザーを試す場合はコメント解除
        // const string loginname = "admin@tenant.onmicrosoft.com";

        String AccessToken;

        public DriveManagePage()
        {
            InitializeComponent();
        }

        private async void Page_LoadedAsync(Object sender, RoutedEventArgs e)
        {
            AccessToken = await GetAccessToken(resource, clientid, redirecturi);
            if (String.IsNullOrEmpty(this.AccessToken)) {
                return;
            }
            DisplayFiles();
        }

        // アクセス トークン取得
        private async Task<String> GetAccessToken(String resource, String clientid, String redirecturi)
        {
            try {
                var authenticationContext = new Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext("https://login.microsoftonline.com/common");
                AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenAsync(
                    resource,
                    clientid,
                    new Uri(redirecturi),
                    new PlatformParameters(PromptBehavior.Auto, "https://login.microsoftonline.com/common/oauth2/nativeclient")
                    // ADFS 環境で SSO ドメイン以外のテナントのユーザーを試す場合はコメント解除
                    //, new UserIdentifier(loginname, UserIdentifierType.RequiredDisplayableId)            
                    );
                return authenticationResult.AccessToken;
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        // ファイル一覧表示
        private async void DisplayFiles()
        {
            using (var httpClient = new HttpClient()) {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    new Uri("https://graph.microsoft.com/v1.0/me/drive/root/children?$select=name,weburl,createdDateTime,lastModifiedDateTime")
                );
                var response = await httpClient.SendAsync(request);
                var files = JsonConvert.DeserializeObject<DriveFiles>(response.Content.ReadAsStringAsync().Result);

                FileListLB.Items.Clear();
                foreach (DriveFile file in files.Value) {
                    FileListLB.Items.Add(file.Name);
                }
            }
            //if (!String.IsNullOrEmpty(fileNameTB.Text)) {
            //    FileListLB.SelectedItem = fileNameTB.Text;
            //}
            Console.WriteLine();
        }

        private void LoginButton_Click(Object sender, RoutedEventArgs e)
        {

        }
    }
}
