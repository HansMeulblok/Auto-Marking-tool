using System;
using System.Windows.Forms;

namespace AutoMarking
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Set up the application environment and start the form
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm()); 
        }
    }
}
