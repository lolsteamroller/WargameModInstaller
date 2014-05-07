﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class AlterDictionaryCmd : IInstallCmd, IHasTarget, IHasTargetContent
    {
        public AlterDictionaryCmd()
        {
            this.AlteredEntries = new List<KeyValuePair<String, String>>();
        }

        /// <summary>
        /// Gets or sets a dictionary containing hash values of entries as keys and content to alter.
        /// </summary>
        public IEnumerable<KeyValuePair<String, String>>  AlteredEntries
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a command ID.
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a command execution priority.
        /// Commands with a higher priority are executed sooner.
        /// </summary>
        public int Priority
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a path to the dat file which holds the image to replace
        /// </summary>
        public InstallEntityPath TargetPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a path to a content inside the dat file.
        /// </summary>
        public ContentPath TargetContentPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an information wheather a command is critical.
        /// If the critical command fails, whole installation fails.
        /// </summary>
        public bool IsCritical
        {
            get;
            set;
        }

        public String GetExecutionMessage()
        {
            return String.Format(Properties.Resources.AlteringDictionary + " {0}...",
                TargetContentPath);
        }
    }
}