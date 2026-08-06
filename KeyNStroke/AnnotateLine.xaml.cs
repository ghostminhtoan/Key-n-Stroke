using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static KeyNStroke.NativeMethodsMouse;

namespace KeyNStroke
{
    /// <summary>
    /// Interaktionslogik für AnnotateLine.xaml
    /// </summary>
    public partial class AnnotateLine : Window
    {

        IMouseRawEventProvider m;
        IKeystrokeEventProvider k;
        SettingsStore s;
        IntPtr windowHandle;
        bool isDown;
        POINT startCursorPosition = new POINT(0, 0);
        POINT endCursorPosition = new POINT(0, 0);
        bool nextClickDraws = false;
        bool nextClickHides = false;

        public AnnotateLine(IMouseRawEventProvider m, IKeystrokeEventProvider k, SettingsStore s)
        {
            InitializeComponent();

            this.m = m;
            this.s = s;
            this.k = k;

            s.PropertyChanged += settingChanged;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            s.CallPropertyChangedForAllProperties();
            m.MouseEvent += m_MouseEvent;
            this.k.KeystrokeEvent += m_KeystrokeEvent;
            windowHandle = new WindowInteropHelper(this).Handle;
            SetFormStyles();
        }

        #region Shortcut

        public string AnnotateLineShortcut;

        void m_KeystrokeEvent(KeystrokeEventArgs e)
        {
            if (s == null) return;
            string pressed = e.ShortcutIdentifier();
            e.raw.preventDefault = e.raw.preventDefault || CheckForTrigger(pressed);
        }

        private bool CheckForTrigger(string pressed)
        {
            if (AnnotateLineShortcut != null && KeystrokeDisplay.ShortcutMatches(AnnotateLineShortcut, pressed))
            {
                nextClickDraws = true;
                return true;
            }
            return false;
        }

        public void SetAnnotateLineShortcut(string shortcut)
        {
            if (KeystrokeDisplay.ValidateShortcutSetting(shortcut))
            {
                AnnotateLineShortcut = shortcut;
            }
            else
            {
                AnnotateLineShortcut = s.AnnotateLineShortcutDefault;
            }
        }

        #endregion

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetKeyState(int vKey);
        private const int VK_SHIFT = 0x10;

        private void m_MouseEvent(MouseRawEventArgs raw_e)
        {
            if (s == null) return;
            if (!isDown && nextClickHides && raw_e.Action == MouseAction.Down)
            {
                raw_e.preventDefault = true;
                nextClickHides = false;
                this.Hide();
            }

            if (isDown && raw_e.Action == MouseAction.Move)
            {
                this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (s == null || !isDown) return;
                    Point currentPos = this.PointFromScreen(new Point(raw_e.Position.X, raw_e.Position.Y));
                    bool isShiftPressed = (GetKeyState(VK_SHIFT) & 0x8000) != 0;

                    if (isShiftPressed)
                    {
                        if (drawingPolyline.Points.Count > 0)
                        {
                            Point startPos = drawingPolyline.Points[0];
                            double dx = currentPos.X - startPos.X;
                            double dy = currentPos.Y - startPos.Y;
                            if (dx != 0 || dy != 0)
                            {
                                double angle = Math.Atan2(dy, dx);
                                double angleDeg = angle * 180.0 / Math.PI;
                                double roundedAngle = Math.Round(angleDeg / 45.0) * 45.0;
                                double rad = roundedAngle * Math.PI / 180.0;
                                double dist = Math.Sqrt(dx * dx + dy * dy);
                                Point constrainedPos = new Point(startPos.X + dist * Math.Cos(rad), startPos.Y + dist * Math.Sin(rad));
                                
                                drawingPolyline.Points.Clear();
                                drawingPolyline.Points.Add(startPos);
                                drawingPolyline.Points.Add(constrainedPos);
                            }
                        }
                    }
                    else
                    {
                        if (drawingPolyline.Points.Count == 0 || drawingPolyline.Points.Last() != currentPos)
                        {
                            drawingPolyline.Points.Add(currentPos);
                        }
                    }
                }));
            }
            else if (!isDown && raw_e.Action == MouseAction.Down && nextClickDraws)
            {
                isDown = true;
                raw_e.preventDefault = true;
                nextClickDraws = false;
                this.Show();
                this.UpdateLayout();
                drawingPolyline.Points.Clear();
                Point startPos = this.PointFromScreen(new Point(raw_e.Position.X, raw_e.Position.Y));
                drawingPolyline.Points.Add(startPos);
            }
            else if (isDown && raw_e.Action == MouseAction.Up)
            {
                isDown = false;
                nextClickHides = true;
            }
        }

        private void settingChanged(object sender, PropertyChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (s == null) return;
                switch (e.PropertyName)
                {
                    case "AnnotateLineShortcutTrigger":
                        nextClickDraws = true;
                        break;
                    case "AnnotateLineColor":
                        UpdateColor();
                        break;
                    case "AnnotateLineShortcut":
                        SetAnnotateLineShortcut(s.AnnotateLineShortcut);
                        break;
                }
            }));
        }

        void SetFormStyles()
        {
            Log.e("CI", $"WindowHandle={windowHandle}");
            NativeMethodsGWL.ClickThrough(windowHandle);
            NativeMethodsGWL.HideFromAltTab(windowHandle);

            this.Left = SystemParameters.VirtualScreenLeft;
            this.Top = SystemParameters.VirtualScreenTop;
            this.Width = SystemParameters.VirtualScreenWidth;
            this.Height = SystemParameters.VirtualScreenHeight;
        }

        void UpdateColor()
        {
            drawingPolyline.Stroke = new SolidColorBrush(UIHelper.ToMediaColor(s.AnnotateLineColor));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (m != null)
            {
                m.MouseEvent -= m_MouseEvent;
            }
            if (k != null)
            {
                k.KeystrokeEvent -= m_KeystrokeEvent;
            }
            if (s != null)
            {
                s.PropertyChanged -= settingChanged;
            }
            m = null;
            s = null;
            k = null;
        }
    }
}
