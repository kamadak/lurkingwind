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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lurkingwind
{
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();

            Icon = Properties.Resources.icon_lurkingwind;
            actionColumn.ValueType = typeof(Rule.Actions);
            foreach (var x in Enum.GetValues(typeof(Rule.Actions)))
                actionColumn.Items.Add(x);
        }

        public void SetRuleList(List<Rule> list)
        {
            dataGridView1.Rows.Clear();
            foreach (var x in list)
            {
                string titlePattern =
                    x.TitlePattern != null ? x.TitlePattern.ToString() : null;
                string classPattern =
                    x.ClassPattern != null ? x.ClassPattern.ToString() : null;
                dataGridView1.Rows.Add(titlePattern, classPattern, x.Action);
            }
        }

        public List<Rule> GetRuleList()
        {
            List<Rule> list = new List<Rule>();
            foreach (var x in dataGridView1.Rows.Cast<DataGridViewRow>())
            {
                var titlePattern = (string)x.Cells[0].Value;
                var classPattern = (string)x.Cells[1].Value;
                var action = (Nullable<Rule.Actions>)x.Cells[2].Value;

                // If the title and class patterns are both empty,
                // guess that the user want to remove the row.
                if (string.IsNullOrEmpty(titlePattern) && string.IsNullOrEmpty(classPattern))
                    continue;
                if (!action.HasValue)
                    continue;
                list.Add(new Rule(titlePattern, classPattern, action.Value));
            }
            return list;
        }
    }
}
