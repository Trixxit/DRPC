global using static RPC_Integration.Util;
global using static RPC_Integration.RPC.Logging;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using Discord;
using System.Data;
using System.Printing;
using System.Windows.Media.Imaging;

namespace RPC_Integration
{
    internal class RPC
    {
        static readonly Dictionary<string, string> basePhotoMapping = new Dictionary<string, string>()
        {
            { "bored", "https://cdn.discordapp.com/app-assets/1288446986420359199/1288465512896135199.png" },
            { "bored2", "https://cdn.discordapp.com/app-assets/1288446986420359199/1288465791016239146.png" },
            { "cat1", "https://cdn.discordapp.com/app-assets/1288446986420359199/1288494618668699689.png" },
            { "cat2", "https://cdn.discordapp.com/app-assets/1288446986420359199/1288494577799532544.png" },
            { "cat3", "https://cdn.discordapp.com/app-assets/1288446986420359199/1288494556257714278.png" },
            { "lofigirl", "https://cdn.discordapp.com/app-assets/1288446986420359199/1288485647832584242.png" },
            { "sayoko", "https://cdn.discordapp.com/app-assets/1288446986420359199/1290190164542492702.png" }
        };

        internal static Dictionary<string, string> PhotoMapping = new();

        internal Discord.Activity Activity = new()
        {
            State = "N/A",
            Details = "N/A",
            Assets =
            {
                LargeImage = "bored",
                SmallImage = "bored2",
                LargeText = "N/A",
                SmallText = "N/A",
            },
            Timestamps =
            {
                Start = ((DateTimeOffset)(DateTime.UtcNow)).ToUnixTimeSeconds(),
                End = ((DateTimeOffset)((DateTime.UtcNow).AddDays(30))).ToUnixTimeSeconds(),
            },
            Instance = true,
            Party =
            {
                Size =
                {
                    CurrentSize = 1,
                    MaxSize = 2
                }
            }
        };

        const long CLIENT_ID = 1288446986420359199;

        internal static RPC? instance;

        public static Stream Out 
            => Console.OpenStandardOutput();

        public const string Args = @"
    '-noWindow' / '--nw': Open without the GUI
    '-debug' / '--d': Activates debug output
    '-warning' / '--w': Activates warning output
    '-error' / '--e': Deactivates error output (on by default)
";
        internal static Logging LogLevel = Logging.None;

        internal static string _args { get; private set; }

        public RPC(string args)
        {
            _args = args;
            LogLevel = (CheckArg("-debug", "--d") ? Logging.Debug : 0) | (CheckArg("-warning", "--w") ? Logging.Warning : 0) | (!CheckArg("-error", "--e") ? Logging.Error : 0) | ((CheckArg("-debug", "--d") || CheckArg("-warning", "--w") || (!CheckArg("-error", "--e"))) ? 0 : Logging.None);

            Log(Debug, $"Log Level: {LogLevel:F}");
            if (instance != null)
                return;
            instance = this;
            if (!Directory.Exists("Profiles"))
            {
                Directory.CreateDirectory("Profiles");
                Log(Debug, $"Created profile folder");
            }
            PhotoMapping = basePhotoMapping;
            if (!File.Exists("mapping"))
            {
                StreamWriter? ws = null;
                try
                {
                    Log(Debug, $"Writing base photo mappings");
                    ws = File.CreateText("mapping");
                    foreach (var kvp in basePhotoMapping)
                        ws.WriteLine($"{kvp.Key}::-::{kvp.Value}");
                }
                catch (Exception ex)
                {
                    Log(Warning, $"Failed to write base photo mapping! This won't affect functionality apart from preventing new mappings from being added.\nException: {ex.Message}.");
                }
                finally
                {
                    Log(Debug, $"Finished writing photo map.");
                    ws?.Close();
                }
            }
            else
            {
                try
                {
                    Log(Debug, $"Reading photo map.");
                    string[] photolinks = File.ReadAllLines("mapping");
                    foreach (string photolink in photolinks)
                    {
                        string[] split = photolink.Split("::-::");
                        PhotoMapping.Add(split[0], split[1]);
                    }
                }
                catch (Exception ex)
                {
                    Log(Error, $"Failed to read detected photo mapping! Reverting to base mapping set -- this means that any non-default mappings will not be used!\nException: {ex.Message}.");
                }
            }
            if (!CheckArg("-nowindow", "--nw"))
            {
                Log(Debug, $"Opening GUI");
                GUI gui = new GUI();
                gui.Show();
            }
        }

        [Flags]
        internal enum Logging : byte
        {
            None = 0,
            Debug = 1,
            Warning = 2,
            Error = 4,
        }
    }

    internal static class Util
    {
        internal static void Log(RPC.Logging level, string message)
        {
            if (RPC.LogLevel.HasFlag(Debug) && level == Debug)
                Console.WriteLine($"[DEBUG] {message}");
            else if (RPC.LogLevel.HasFlag(Warning) && level == Warning)
                Console.WriteLine($"[WARNING] {message}");
            else if (RPC.LogLevel.HasFlag(Error) && level == Error)
                Console.WriteLine($"[DEBUG] {message}");
            else if (level == None)
                Console.WriteLine(message);
        }

        internal static bool CheckArg(string lg, string st)
            => RPC._args.Contains(lg, StringComparison.InvariantCultureIgnoreCase) || 
                RPC._args.Contains(st, StringComparison.InvariantCultureIgnoreCase);

        internal static SolidColorBrush GetBrush(int R, int G, int B)
            => new(new() { R = (byte)R, G = (byte)G, B = (byte)B, A = 255 });

        internal static ImageBrush GetIBrush(string path)
            => new() { ImageSource = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute)) };

        public static bool ConvertToUnixTime(string timestamp, out long s)
        {
            try
            {
                DateTime parsedTime = DateTime.ParseExact(timestamp, "yyyy:MM:dd:HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal);

                s = ((DateTimeOffset)parsedTime).ToUnixTimeSeconds();
            }
            catch
            {
                s = 0;
                return false;
            }
            return true;
        }


        public static bool ConvertFromUnixTime(long unixTime, out string s)
        {
            try
            {
                s = DateTimeOffset.FromUnixTimeSeconds(unixTime).ToString("yyyy:MM:dd:HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                s = "";
                return false;
            }
            return true;
        }

    }
}
