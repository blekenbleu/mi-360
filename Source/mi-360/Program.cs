﻿using System;
using System.Windows.Forms;

namespace mi360
{
    class Program
    {
        // Satisfies rule: MarkWindowsFormsEntryPointsWithStaThread.
        [STAThread]
        static void Main(string[] args)
        {
           Application.Run(new Mi360Application());
        }

    }
}
