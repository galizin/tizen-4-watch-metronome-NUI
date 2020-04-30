using System;
using System.Runtime.InteropServices;
using Tizen.NUI;
using System.Diagnostics;
using Tizen.NUI.BaseComponents;
using Tizen.NUI.UIComponents;

namespace NUITemplate1
{
    class Program : NUIApplication
    {
        const uint maxrate = 240;
        uint rate = maxrate / 2;
        TextLabel text, textp, textm, textd, texttm, textb;
        Timer timer1;
        Tizen.System.Vibrator vibrator;
        readonly int vibn = Tizen.System.Vibrator.NumberOfVibrators;
        Stopwatch stopWatch = new Stopwatch();
        bool plockind = false;
        bool tmind = false;
        private enum locktype { cpu = 0, scr, dim };
        protected override void OnCreate()
        {
            base.OnCreate();
            Initialize();
        }

        void Initialize()
        {
            InitText(ref texttm, "", 120, 40, 50, 50, 6.0f);
            InitText(ref textb, "", 200, 30, 50, 50, 6.0f);
            InitText(ref text, rate.ToString(), 80, 80, 130, 200);
            InitText(ref textp, "+", 210, 80, 100, 100);
            InitText(ref textm, "-", 210, 180, 100, 100);
            InitText(ref textd, ".", -18, 154, 40, 40, 6.0f);
            texttm.TextColor = Color.Black;
            texttm.TouchEvent += Texttm_TouchEvent;
            textp.TouchEvent += PlusTouched;
            textm.TouchEvent += MinusTouched;
            text.TouchEvent += Text_TouchEvent;
            textb.TouchEvent += Textb_TouchEvent;
            if (vibn > 0)
                vibrator = Tizen.System.Vibrator.Vibrators[0];
            textb.Text = Tizen.System.Battery.Percent.ToString();
            Tizen.System.Battery.PercentChanged += Battery_PercentChanged;

            EcoreTimerPrecisionSet(0.001f);
            timer1 = new Timer(60000 / rate);
            stopWatch.Start();
            timer1.Start();
            timer1.Tick += TimerTick;
            _ = DevicePowerRequestLock((int)locktype.dim, 0); //locktype.dim works
        }

        private bool Texttm_TouchEvent(object source, View.TouchEventArgs e)
        {
            if (e.Touch.GetPointCount() > 0)
            {
                if (e.Touch.GetState(0) == PointStateType.Down)
                {
                    if (rate < maxrate)
                    {
                        if (tmind)
                        {
                            tmind = false;
                            texttm.TextColor = Color.Black;
                        }
                        else
                        {
                            tmind = true;
                            texttm.TextColor = Color.White;
                        }
                    }
                }
            }
            return true;
        }

        private bool Textb_TouchEvent(object source, View.TouchEventArgs e)
        {
            if (e.Touch.GetPointCount() > 0)
            {
                if (e.Touch.GetState(0) == PointStateType.Down)
                {
                    if (rate < maxrate)
                    {
                        plockind = !plockind;
                        if (plockind)
                        {
                            _ = DevicePowerRequestLock((int)locktype.scr, 0);
                            textb.Text = Tizen.System.Battery.Percent.ToString() + "L";
                        }
                        else
                        {
                            _ = DevicePowerReleaseLock((int)locktype.scr);
                            textb.Text = Tizen.System.Battery.Percent.ToString();
                        }

                    }
                }
            }
            return true;
        }

        private void Battery_PercentChanged(object sender, Tizen.System.BatteryPercentChangedEventArgs e)
        {
            if (plockind)
                textb.Text = Tizen.System.Battery.Percent.ToString() + "L";
            else
                textb.Text = Tizen.System.Battery.Percent.ToString();

        }

        private bool Text_TouchEvent(object source, View.TouchEventArgs e)
        {
            if (e.Touch.GetPointCount() > 0)
            {
                if (e.Touch.GetState(0) == PointStateType.Down)
                {
                    if (rate < maxrate)
                    {
                        _ = DevicePowerReleaseLock((int)locktype.dim);
                        _ = DevicePowerReleaseLock((int)locktype.scr);
                        timer1.Stop();
                        this.Exit();
                    }
                }
            }
            return true;
        }

        void InitText(ref TextLabel text, string caption, int posx, int posy, int sizex, int sizey, float pointsize = 18.0f)
        {
            text = new TextLabel(caption);
            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.VerticalAlignment = VerticalAlignment.Center;
            text.TextColor = Color.White;
            text.MultiLine = true;
            text.PointSize = pointsize;
            text.Position2D = new Position2D(posx, posy);
            text.Size2D = new Size2D(sizex, sizey);
            Window.Instance.GetDefaultLayer().Add(text);
        }

        private bool PlusTouched(object sender, TextLabel.TouchEventArgs e)
        {
            if (e.Touch.GetPointCount() > 0)
            {
                if (e.Touch.GetState(0) == PointStateType.Down)
                {
                    if (rate < maxrate)
                    {
                        rate += 10;
                        text.Text = rate.ToString();
                        timer1.Interval = 60000 / rate;
                    }
                }
            }
            return true;
        }
        private bool MinusTouched(object sender, TextLabel.TouchEventArgs e)
        {
            if (e.Touch.GetPointCount() > 0)
            {
                if (e.Touch.GetState(0) == PointStateType.Down)
                {
                    if (rate > 10)
                    {
                        rate -= 10;
                        text.Text = rate.ToString();
                        timer1.Interval = 60000 / rate;
                    }
                }
            }
            return true;
        }

        private bool TimerTick(object sender, Timer.TickEventArgs e)
        {
            stopWatch.Stop();
            texttm.Text = stopWatch.Elapsed.Milliseconds.ToString();
            stopWatch.Reset();
            stopWatch.Start();
            if (textd.Text != "")
                textd.Text = "";
            else
                textd.Text = ".";
            if (vibn > 0)
                vibrator.Vibrate(70, 100);
            return true;
        }

        static void Main(string[] args)
        {
            var app = new Program();
            app.Run(args);
        }

        [DllImport("libcapi-system-device.so.0", EntryPoint = "device_power_request_lock", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int DevicePowerRequestLock(int type, int timeout_ms);

        [DllImport("libcapi-system-device.so.0", EntryPoint = "device_power_release_lock", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int DevicePowerReleaseLock(int type);

        [DllImport("libecore.so.1", EntryPoint = "ecore_timer_precision_set", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void EcoreTimerPrecisionSet(double precision);

    }
}
