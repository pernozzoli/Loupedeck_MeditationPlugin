namespace Loupedeck.MeditationPlugin
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices.ComTypes;
    using System.Threading;
    using System.Threading.Tasks;

    using NAudio.Wave;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    // This class contains the plugin-level logic of the Loupedeck plugin.

    public class MeditationPlugin : Plugin
    {
        // Gets a value indicating whether this is an Universal plugin or an Application plugin.
        public override Boolean UsesApplicationApiOnly => true;

        // Gets a value indicating whether this is an API-only plugin.
        public override Boolean HasNoApplication => true;

        public string GongPath
        {
            get
            {
                var pluginDataDirectory = this.GetPluginDataDirectory();
                var gongPath = Path.Combine(pluginDataDirectory, "gong.wav");

                if (IoHelpers.EnsureDirectoryExists(pluginDataDirectory))
                {
                    if (!File.Exists(gongPath))
                    {
                        Stream soundStream = EmbeddedResources.GetStream("Loupedeck.MeditationPlugin.Resources.bell-sound.wav");
                        using (Stream destination = File.Create(gongPath))
                        {
                            soundStream.CopyTo(destination);
                        }
                    }
                }

                return gongPath;
            }
        }


        public event EventHandler<EventArgs> Tick;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private async void Timer()
        {
            while (true && !this._cancellationTokenSource.IsCancellationRequested)
            {
                await Task.Delay(1000);
                Tick?.Invoke(this, new EventArgs());
            }

        }

        // This method is called when the plugin is loaded during the Loupedeck service start-up.
        public override void Load()
        {
            this.Timer();
        }

        // This method is called when the plugin is unloaded during the Loupedeck service shutdown.
        public override void Unload()
        {
            this._cancellationTokenSource.Cancel();
        }

        public void PlayGong()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "/usr/bin/afplay",
                Arguments = GongPath,
            };
            Process proc = new Process()
            {
                StartInfo = startInfo,
            };
            proc.Start();

            //using (Stream music = this.GongStream)
            //{
            //    music.Seek(0, SeekOrigin.Begin);
            //    System.Media.SoundPlayer soundPlayer = new System.Media.SoundPlayer(this.GongStream);
            //    soundPlayer.Play();
            //}
            //StreamMediaFoundationReader reader = new StreamMediaFoundationReader(this.GongStream);
            //using (var outputDevice = new WaveOutEvent())
            //{
            //    outputDevice.Init(reader);
            //    outputDevice.Play();
            //    //while (outputDevice.PlaybackState == PlaybackState.Playing)
            //    //{
            //    //    Thread.Sleep(1000);
            //    //}
            //}
        }
    }
}
