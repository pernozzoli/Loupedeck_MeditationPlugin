using System;
using System.Globalization;
using System.IO;
using System.Timers;

namespace Loupedeck.MeditationPlugin.Actions
{
    public class StartMeditationCommand : PluginDynamicCommand
    {
        protected int Minutes { get; set; } = 5;
        public MeditationPlugin MeditationPlugin => this.Plugin as MeditationPlugin;

        protected DateTimeOffset _startTime;
        protected TimeSpan _timeElapsed = TimeSpan.Zero;
        protected bool _timerStarted = false;

        public StartMeditationCommand() : base("Start Meditation", "Starts a meditation with a given time", "Meditations")
        {
            this.MakeProfileAction("text;Meditation time:");
        }

        /// <summary>
        /// Run the command, given either a defined number of minutes or 5 minutes as default
        /// </summary>
        /// <param name="actionParameter">Action parameter containing meditation interval</param>
        protected override void RunCommand(String actionParameter)
        {
            int minutes = 0;
            if (int.TryParse(actionParameter, out minutes))
            {
                Minutes = minutes;
            }
            else
            {
                Minutes = 5;
            }

            /// Start gong
            MeditationPlugin.PlayGong();            _startTime = DateTimeOffset.Now;

            /// Get the meditation time set
            if (!_timerStarted)
            {
                _timerStarted = true;
                MeditationPlugin.Tick += this.MeditationPlugin_Tick;
            }
        }

        private void MeditationPlugin_Tick(Object sender, EventArgs e)
        {
            _timeElapsed = DateTimeOffset.Now - _startTime;
            if (_timeElapsed.TotalMinutes >= Minutes)
            {
                MeditationPlugin.Tick -= this.MeditationPlugin_Tick;
                /// Stop gong
                MeditationPlugin.PlayGong();
                _timerStarted = false;
            }

            this.ActionImageChanged("");
        }


        /// <summary>
        /// Gets the command image depending on if timer is running or paused / stopped
        /// </summary>
        /// <param name="actionParameter">Action parameter containing the meditation interval</param>
        /// <param name="imageSize">Icon image size</param>
        /// <returns>The bitmap image, including remaining time if meditation is running</returns>
        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            if (_timerStarted)
            {
                using (BitmapBuilder bitmapBuilder = new BitmapBuilder(imageSize))
                {
                    var fn = EmbeddedResources.FindFile("meditation_running_80.png");
                    BitmapImage runningIcon = EmbeddedResources.ReadImage(fn);
                    var x1 = bitmapBuilder.Width * 0.1;
                    var w = bitmapBuilder.Width * 0.8;
                    var y1 = bitmapBuilder.Height * 0.45;
                    var y2 = bitmapBuilder.Height * 0.65;
                    var h = bitmapBuilder.Height * 0.3;
                    bitmapBuilder.DrawImage(runningIcon);
                    bitmapBuilder.DrawText((TimeSpan.FromMinutes(Minutes) - _timeElapsed).ToString(@"mm\:ss"), (Int32)x1, (Int32)y2, (Int32)w, (Int32)h, BitmapColor.White, imageSize == PluginImageSize.Width90 ? 15 : 9, imageSize == PluginImageSize.Width90 ? 2 : 2);
                    //bitmapBuilder.DrawText("Test", (Int32)x1, (Int32)y2, (Int32)w, (Int32)h, BitmapColor.White, imageSize == PluginImageSize.Width90 ? 15 : 9, imageSize == PluginImageSize.Width90 ? 2 : 2);
                    return bitmapBuilder.ToImage();
                    //return runningIcon;
                }
            }
            else
            {
                var fn = EmbeddedResources.FindFile("meditation_paused_80.png");
                return EmbeddedResources.ReadImage(fn);
            }
        }
    }
}

