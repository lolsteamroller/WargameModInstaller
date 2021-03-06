﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;

namespace WargameModInstaller.Common.Entities
{
    /// <summary>
    /// Represents a path to a file being a subject of the installation.
    /// </summary>
    public class InstallEntityPath : PathBase
    {
        public InstallEntityPath(String pathValue)
            : base(pathValue)
        {

        }

        public InstallEntityPath(String pathValue, InstallEntityPathType pathType)
            : base(pathValue, pathType)
        {

        }

        protected override IEnumerable<PathTypeResolveRule> CreateResolveRules()
        {
            var absolutePathRule = new PathTypeResolveRule(InstallEntityPathType.Absolute, 1, AbsolutePathRule);
            var relativePathRule = new PathTypeResolveRule(InstallEntityPathType.Relative, 2, RelativePathRule);

            var rulesList = new List<PathTypeResolveRule>();
            rulesList.Add(absolutePathRule);
            rulesList.Add(relativePathRule);

            return rulesList;
        }

        protected override PathType GetDefaultPathType()
        {
            return InstallEntityPathType.Unknown;
        }

        private bool AbsolutePathRule(String path)
        {
            return PathUtilities.IsValidAbsolutePath(path);
        }

        private bool RelativePathRule(String path)
        {
            return PathUtilities.IsValidRelativePath(path);
        }

        public override bool Equals(PathBase other)
        {
            PathBase otherPath = other as PathBase;
            if (otherPath != null)
            {
                //Installtion paths have to be case insensitive, since windows local paths are case insensitive
                return (StringComparer.CurrentCultureIgnoreCase.Compare(otherPath.Value, this.Value)) == 0 &&
                    (otherPath.PathType == this.PathType);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            //Installtion paths have to be case insensitive, since windows local paths are case insensitive
            return StringComparer.CurrentCultureIgnoreCase.GetHashCode(Value) +
                PathType.GetHashCode();
        }

    }
}
