﻿using System;
using System.Linq;
using System.Windows.Forms;

namespace OsuMemoryDataProviderTester
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(args.FirstOrDefault()));
        }
    }
}
