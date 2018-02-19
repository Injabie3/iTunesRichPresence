using System;
using System.Windows.Forms;
using System.Text;
using iTunesLib;

namespace iTunesRichPresence {
    public partial class MainForm : Form {
        private IiTunes _iTunes;
        private string _currentArtist;
        private string _currentTitle;
        private ITPlayerState _currentState;
        public MainForm() {
            _currentArtist = "";
            _currentTitle = "";
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e) {
            InitializeDiscord();
            InitializeiTunes();
            pollTimer.Enabled = true;
        }

        private void InitializeDiscord() {
            DiscordRPC.EventHandlers handlers = new DiscordRPC.EventHandlers {
                readyCallback = HandleReadyCallback,
                errorCallback = HandleErrorCallback,
                disconnectedCallback = HandleDisconnectedCallback
            };

            // Use own Discord app.
            DiscordRPC.Discord_Initialize("350502362919600130", ref handlers, true, null);
        }

        private void HandleReadyCallback() {}

        private static void HandleErrorCallback(int errorCode, string message) { }
        private static void HandleDisconnectedCallback(int errorCode, string message) { }

        private void InitializeiTunes() {
            _iTunes = new iTunesApp();
        }
        
        private void UpdatePresence() {
            var presence = new DiscordRPC.RichPresence {details = $"Title: {_currentTitle}" };
            
            // Discord Rich Presence asset keys, configure on developer dashboard.
            presence.largeImageKey = "rem";
            presence.smallImageKey = "itunes";
            presence.largeImageText = "Rem is best girl :3";
            presence.smallImageText = "iTunes";

            // For debugging purposes.
            Console.WriteLine($"{_currentArtist} - {_currentTitle}");

            //if (_iTunes.CurrentPlaylist.Kind == ITPlaylistKind.ITPlaylistKindUser) {
            //    presence.state = _iTunes.CurrentPlaylist.Name == "Music"
            //        ? $"Album: {_iTunes.CurrentTrack.Album}"
            //        : $"Playlist: {_iTunes.CurrentPlaylist.Name}";
            //}
            //else {
            //    presence.state = $"Album: {_iTunes.CurrentTrack.Album}";
            //}

            // Override the above
            presence.state = $"Artist: {_currentArtist}";

            if (_currentState != ITPlayerState.ITPlayerStatePlaying)
            {
                presence.state = "Paused.";
            }
            else {
                presence.startTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds() - _iTunes.PlayerPosition;
                presence.endTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds() + (_iTunes.CurrentTrack.Duration - _iTunes.PlayerPosition);
            }

            DiscordRPC.Discord_UpdatePresence(ref presence);
        }

        private void pollTimer_Tick(object sender, EventArgs e) {
            if (_iTunes.CurrentTrack == null) return;
            if (_currentArtist == _iTunes.CurrentTrack.Artist && _currentTitle == _iTunes.CurrentTrack.Name && _currentState == _iTunes.PlayerState) return;
            _currentArtist = _iTunes.CurrentTrack.Artist;
            _currentTitle = _iTunes.CurrentTrack.Name;
            _currentState = _iTunes.PlayerState;
            
            UpdatePresence();
        }
    }
}
