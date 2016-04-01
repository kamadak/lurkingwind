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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lurkingwind
{
    public class Settings
    {
        const string defaultSubdir = "Lurkingwind";
        const string defaultBasename = "user.config";
        const int currentVersion = 1;

        [System.Xml.Serialization.XmlIgnore]
        public string Path { get; private set; }

        public int SettingsVersion { get; set; }
        [System.Xml.Serialization.XmlArrayItem(ElementName = "Rule")]
        public List<SavedRule> RuleList { get; set; }

        public Settings()
        {
            SettingsVersion = currentVersion;
        }

        public void Load()
        {
            Path = InitializePath();
            var serializer = new System.Xml.Serialization.XmlSerializer(
                typeof(Settings));
            using (var sr = new System.IO.StreamReader(
                Path, new System.Text.UTF8Encoding(false)))
            {
                var tmp = (Settings)serializer.Deserialize(sr);
                RuleList = tmp.RuleList;
            }
        }

        string InitializePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!System.IO.Directory.Exists(appData))
                throw new System.IO.DirectoryNotFoundException(
                    string.Format("Application data folder \"{0}\" is not found.", appData));
            var dir = System.IO.Path.Combine(appData, defaultSubdir);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            return System.IO.Path.Combine(dir, defaultBasename);
        }

        public void Save()
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(
                typeof(Settings));
            using (var sw = new System.IO.StreamWriter(
                Path, false, new System.Text.UTF8Encoding(false)))
            {
                serializer.Serialize(sw, this);
            }
        }

        public void ExternRuleList(List<Rule> list)
        {
            RuleList = list.Select(x => SavedRule.Extern(x)).ToList();
        }

        public List<Rule> InternRuleList()
        {
            if (RuleList != null)
                return RuleList.Select(x => SavedRule.Intern(x)).ToList();
            else
                return new List<Rule>();
        }
    }

    public class Rule
    {
        public enum Actions { DoNothing, MoveToFront, Notify };

        public Regex TitlePattern { get; set; }
        public Regex ClassPattern { get; set; }
        public Actions Action { get; set; }

        public Rule(string titlePattern, string classPattern, Actions action)
        {
            if (titlePattern != null)
                TitlePattern = new Regex(titlePattern, RegexOptions.Compiled);
            if (classPattern != null)
                ClassPattern = new Regex(classPattern, RegexOptions.Compiled);
            Action = action;
        }

        public bool IsMatch(string title, string classname)
        {
            if (TitlePattern != null && !TitlePattern.IsMatch(title))
                return false;
            if (ClassPattern != null && !ClassPattern.IsMatch(classname))
                return false;
            return true;
        }
    }

    public class SavedRule
    {
        public string TitlePattern { get; set; }
        public string ClassPattern { get; set; }
        public Rule.Actions Action { get; set; }

        public static SavedRule Extern(Rule x)
        {
            var y = new SavedRule();
            if (x.TitlePattern != null)
                y.TitlePattern = x.TitlePattern.ToString();
            if (x.ClassPattern != null)
                y.ClassPattern = x.ClassPattern.ToString();
            y.Action = x.Action;
            return y;
        }

        public static Rule Intern(SavedRule x)
        {
            return new Rule(x.TitlePattern, x.ClassPattern, x.Action);
        }
    }
}
