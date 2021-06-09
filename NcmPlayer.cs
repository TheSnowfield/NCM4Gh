using System;
using System.Text;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCM4Gh
{
    public struct NcmMusicInfo
    {
        public string Artist;
        public string Album;
        public string WndTitle;
        public float Progress;
    }

    public static class NcmPlayer
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindowW(string lpClassName, IntPtr lpWindowName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr GetWindowTextW(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        private static uint wndHandle_;
        private static uint wndFailture_;
        private static NcmMusicInfo? nowPlaying_;

        /// <summary>
        /// Get now playing
        /// </summary>
        public static NcmMusicInfo? NowPlaying
            => nowPlaying_ ?? GetNowPlaying();

        /// <summary>
        /// Track ncm now playing
        /// </summary>
        /// <returns></returns>
        private static NcmMusicInfo? GetNowPlaying()
        {
            // Check if handle is null
            if (wndHandle_ == 0)
            {
                // Search the main window
                wndHandle_ = TrackNCMWindow();

                // Still nothing
                if (wndHandle_ == 0)
                {
                    ++wndFailture_;
                    return null;
                }
            }

            // Get window title
            var _wndTitle = new StringBuilder(512);
            var _result = GetWindowTextW((IntPtr)wndHandle_, _wndTitle, _wndTitle.Capacity);

            if (_result == IntPtr.Zero)
            {
                ++wndFailture_;
                return null;
            }

            // Hitted the cache
            var _title = _wndTitle.ToString();
            if (nowPlaying_?.WndTitle == _title)
            {
                return nowPlaying_;
            }

            // Request api
            var _searchRsult = GetPlayingInformation(_title).Result;

            // Todo
            nowPlaying_ = new NcmMusicInfo
            {
                Album = "",
                Artist = "",
                Progress = 0.0f,
                WndTitle = _title
            };

            return nowPlaying_;
        }

        /// <summary>
        /// Track ncm main window handle
        /// </summary>
        /// <returns></returns>
        private static uint TrackNCMWindow()
        {
            // Find main window
            var hwnd_ = FindWindowW("OrpheusBrowserHost", IntPtr.Zero);
            {
                // The window exists
                if (hwnd_ != IntPtr.Zero)
                {
                    return (uint)hwnd_;
                }
            }

            return 0;
        }

        private class NcmSearchResult
        {
            public int code { get; set; }

            public NcmSearchSongs result { get; set; }
        }

        private struct NcmSearchSongs
        {
            public int songCount { get; set; }

            public NcmSearchSong[] songs { get; set; }
        }

        private struct NcmSearchSong
        {
            public string name { get; set; }

            public uint id { get; set; }

            public uint duration { get; set; }
        }

        /// <summary>
        /// Get specific song info
        /// </summary>
        private static async Task<NcmSearchResult> GetPlayingInformation(string searchKeyword)
        {
            using var _request = new HttpClient();
            {
                var _json = await _request.GetStringAsync
                    ($"https://music.163.com/api/search/pc?s={searchKeyword}&type=1");
                {
                    return JsonSerializer.Deserialize<NcmSearchResult>(_json);
                }
            }
        }

        private struct NcmLyricInfo
        {

        }
    }
}
