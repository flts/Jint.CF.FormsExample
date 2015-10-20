using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Globalization;

namespace Jint.Example
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {
            Application.Run(new Form1());
        }
    }
}