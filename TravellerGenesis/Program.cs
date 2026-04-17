using System;
using System.Windows.Forms;

namespace TravellerGenesis
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Forms.MainForm());
        }
    }
}
