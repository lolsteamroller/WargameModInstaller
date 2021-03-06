﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Services.Commands.Base;

namespace WargameModInstaller.Services.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class CopyModFileCmdExecutor : CmdExecutorBase<CopyModFileCmd>
    {
        public CopyModFileCmdExecutor(CopyModFileCmd command)
            : base(command)
        {
            this.TotalSteps = 1;
        }

        protected override void ExecuteInternal(CmdExecutionContext context, CancellationToken token)
        {
            InitializeProgress();

            String sourceFullPath = Path.Combine(context.InstallerSourceDirectory, Command.SourcePath);
            if (!File.Exists(sourceFullPath))
            {
                throw new CmdExecutionFailedException("A file given by the sourcePath doesn't exist.",
                    String.Format(Properties.Resources.CopyFileErrorParamMsg, Command.SourcePath));
            }

            String targetFullPath = Path.Combine(context.InstallerTargetDirectory, Command.TargetPath);
            if (!PathUtilities.IsValidPath(targetFullPath))
            {
                throw new CmdExecutionFailedException("A given targetPath is not a valid path.",
                    String.Format(Properties.Resources.CopyFileErrorParamMsg, Command.SourcePath));
            }

            PathUtilities.CreateDirectoryIfNotExist(Path.GetDirectoryName(targetFullPath));
            
            FileUtilities.CopyFileEx(sourceFullPath, targetFullPath, token);

            SetMaxProgress();
        }

    }

}
