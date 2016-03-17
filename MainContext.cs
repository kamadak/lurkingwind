//
// Copyright (c) 2016 KAMADA Ken'ichi.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE AUTHOR AND CONTRIBUTORS ``AS IS'' AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED.  IN NO EVENT SHALL THE AUTHOR OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
// OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
// OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
// SUCH DAMAGE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lurkingwind
{
    internal class MainContext : ApplicationContext
    {
        NotifyIcon icon;
        OptionsForm optionsForm;
        Settings settings;
        List<IntPtr> currentWindows;

        public MainContext()
        {
            optionsForm = new OptionsForm();

            settings = new Settings();

            icon = CreateNotifyIcon();
            ThreadExit += new EventHandler((sender, e) => icon.Dispose());
            icon.Visible = true;

            currentWindows = new List<IntPtr>();
            NativeMethods.EnumWindows(new NativeMethods.EnumWindowsDelegate(ListAllWindows), IntPtr.Zero);
        }

        NotifyIcon CreateNotifyIcon()
        {
            NotifyIcon icon;

            icon = new NotifyIcon();
            icon.Text = Application.ProductName;
            icon.Icon = optionsForm.Icon;

            var ctxmenu = new ContextMenuStrip();
            var options = new ToolStripMenuItem("&Options...", null);
            options.Click += new EventHandler((sender, e) => optionsForm.ShowDialog());
            ctxmenu.Items.Add(options);
            ctxmenu.Items.Add(new ToolStripSeparator());
            var exit = new ToolStripMenuItem("E&xit", null);
            exit.Click += new EventHandler((sender, e) => ExitThread());
            ctxmenu.Items.Add(exit);
            icon.ContextMenuStrip = ctxmenu;

            return icon;
        }

        bool ListAllWindows(IntPtr hWnd, IntPtr lparam)
        {
            int ret, tlen;

            currentWindows.Add(hWnd);

            Console.WriteLine("{0}", hWnd);
            tlen = NativeMethods.GetWindowTextLength(hWnd);
            if (tlen > 0)
            {
                var title = new StringBuilder(tlen + 1);
                ret = NativeMethods.GetWindowText(hWnd, title, title.Capacity);
                if (ret != 0)
                    Console.WriteLine("{0}", title);
            }
            var classname = new StringBuilder(256);
            ret = NativeMethods.GetClassName(hWnd, classname, classname.Capacity);
            if (ret != 0)
                Console.WriteLine("{0}", classname);
            return true;
        }
    }
}
