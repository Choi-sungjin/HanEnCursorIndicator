// Video Downloader for Windows
// Saves the video from a public Instagram / TikTok / X (Twitter) post link
// to the user's Videos\VideoDownloader folder.
//
// Built with the C# 5 compiler included in Windows (csc v4.0.30319),
// so this file must avoid C# 6+ syntax (no string interpolation, no "?.").

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoDownloader
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            // Windows 10/11 ships .NET 4.8 where TLS 1.2 is default, but set it
            // explicitly so the app also works on machines with older defaults.
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072; // Tls12

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    internal sealed class ExtractedVideo
    {
        public string DirectUrl;
        public string FileName;
        public string Platform;
    }

    internal sealed class ExtractionException : Exception
    {
        public ExtractionException(string message) : base(message) { }
    }

    internal static class Extractor
    {
        private const string UserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
            "(KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";

        private static readonly Regex InstagramPattern =
            new Regex(@"instagram\.com/(?:reel|p|tv)/([A-Za-z0-9_-]+)", RegexOptions.IgnoreCase);
        private static readonly Regex TikTokHostPattern =
            new Regex(@"tiktok\.com", RegexOptions.IgnoreCase);
        private static readonly Regex TikTokVideoIdPattern =
            new Regex(@"/video/(\d+)");
        private static readonly Regex XPattern =
            new Regex(@"(?:twitter|x)\.com/([^/?]+)/status/(\d+)", RegexOptions.IgnoreCase);

        public static ExtractedVideo Extract(string rawUrl)
        {
            string url = (rawUrl ?? "").Trim();
            if (url.Length == 0)
            {
                throw new ExtractionException("링크를 입력해 주세요.");
            }
            if (InstagramPattern.IsMatch(url))
            {
                return ExtractInstagram(url);
            }
            if (TikTokHostPattern.IsMatch(url))
            {
                return ExtractTikTok(url);
            }
            if (XPattern.IsMatch(url))
            {
                return ExtractX(url);
            }
            throw new ExtractionException(
                "지원하지 않는 링크예요. Instagram, TikTok, X(Twitter) 링크만 지원합니다.");
        }

        // Instagram: the public embed page exposes og:video / video_url for public posts.
        private static ExtractedVideo ExtractInstagram(string url)
        {
            string shortcode = InstagramPattern.Match(url).Groups[1].Value;
            string html = HttpGetString(
                "https://www.instagram.com/p/" + shortcode + "/embed/captioned/");

            string videoUrl = null;
            Match og = Regex.Match(html,
                "<meta[^>]+property=[\"']og:video[\"'][^>]+content=[\"']([^\"']+)[\"']",
                RegexOptions.IgnoreCase);
            if (og.Success)
            {
                videoUrl = WebUtility.HtmlDecode(og.Groups[1].Value);
            }
            else
            {
                Match m = Regex.Match(html, "\"video_url\":\"([^\"]+)\"");
                if (m.Success)
                {
                    videoUrl = UnescapeJsonUrl(m.Groups[1].Value);
                }
            }

            if (string.IsNullOrEmpty(videoUrl))
            {
                throw new ExtractionException(
                    "동영상을 찾지 못했어요. 비공개 계정이거나 동영상이 없는 게시물일 수 있어요.");
            }
            return new ExtractedVideo
            {
                DirectUrl = videoUrl,
                FileName = "instagram_" + shortcode + ".mp4",
                Platform = "Instagram"
            };
        }

        // TikTok: parse the watch page's embedded JSON, then fall back to the
        // community tikwm.com mirror API if the page structure has changed.
        private static ExtractedVideo ExtractTikTok(string url)
        {
            string resolvedUrl = ResolveRedirect(url);
            Match idMatch = TikTokVideoIdPattern.Match(resolvedUrl);
            string videoId = idMatch.Success
                ? idMatch.Groups[1].Value
                : DateTime.Now.Ticks.ToString();

            ExtractedVideo fromPage = TryExtractTikTokFromPage(resolvedUrl, videoId);
            if (fromPage != null)
            {
                return fromPage;
            }
            ExtractedVideo fromMirror = TryExtractTikTokFromMirror(resolvedUrl, videoId);
            if (fromMirror != null)
            {
                return fromMirror;
            }
            throw new ExtractionException(
                "동영상을 찾지 못했어요. TikTok 페이지 구조가 바뀌었거나 비공개 영상일 수 있어요.");
        }

        private static ExtractedVideo TryExtractTikTokFromPage(string url, string videoId)
        {
            try
            {
                string html = HttpGetString(url);
                Match data = Regex.Match(html,
                    "<script id=\"__UNIVERSAL_DATA_FOR_REHYDRATION__\"[^>]*>(.*?)</script>",
                    RegexOptions.Singleline);
                if (!data.Success)
                {
                    return null;
                }
                string json = data.Groups[1].Value;
                Match addr = Regex.Match(json, "\"playAddr\":\"([^\"]+)\"");
                if (!addr.Success)
                {
                    addr = Regex.Match(json, "\"downloadAddr\":\"([^\"]+)\"");
                }
                if (!addr.Success)
                {
                    return null;
                }
                return new ExtractedVideo
                {
                    DirectUrl = UnescapeJsonUrl(addr.Groups[1].Value),
                    FileName = "tiktok_" + videoId + ".mp4",
                    Platform = "TikTok"
                };
            }
            catch
            {
                return null;
            }
        }

        private static ExtractedVideo TryExtractTikTokFromMirror(string url, string videoId)
        {
            try
            {
                string body = HttpGetString(
                    "https://www.tikwm.com/api/?url=" + Uri.EscapeDataString(url));
                Match code = Regex.Match(body, "\"code\"\\s*:\\s*(-?\\d+)");
                if (!code.Success || code.Groups[1].Value != "0")
                {
                    return null;
                }
                Match play = Regex.Match(body, "\"play\"\\s*:\\s*\"([^\"]+)\"");
                if (!play.Success)
                {
                    return null;
                }
                string playUrl = UnescapeJsonUrl(play.Groups[1].Value);
                if (!playUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    playUrl = "https://www.tikwm.com" + playUrl;
                }
                return new ExtractedVideo
                {
                    DirectUrl = playUrl,
                    FileName = "tiktok_" + videoId + ".mp4",
                    Platform = "TikTok"
                };
            }
            catch
            {
                return null;
            }
        }

        // X: the open-source vxtwitter.com mirror re-serves public tweet metadata
        // (including direct video URLs) as JSON without requiring a login.
        private static ExtractedVideo ExtractX(string url)
        {
            Match m = XPattern.Match(url);
            string username = m.Groups[1].Value;
            string tweetId = m.Groups[2].Value;

            string body = HttpGetString(
                "https://api.vxtwitter.com/" + username + "/status/" + tweetId);

            Match media = Regex.Match(body, "\"mediaURLs\"\\s*:\\s*\\[(.*?)\\]",
                RegexOptions.Singleline);
            string videoUrl = null;
            if (media.Success)
            {
                foreach (Match item in Regex.Matches(media.Groups[1].Value, "\"([^\"]+)\""))
                {
                    string candidate = UnescapeJsonUrl(item.Groups[1].Value);
                    if (candidate.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                    {
                        videoUrl = candidate;
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(videoUrl))
            {
                throw new ExtractionException("이 게시물에는 동영상이 없어요.");
            }
            return new ExtractedVideo
            {
                DirectUrl = videoUrl,
                FileName = "x_" + tweetId + ".mp4",
                Platform = "X (Twitter)"
            };
        }

        private static string UnescapeJsonUrl(string value)
        {
            return value
                .Replace("\\u0026", "&")
                .Replace("\\u002F", "/")
                .Replace("\\/", "/");
        }

        private static string ResolveRedirect(string url)
        {
            try
            {
                HttpWebRequest request = CreateRequest(url);
                request.Method = "HEAD";
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return response.ResponseUri.ToString();
                }
            }
            catch
            {
                return url;
            }
        }

        private static HttpWebRequest CreateRequest(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = UserAgent;
            request.AllowAutoRedirect = true;
            request.Timeout = 30000;
            request.ReadWriteTimeout = 30000;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            return request;
        }

        private static string HttpGetString(string url)
        {
            HttpWebRequest request = CreateRequest(url);
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse errorResponse = ex.Response as HttpWebResponse;
                if (errorResponse != null)
                {
                    throw new ExtractionException(string.Format(
                        "페이지를 불러오지 못했어요 (HTTP {0}). 비공개 게시물일 수 있어요.",
                        (int)errorResponse.StatusCode));
                }
                throw new ExtractionException("네트워크 오류: " + ex.Message);
            }
        }

        public static void DownloadToFile(
            ExtractedVideo video, string filePath, Action<int> onProgress, CancellationToken token)
        {
            HttpWebRequest request = CreateRequest(video.DirectUrl);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream input = response.GetResponseStream())
            using (FileStream output = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                long total = response.ContentLength;
                byte[] buffer = new byte[64 * 1024];
                long copied = 0;
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    token.ThrowIfCancellationRequested();
                    output.Write(buffer, 0, read);
                    copied += read;
                    if (total > 0 && onProgress != null)
                    {
                        onProgress((int)(copied * 100 / total));
                    }
                }
            }
        }
    }

    internal sealed class MainForm : Form
    {
        private readonly TextBox _urlBox;
        private readonly Button _pasteButton;
        private readonly Button _downloadButton;
        private readonly Button _openFolderButton;
        private readonly ProgressBar _progressBar;
        private readonly Label _statusLabel;
        private bool _busy;

        private static string SaveFolder
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                    "VideoDownloader");
            }
        }

        public MainForm()
        {
            Text = "영상 다운로더 - Instagram / TikTok / X";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(520, 210);
            Font = new Font("Segoe UI", 9F);

            Label guide = new Label();
            guide.Text = "Instagram / TikTok / X(Twitter) 공개 게시물 링크를 붙여넣으세요.";
            guide.Location = new Point(16, 14);
            guide.AutoSize = true;
            Controls.Add(guide);

            _urlBox = new TextBox();
            _urlBox.Location = new Point(16, 42);
            _urlBox.Width = 488;
            Controls.Add(_urlBox);

            _pasteButton = new Button();
            _pasteButton.Text = "붙여넣기";
            _pasteButton.Location = new Point(16, 76);
            _pasteButton.Size = new Size(110, 30);
            _pasteButton.Click += OnPasteClick;
            Controls.Add(_pasteButton);

            _downloadButton = new Button();
            _downloadButton.Text = "다운로드";
            _downloadButton.Location = new Point(136, 76);
            _downloadButton.Size = new Size(110, 30);
            _downloadButton.Click += OnDownloadClick;
            Controls.Add(_downloadButton);

            _openFolderButton = new Button();
            _openFolderButton.Text = "저장 폴더 열기";
            _openFolderButton.Location = new Point(256, 76);
            _openFolderButton.Size = new Size(120, 30);
            _openFolderButton.Click += OnOpenFolderClick;
            Controls.Add(_openFolderButton);

            _progressBar = new ProgressBar();
            _progressBar.Location = new Point(16, 120);
            _progressBar.Width = 488;
            _progressBar.Height = 18;
            Controls.Add(_progressBar);

            _statusLabel = new Label();
            _statusLabel.Text = "대기 중";
            _statusLabel.Location = new Point(16, 150);
            _statusLabel.Size = new Size(488, 48);
            Controls.Add(_statusLabel);

            AcceptButton = _downloadButton;
        }

        private void OnPasteClick(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                _urlBox.Text = Clipboard.GetText().Trim();
            }
        }

        private void OnOpenFolderClick(object sender, EventArgs e)
        {
            Directory.CreateDirectory(SaveFolder);
            System.Diagnostics.Process.Start("explorer.exe", "\"" + SaveFolder + "\"");
        }

        private async void OnDownloadClick(object sender, EventArgs e)
        {
            if (_busy)
            {
                return;
            }
            string url = _urlBox.Text;
            SetBusy(true);
            _progressBar.Value = 0;

            try
            {
                SetStatus("링크 분석 중...");
                ExtractedVideo video = await Task.Run(delegate { return Extractor.Extract(url); });

                Directory.CreateDirectory(SaveFolder);
                string filePath = MakeUniquePath(Path.Combine(SaveFolder, video.FileName));

                SetStatus(video.Platform + " 동영상 다운로드 중...");
                await Task.Run(delegate
                {
                    Extractor.DownloadToFile(video, filePath, ReportProgress, CancellationToken.None);
                });

                _progressBar.Value = 100;
                SetStatus("저장 완료: " + filePath);
            }
            catch (ExtractionException ex)
            {
                SetStatus("오류: " + ex.Message);
            }
            catch (Exception ex)
            {
                SetStatus("오류: " + ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void ReportProgress(int percent)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<int>(ReportProgress), percent);
                return;
            }
            if (percent >= 0 && percent <= 100)
            {
                _progressBar.Value = percent;
                _statusLabel.Text = "다운로드 중... " + percent + "%";
            }
        }

        private static string MakeUniquePath(string path)
        {
            if (!File.Exists(path))
            {
                return path;
            }
            string dir = Path.GetDirectoryName(path);
            string name = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);
            for (int i = 2; ; i++)
            {
                string candidate = Path.Combine(dir, name + " (" + i + ")" + ext);
                if (!File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            _downloadButton.Enabled = !busy;
            _pasteButton.Enabled = !busy;
            _urlBox.Enabled = !busy;
        }

        private void SetStatus(string text)
        {
            _statusLabel.Text = text;
        }
    }
}
