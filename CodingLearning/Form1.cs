using FireSharp.Config;
using FireSharp.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodingLearningApp
{
    public partial class Form1 : Form
    {
        // 1. Firebase 설정
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "PVwy4h7nZTckjHmtmKILg3Oyy6cCT2o7kcGgayWq",
            BasePath = "https://codinglearning-d9a11-default-rtdb.asia-southeast1.firebasedatabase.app/"
        };

        IFirebaseClient client;
        HttpClient httpClient = new HttpClient();
        string userId = "user123"; // 임시 사용자 ID

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Firebase 및 WebView2 초기화
            InitializeFirebase();
            await InitializeWebView();
        }

        private void InitializeFirebase()
        {
            if (client == null)
            {
                client = new FireSharp.FirebaseClient(config);
            }
        }

        private async Task InitializeWebView()
        {
            try
            {
                // WebView2 환경 초기화
                await webView.EnsureCoreWebView2Async(null);

                // 페이지 로드 완료 시 불필요한 요소(광고, 메뉴 등) 숨기기
                webView.CoreWebView2.NavigationCompleted += (s, e) =>
                {
                    string script = @"
                        document.querySelector('.navbar').style.display = 'none'; 
                        document.querySelector('#footer').style.display = 'none';
                        document.querySelector('.breadcrumb').style.display = 'none';
                        document.querySelector('.problem-menu').style.display = 'none';
                    ";
                    webView.CoreWebView2.ExecuteScriptAsync(script);
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show("WebView2 초기화 실패: " + ex.Message);
            }
        }

        // 2. solved.ac API 연동 및 문제 내용 표시 (기획서 순서 2)
        private async Task GetAndCacheProblem(string problemId)
        {
            InitializeFirebase();
            try
            {
                // solved.ac API 호출
                string url = $"https://solved.ac/api/v3/problem/show?problemId={problemId}";
                if (httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "C# App");

                string response = await httpClient.GetStringAsync(url