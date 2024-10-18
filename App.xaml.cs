using System.Configuration;
using System.Data;
using System.Text;
using System.Windows;

namespace RPC_Integration
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            string args = "";
            if (Console.In.Peek() != -1)
            {
                var fullInput = new StringBuilder();
                string? currentLine;
                while ((currentLine = Console.ReadLine()) != null)
                    fullInput.AppendLine(currentLine);

                args = fullInput.ToString();
            }
            new RPC(args);
            base.OnStartup(e);
        }
    }

}
