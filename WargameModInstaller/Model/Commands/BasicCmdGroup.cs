﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Commands
{
    /// <summary>
    /// Represents a basic group of installation commands holding unrelated commands 
    /// with the same execution priority value.
    /// </summary>
    public class BasicCmdGroup : ICmdGroup
    {
        private readonly List<IInstallCmd> commands;

        public BasicCmdGroup(IEnumerable<IInstallCmd> commands)
        {
            this.commands = new List<IInstallCmd> (commands);
        }

        public BasicCmdGroup(IEnumerable<IInstallCmd> commands, int priority)
        {
            this.commands = new List<IInstallCmd>(commands);
            this.Priority = priority;
        }

        /// <summary>
        /// Gets or sets the execution priority of the current group.
        /// </summary>
        /// <remarks>Should change priority off all contained commands?</remarks>
        public int Priority
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the set of commands which belong to the current group.
        /// </summary>
        public IReadOnlyCollection<IInstallCmd> Commands
        {
            get 
            { 
                return commands; 
            }
        }

    }

}
