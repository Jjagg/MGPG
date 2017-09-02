﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Construction;
using Microsoft.DotNet.Cli.Sln.Internal;
using Microsoft.DotNet.Tools.Common;

namespace MGPG
{
    /// <summary>
    /// Core class that generates the template when <see cref="Generate"/> is called.
    /// </summary>
    public class Generator
    {

        /// <summary>
        /// Get or set the configuration of this <see cref="Generator"/>.
        /// </summary>
        public Settings Settings { get; set; }

        public event EventHandler<RenderEventArgs> Render;

        /// <summary>
        /// Create a <see cref="Generator"/> with default <see cref="MGPG.Settings"/>.
        /// </summary>
        public Generator()
        {
            Settings = new Settings();
        }

        /// <summary>
        /// Generate the template defined in the template file set in <see cref="Settings"/>.
        /// </summary>
        public void Generate(GeneratorArguments arguments)
        {
            var logger = new Logger
            {
                LogLevel = Settings.LogLevel,
                LogWriter = Settings.LogWriter ?? Console.Out,
                SupressErrors = Settings.SupressErrors
            };

            if (File.Exists(arguments.DestinationFolder) ||
                (Directory.Exists(arguments.DestinationFolder) && Directory.EnumerateFileSystemEntries(arguments.DestinationFolder).Any() && !Settings.Overwrite))
            {
                logger.Log(LogLevel.Error, $"The destination directory '{arguments.DestinationFolder}' already exists and is not empty. Set the Overwrite flag to write anyway.");
                return;
            }
            if (!File.Exists(arguments.TemplateFile))
            {
                logger.Log(LogLevel.Error, $"Template file not found at '{arguments.TemplateFile}'.");
                return;
            }

            var dst = Path.GetFullPath(arguments.DestinationFolder);
            var templatePath = Path.GetFullPath(arguments.TemplateFile);

            var template = new Template(templatePath, logger);
            if (template.HasFatalError)
                return;

            var variables = template.Variables.With(arguments.Variables);
            variables.Set(Template.SourceFileExtensionVariable, arguments.SourceLanguage.GetFileExtension());

            Directory.CreateDirectory(dst);
            var projectPaths = new List<string>();
            foreach (var pe in template.ProjectEntries)
            {
                var adst = RenderFileEntry(pe, templatePath, variables, dst, template, logger);
                logger.Log(LogLevel.Info, $"Rendered project '{pe.AbsoluteSrc}' to '{adst}'.");
                projectPaths.Add(adst);
                foreach (var fe in pe.FileEntries)
                {
                    adst = RenderFileEntry(fe, templatePath, variables, dst, template, logger);
                    logger.Log(LogLevel.Info, $"Rendered file '{fe.AbsoluteSrc}' to '{adst}'.");
                }
            }

            if (arguments.Solution != null)
            {
                var slnPath = Path.GetFullPath(arguments.Solution);
                SlnFile sln;
                if (File.Exists(arguments.Solution))
                {
                    sln = SlnFileFactory.CreateFromFileOrDirectory(arguments.Solution);
                }
                else
                {
                    sln = new SlnFile {FullPath = slnPath};
                    sln.FormatVersion = "12.00";
                    sln.MinimumVisualStudioVersion = "10.0.40219.1";
                    logger.Log(LogLevel.Info, $"Created solution '{Path.GetFileName(slnPath)}'.");
                }
                foreach (var projectPath in projectPaths)
                {
                    sln.AddProject(projectPath);
                    logger.Log(LogLevel.Info, $"Added project '{Path.GetFileName(projectPath)}' to solution '{Path.GetFileName(slnPath)}'.");
                }
                sln.Write();
            }
        }

        private string RenderFileEntry(FileEntry fe, string templatePath, VariableCollection variables, string dst, Template template, Logger logger)
        {
            try
            {
                logger.Log(LogLevel.Verbose, $"Looking for {fe.RawRelativeSrc} at {fe.PossibleRawSrcPaths.Aggregate((s1, s2) => string.Join(", ", s1, s2))}");
                var asrc = fe.PossibleRawSrcPaths.Select(s => RenderString(templatePath, s, variables, logger, fe.Line, fe.Column)).FirstOrDefault(File.Exists);
                if (asrc == null)
                {
                    logger.Log(LogLevel.Error, templatePath, fe.Line, fe.Column,
                        $"Source file '{fe.RawRelativeSrc}' not found.");
                    return null;
                }
                logger.Log(LogLevel.Info, $"Rendering file '{asrc}'");
                fe.AbsoluteSrc = asrc;

                var adst = Path.Combine(dst, RenderString(templatePath, fe.RawRelativeDst, variables, logger, fe.Line, fe.Column));
                if (File.Exists(adst))
                {
                    logger.Log(LogLevel.Warning,
                        templatePath, fe.Line, fe.Column,
                        $"Tried to render file to '{adst}', but file exists. Check the template file for duplicate dst attributes.");
                    return null;
                }

                var dir = Path.GetDirectoryName(adst);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                if (fe.Raw)
                {
                    File.Copy(asrc, adst, true);
                }
                else
                {
                    var text = File.ReadAllText(asrc);
                    var rendered = RenderString(asrc, text, template.Variables, logger);
                    File.WriteAllText(adst, rendered);
                }
                Render?.Invoke(this, new RenderEventArgs(asrc, adst));
                return adst;
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, $"Error rendering '{fe.AbsoluteSrc ?? fe.RawRelativeSrc}':\n        {e.Message}");
                return null;
            }
        }

        public static string RenderString(string fileName, string src, VariableCollection vars, Logger logger, int lineOffset = 0, int colOffset = 0)
        {
            var sb = new StringBuilder();

            var sr = new StringReader(src);
            while (sr.ReadTo("{{", sb))
            {
                var line = sr.Line + lineOffset;
                var col = sr.Column + colOffset;
                string varName;
                if (!sr.ReadTo("}}", out varName))
                    logger.Log(LogLevel.Error, fileName, line, col, "No matching block end.");

                varName = varName.Trim();
                if (varName[0] == '#')
                {
                    // TODO if-then support
                    switch (varName.Substring(1))
                    {
                        case "newGuid":
                            var guid = Guid.NewGuid();
                            logger.Log(LogLevel.Verbose, fileName, line, col, $"Generated Guid '{guid}'.");
                            sb.Append(guid);
                            break;
                        case "year":
                            sb.Append(DateTime.Today.Year);
                            break;
                        default:
                            logger.Log(LogLevel.Warning, fileName, line, col, $"Unknown function '{varName}'.");
                            break;
                    }
                }
                else
                {
                    // it's a variable, replace it with its value
                    var vdata = vars.Get(varName);
                    if (vdata == null)
                    {
                        logger.Log(LogLevel.Error, fileName, line, col, $"Variable '{varName}' does not exist.");
                        continue;
                    }
                    if (string.IsNullOrEmpty(vdata.Value))
                    {
                        logger.Log(LogLevel.Warning, fileName, line, col, $"Variable '{varName}' not set.");
                    }
                    else
                    {
                        logger.Log(LogLevel.Verbose, fileName, line, col, $"Replaced variable '{varName}' with '{vdata.Value}'.");
                        sb.Append(vdata.Value);
                    }
                }
            }

            return sb.ToString();
        }

    }
}