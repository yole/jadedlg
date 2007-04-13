using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Win32;

namespace JadeDlg
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string jadeEmpirePath = null;
            RegistryKey key = Registry.LocalMachine.OpenSubKey( "SOFTWARE\\BioWare\\Jade Empire" );
            if (key != null)
            {
                try
                {
                    jadeEmpirePath = (string) key.GetValue("Path");
                }
                finally
                {
                    key.Close();
                }
            }

            if (jadeEmpirePath == null)
            {
                MessageBox.Show((IWin32Window) null, "Jade Empire Special Edition installation not found. Exiting.");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(jadeEmpirePath));
        }
    }
}