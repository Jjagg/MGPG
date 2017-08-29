// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Cli.Sln.Internal;

namespace Microsoft.DotNet.Tools.Common
{
    public static class SlnFileFactory
    {
        public static SlnFile CreateFromFileOrDirectory(string fileOrDirectory)
        {
            if (File.Exists(fileOrDirectory))
            {
                return FromFile(fileOrDirectory);
            }
            else
            {
                return FromDirectory(fileOrDirectory);
            }
        }

        private static SlnFile FromFile(string solutionPath)
        {
            SlnFile slnFile = null;
            try
            {
                slnFile = SlnFile.Read(solutionPath);
            }
            catch (InvalidSolutionFormatException e)
            {
                throw new Exception(string.Format(
                    "Invalid solution `{0}`. {1}.",
                    solutionPath,
                    e.Message));
            }
            return slnFile;
        }

        private static SlnFile FromDirectory(string solutionDirectory)
        {
            DirectoryInfo dir;
            try
            {
                dir = new DirectoryInfo(solutionDirectory);
                if (!dir.Exists)
                {
                    throw new Exception(string.Format(
                        "Could not find solution or directory `{0}`.",
                        solutionDirectory));
                }
            }
            catch (ArgumentException)
            {
                throw new Exception(string.Format(
                    "Could not find solution or directory `{0}`.",
                    solutionDirectory));
            }

            FileInfo[] files = dir.GetFiles("*.sln");
            if (files.Length == 0)
            {
                throw new Exception(string.Format(
                    "Specified solution file {0} does not exist, or there is no solution file in the directory.",
                    solutionDirectory));
            }

            if (files.Length > 1)
            {
                throw new Exception(string.Format(
                    "Found more than one solution file in {0}. Please specify which one to use.",
                    solutionDirectory));
            }

            FileInfo solutionFile = files.Single();
            if (!solutionFile.Exists)
            {
                throw new Exception(string.Format(
                    "Specified solution file {0} does not exist, or there is no solution file in the directory.",
                    solutionDirectory));
            }

            return FromFile(solutionFile.FullName);
        }
    }
}
