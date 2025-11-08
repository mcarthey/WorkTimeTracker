using System;
using System.Windows.Media;
using System.Windows.Threading;

namespace WorkTimeTracker.Helpers
{
    public static class BrushAnimator
    {
        public static void AnimateBrush(SolidColorBrush brush, Color to, double durationSeconds = 0.2, bool hold = false)
        {
            if (brush == null) return;

            var from = brush.Color;
            var start = DateTime.Now;
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            timer.Tick += (s, _) =>
            {
                var t = (DateTime.Now - start).TotalSeconds / durationSeconds;
                if (t >= 1.0)
                {
                    brush.Color = to;
                    timer.Stop();

                    // Optional: fade back to white after a pause (for stop pulse)
                    if (hold)
                    {
                        var delay = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.5) };
                        delay.Tick += (s2, _) =>
                        {
                            delay.Stop();
                            AnimateBrush(brush, Colors.White, 0.4);
                        };
                        delay.Start();
                    }
                }
                else
                {
                    brush.Color = Color.FromScRgb(
                        1f,
                        (float)(from.ScR + (to.ScR - from.ScR) * t),
                        (float)(from.ScG + (to.ScG - from.ScG) * t),
                        (float)(from.ScB + (to.ScB - from.ScB) * t));
                }
            };
            timer.Start();
        }
    }
}
