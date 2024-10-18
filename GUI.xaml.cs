using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace RPC_Integration
{
    /// <summary>
    /// Interaction logic for GUI.xaml
    /// </summary>
    public partial class GUI : Window
    {
        internal static GUI? instance;
        public GUI()
        {
            InitializeComponent();
            Create();
        }

        private void Create()
        {
            instance?.Close();
            instance = this;
            Background = Brushes.White;
            tbConnected.Foreground = Brushes.Red;

            canvas.Background = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri("bg.png", UriKind.Relative)),
                Opacity = 0.08
            };

            bdSmallImagePreview.Background = GetIBrush("ph.png");
            bdLargeImagePreview.Background = GetIBrush("ph.png");
            canvas.InvalidateVisual();

            LinkFields();

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1),
            };
            timer.Tick += (s, e) => UpdateElapsedTime();
            timer.Start();

            UpdateFields(ref RPC.instance.Activity);
        }

        private void LinkFields()
        {
            tibDetails.Link(() => ltbDetails.Text, text => { ltbDetails.Text = text; FullSync(); }, 255, 1);
            tibState.Link(() => ltbState.Text, text => { ltbState.Text = text; FullSync(); }, 255, 1);
            tibLIText.Link(() => ltbLIText.Text, text => { ltbLIText.Text = text; FullSync(); }, 255, 1);
            tibSIText.Link(() => ltbSIText.Text, text => { ltbSIText.Text = text; FullSync(); }, 255, 1);
            sibLargeImage.Link(() => ltbLargeImage.Text, text => { ltbLargeImage.Text = text; FullSync(); }, RPC.PhotoMapping.Keys.ToArray());
            sibSmallImage.Link(() => ltbSmallImage.Text, text => { ltbSmallImage.Text = text; FullSync(); }, RPC.PhotoMapping.Keys.ToArray());
            nibPCurrent.Link(() => {
                try
                {
                    return int.Parse(ltbPCurrent.Text);
                }
                catch
                {
                    return 1;
                }
            }
            , text => { ltbPCurrent.Text = $"{text}"; FullSync(); }, 999, 1, () =>
            {
                try
                {
                    return (int.Parse(ltbPMax.Text), 1);
                }
                catch
                {
                    return (999, 1);
                }
            });

            nibPMax.Link(() => {
                try
                {
                    return int.Parse(ltbPMax.Text);
                }
                catch
                {
                    return 1;
                }
            }, text => { ltbPMax.Text = $"{text}"; FullSync(); }, 999, 1, () =>
            {
                try
                {
                    return (999, int.Parse(ltbPCurrent.Text));
                }
                catch
                {
                    return (999, 1);
                }
            });


            ltbDetails.Link(tbDetailsPreview, text => RPC.instance.Activity.Details = text);
            ltbLargeImage.Link(text =>
            {
                bdLargeImagePreview.Background = GetIBrush(RPC.PhotoMapping.GetValueOrDefault(text, "ph.png"));
                RPC.instance.Activity.Assets.LargeImage = text;
            });
            ltbSmallImage.Link(text =>
            {
                bdSmallImagePreview.Background = GetIBrush(RPC.PhotoMapping.GetValueOrDefault(text, "ph.png"));
                RPC.instance.Activity.Assets.SmallImage = text;
            });
            ltbPCurrent.Link(text => ParsePartySize(text, RPC.instance.Activity.Party.Size, true));
            ltbPMax.Link(text => ParsePartySize(text, RPC.instance.Activity.Party.Size, false));


            ltbState.Link(tbStatePreview, text => RPC.instance.Activity.State = text);
            ltbLIText.Link(text => RPC.instance.Activity.Assets.LargeText = text);
            ltbSIText.Link(text => RPC.instance.Activity.Assets.SmallText = text);
        }

        private void ParsePartySize(string text, Discord.PartySize partySize, bool isCurrent)
        {
            if (int.TryParse(text, out int num))
            {
                if (isCurrent)
                    partySize.CurrentSize = num;
                else
                    partySize.MaxSize = num;

                tbParty.Text = $"({partySize.CurrentSize} of {partySize.MaxSize})";
            }
        }

        internal void FullSync()
        {
            PushChanges();
            Sync();
        }

        internal void PushChanges()
        {
            tbDetailsPreview.Text = ltbDetails.Text;
            bdLargeImagePreview.Background = GetIBrush(RPC.PhotoMapping.GetValueOrDefault(ltbLargeImage.Text, "ph.png"));
            bdSmallImagePreview.Background = GetIBrush(RPC.PhotoMapping.GetValueOrDefault(ltbSmallImage.Text, "ph.png"));
            tbParty.Text = $"({ltbPCurrent.Text} of {ltbPMax.Text})";
            tbStatePreview.Text = ltbState.Text;
        }

        internal void Sync()
        {
            ref Activity a = ref RPC.instance.Activity;
            if (!string.IsNullOrEmpty(ltbDetails.Text))
            {
                a.Details = ltbDetails.Text;
            }
            if (!string.IsNullOrEmpty(ltbState.Text))
            {
                a.State = ltbState.Text;
            }
            if (!string.IsNullOrEmpty(ltbLargeImage.Text))
            {
                a.Assets.LargeImage = ltbLargeImage.Text;
            }
            if (!string.IsNullOrEmpty(ltbSmallImage.Text))
            {
                a.Assets.SmallImage = ltbSmallImage.Text;
            }
            if (!string.IsNullOrEmpty(ltbLIText.Text))
            {
                a.Assets.LargeText = ltbLIText.Text;
            }
            if (!string.IsNullOrEmpty(ltbSIText.Text))
            {
                a.Assets.SmallText = ltbSIText.Text;
            }
            if (int.TryParse(ltbPCurrent.Text, out int currentSize))
            {
                a.Party.Size.CurrentSize = currentSize;
            }
            if (int.TryParse(ltbPMax.Text, out int maxSize))
            {
                a.Party.Size.MaxSize = maxSize;
            }
            if (!string.IsNullOrEmpty(ltbStart.Text) && ConvertToUnixTime(ltbStart.Text, out long t))
            {
                a.Timestamps.Start = t;
            }
            if (!string.IsNullOrEmpty(ltbEnd.Text) && ConvertToUnixTime(ltbEnd.Text, out t))
            {
                a.Timestamps.End = t;
            }
        }


        private void UpdateElapsedTime()
        {
            try
            {
                DateTime now = DateTime.UtcNow;
                DateTime startTime = DateTimeOffset.FromUnixTimeSeconds(RPC.instance.Activity.Timestamps.Start).DateTime;
                TimeSpan difference = now - startTime;
                tbElapsedPreview.Text = difference.TotalHours >= 1
                    ? difference.ToString(@"hh\:mm\:ss")
                    : difference.ToString(@"mm\:ss");
            }
            catch (Exception ex)
            {
                Log(Warning, $"Non-critical fail: Failed to update elapsed time, see: {ex.Message}");
            }
        }

        internal void UpdateFields(ref Discord.Activity activity)
        {
            LinkedTB.ignoreCallback = true;
            ltbDetails.Text = activity.Details;
            ltbState.Text = activity.State;
            ltbLargeImage.Text = activity.Assets.LargeImage;
            ltbLIText.Text = activity.Assets.LargeText;
            ltbSmallImage.Text = activity.Assets.SmallImage;
            ltbSIText.Text = activity.Assets.SmallText;
            ltbPCurrent.Text = activity.Party.Size.CurrentSize.ToString();
            ltbPMax.Text = activity.Party.Size.MaxSize.ToString();
            _ = ConvertFromUnixTime(activity.Timestamps.Start, out string s);
            ltbStart.Text = $"{s}";
            _ = ConvertFromUnixTime(activity.Timestamps.End, out s);
            ltbEnd.Text = $"{s}";
            LinkedTB.ignoreCallback = false;
            PushChanges();
        }
    }
}
