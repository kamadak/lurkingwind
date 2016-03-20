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
        const int timerInterval = 1 * 1000;
        const int balloonTimeout = 30 * 1000;

        NotifyIcon icon;
        OptionsForm optionsForm;
        Settings settings;
        System.Windows.Forms.Timer timer;
        List<Rule> ruleList;
        HashSet<IntPtr> currentWindows;
        HashSet<IntPtr> newWindows;
        StringBuilder newText;

        public MainContext()
        {
            optionsForm = new OptionsForm();

            settings = new Settings();
            ruleList = settings.InternRuleList();

            icon = CreateNotifyIcon();
            ThreadExit += new EventHandler((sender, e) => icon.Dispose());
            icon.Visible = true;

            icon.BalloonTipIcon = ToolTipIcon.Info;
            icon.BalloonTipTitle = Application.ProductName;

            currentWindows = new HashSet<IntPtr>();
            NativeMethods.EnumWindows(new NativeMethods.EnumWindowsDelegate(ListAllWindows), IntPtr.Zero);

            timer = new System.Windows.Forms.Timer();
            timer.Interval = timerInterval;
            timer.Tick += new EventHandler((sender, e) => {
                newWindows = new HashSet<IntPtr>();
                newText = new StringBuilder();
                NativeMethods.EnumWindows(new NativeMethods.EnumWindowsDelegate(CheckNewWindows), IntPtr.Zero);
                currentWindows = newWindows;
                if (newText.Length > 0)
                {
                    icon.BalloonTipText = newText.ToString();
                    icon.ShowBalloonTip(balloonTimeout);
                }
            });
            timer.Start();
        }

        NotifyIcon CreateNotifyIcon()
        {
            NotifyIcon icon;

            icon = new NotifyIcon();
            icon.Text = Application.ProductName;
            icon.Icon = Properties.Resources.icon_lurkingwind;

            var ctxmenu = new ContextMenuStrip();
            var options = new ToolStripMenuItem("&Options...", null);
            options.Click += new EventHandler((sender, e) => ShowOptionsDialog());
            ctxmenu.Items.Add(options);
            ctxmenu.Items.Add(new ToolStripSeparator());
            var exit = new ToolStripMenuItem("E&xit", null);
            exit.Click += new EventHandler((sender, e) => ExitThread());
            ctxmenu.Items.Add(exit);
            icon.ContextMenuStrip = ctxmenu;

            return icon;
        }

        void ShowOptionsDialog()
        {
            // If already shown, do not call ShowDialog() again.
            if (optionsForm.Visible)
            {
                optionsForm.Activate();
                return;
            }

            optionsForm.SetRuleList(ruleList);
            var ret = optionsForm.ShowDialog();
            if (ret != DialogResult.OK)
                return;
            ruleList = optionsForm.GetRuleList();
            settings.ExternRuleList(ruleList);
            settings.Save();
            // No need to call ListAllWindows() again here.  The timer
            // runs while the dialog is shown, so do not worry about
            // detecting a lot of windows at a burst.
        }

        bool ListAllWindows(IntPtr hWnd, IntPtr lparam)
        {
            currentWindows.Add(hWnd);
            return true;
        }

        bool CheckNewWindows(IntPtr hWnd, IntPtr lparam)
        {
            newWindows.Add(hWnd);
            if (currentWindows.Contains(hWnd))
                return true;

            int tlen = NativeMethods.GetWindowTextLength(hWnd);
            var title = new StringBuilder(tlen + 1);
            if (tlen > 0)
                NativeMethods.GetWindowText(hWnd, title, title.Capacity);
            var classname = new StringBuilder(256);
            NativeMethods.GetClassName(hWnd, classname, classname.Capacity);

            foreach (var x in ruleList)
            {
                if (!x.IsMatch(title.ToString(), classname.ToString()))
                    continue;
                switch (x.Action)
                {
                case Rule.Actions.MoveToFront:
                    MoveToFront(hWnd);
                    break;
                case Rule.Actions.Notify:
                    newText.AppendLine(string.Format("{0} appeared.", title));
                    break;
                }
            }
            return true;
        }

        void MoveToFront(IntPtr hWnd)
        {
            const uint flags =
                NativeMethods.SWP_NOSIZE |
                NativeMethods.SWP_NOMOVE |
                NativeMethods.SWP_NOACTIVATE |
                NativeMethods.SWP_ASYNCWINDOWPOS;
            Int32 style, exstyle;

            // It makes no sense to raise an invisible window.
            style = NativeMethods.GetWindowLong(hWnd, NativeMethods.GWL_STYLE);
            if ((style & NativeMethods.WS_VISIBLE) == 0)
                return;

            // Save WS_EX_TOPMOST.
            exstyle = NativeMethods.GetWindowLong(hWnd, NativeMethods.GWL_EXSTYLE);
            // Restack the window.
            if (!NativeMethods.SetWindowPos(hWnd, NativeMethods.HWND_NOTOPMOST, 0, 0, 0, 0, flags))
                throw new ApplicationException("SetWindowPos() failed");
            if (!NativeMethods.SetWindowPos(hWnd, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, flags))
                throw new ApplicationException("SetWindowPos() failed");
            // Restore WS_EX_TOPMOST.
            NativeMethods.SetWindowLong(hWnd, NativeMethods.GWL_EXSTYLE, exstyle);
        }
    }
}
