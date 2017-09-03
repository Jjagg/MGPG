// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.DotNet.Cli.Sln.Internal;
using Microsoft.DotNet.Tools.Common;

namespace MGPG
{
    /// <summary>
    /// Core class that generates a project from a template when <see cref="Generate"/> is called.
    /// </summary>
    public class Generator
    {
        private readonly Template _template;
        private Logger _logger;

        /// <summary>
        /// Get or set the configuration of this <see cref="Generator"/>.
        /// </summary>
        public Settings Settings { get; set; }

        /// <summary>
        /// Invoked when a file is written to disk after a succesful render or copy operation.
        /// </summary>
        public event EventHandler<RenderEventArgs> FileWritten;

        /// <summary>
        /// Create a <see cref="Generator"/> with default <see cref="MGPG.Settings"/>.
        /// </summary>
        /// <param name="templatePath">Path to the file that describes the template to generate.</param>
        public Generator(string templatePath)
        {
            Settings = new Settings();

            if (!File.Exists(templatePath))
                throw new ArgumentException($"Template file not found at '{templatePath}'.", nameof(templatePath));
            templatePath = Path.GetFullPath(templatePath);
            _template = new Template(templatePath, _logger);
        }

        /// <summary>
        /// Generate the template defined in the template file set in <see cref="Settings"/>.
        /// </summary>
        public void Generate(GeneratorArguments arguments)
        {
            _logger = new Logger
            {
                LogLevel = Settings.LogLevel,
                LogWriter = Settings.LogWriter ?? Console.Out,
                SupressErrors = Settings.SupressErrors
            };
            if (_template == null)
                return;

            if (File.Exists(arguments.DestinationFolder) ||
                (Directory.Exists(arguments.DestinationFolder) && Directory.EnumerateFileSystemEntries(arguments.DestinationFolder).Any() && !Settings.Overwrite))
            {
                _logger.Log(LogLevel.Error, $"The destination directory '{arguments.DestinationFolder}' already exists and is not empty. Set the Overwrite flag to write anyway.");
                return;
            }

            var dst = Path.GetFullPath(arguments.DestinationFolder);

            if (_template.HasFatalError)
                return;

            var variables = _template.Variables.With(arguments.Variables);
            variables.Set(Template.SourceFileExtensionVariable, arguments.SourceLanguage.GetFileExtension());

            Directory.CreateDirectory(dst);
            var projectPaths = new List<string>();
            foreach (var pe in _template.ProjectEntries)
            {
                var adst = RenderFileEntry(pe, variables, dst);
                _logger.Log(LogLevel.Info, $"Rendered project '{pe.AbsoluteSrc}' to '{adst}'.");
                projectPaths.Add(adst);
                foreach (var fe in pe.FileEntries)
                {
                    adst = RenderFileEntry(fe, variables, dst);
                    _logger.Log(LogLevel.Info, $"Rendered file '{fe.AbsoluteSrc}' to '{adst}'.");
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
                    _logger.Log(LogLevel.Info, $"Created solution '{Path.GetFileName(slnPath)}'.");
                }
                foreach (var projectPath in projectPaths)
                {
                    sln.AddProject(projectPath);
                    _logger.Log(LogLevel.Info, $"Added project '{Path.GetFileName(projectPath)}' to solution '{Path.GetFileName(slnPath)}'.");
                }
                sln.Write();
            }
        }

        private string RenderFileEntry(FileEntry fe, VariableCollection variables, string dst)
        {
            try
            {
                _logger.Log(LogLevel.Verbose, $"Looking for {fe.RawRelativeSrc} at {fe.PossibleRawSrcPaths.Aggregate((s1, s2) => string.Join(", ", s1, s2))}");
                var asrc = fe.PossibleRawSrcPaths.Select(s => RenderString(_template.FullPath, s, variables, fe.Line, fe.Column)).FirstOrDefault(File.Exists);
                if (asrc == null)
                {
                    _logger.Log(LogLevel.Error, _template.FullPath, fe.Line, fe.Column,
                        $"Source file '{fe.RawRelativeSrc}' not found.");
                    return null;
                }
                fe.AbsoluteSrc = asrc;

                var adst = Path.Combine(dst, RenderString(_template.FullPath, fe.RawRelativeDst, variables, fe.Line, fe.Column));
                if (File.Exists(adst))
                {
                    _logger.Log(LogLevel.Warning,
                        _template.FullPath, fe.Line, fe.Column,
                        $"Tried to render file to '{adst}', but file exists. Check the template file for duplicate dst attributes.");
                    return null;
                }

                var dir = Path.GetDirectoryName(adst);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                if (fe.Raw)
                {
                    _logger.Log(LogLevel.Info, $"Copying file '{asrc}' to '{adst}'.");
                    File.Copy(asrc, adst, true);
                }
                else
                {
                    _logger.Log(LogLevel.Info, $"Rendering file '{asrc}'.");
                    var text = File.ReadAllText(asrc);
                    var rendered = RenderString(asrc, text, variables);
                    _logger.Log(LogLevel.Verbose, $"Writing file '{asrc}' to '{adst}'.");
                    File.WriteAllText(adst, rendered);
                }
                FileWritten?.Invoke(this, new RenderEventArgs(asrc, adst));
                return adst;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error rendering '{fe.AbsoluteSrc ?? fe.RawRelativeSrc}':\n        {e.Message}");
                return null;
            }
        }

        public string RenderString(string fileName, string src, VariableCollection vars, int lineOffset = 0, int colOffset = 0)
        {
            var sb = new StringBuilder();

            var sr = new StringReader(src);
            while (sr.ReadTo("{{", sb))
            {
                var line = sr.Line + lineOffset;
                var col = sr.Column + colOffset;
                string varName;
                if (!sr.ReadTo("}}", out varName))
                    _logger.Log(LogLevel.Error, fileName, line, col, "No matching block end.");

                varName = varName.Trim();
                if (varName[0] == '#')
                {
                    // TODO if-then support
                    switch (varName.Substring(1))
                    {
                        case "newGuid":
                            var guid = Guid.NewGuid();
                            _logger.Log(LogLevel.Verbose, fileName, line, col, $"Generated Guid '{guid}'.");
                            sb.Append(guid);
                            break;
                        case "year":
                            sb.Append(DateTime.Today.Year);
                            break;
                        default:
                            _logger.Log(LogLevel.Warning, fileName, line, col, $"Unknown function '{varName}'.");
                            break;
                    }
                }
                else
                {
                    // it's a variable, replace it with its value
                    var vdata = vars.Get(varName);
                    if (vdata == null)
                    {
                        _logger.Log(LogLevel.Error, fileName, line, col, $"Variable '{varName}' does not exist.");
                        continue;
                    }
                    if (string.IsNullOrEmpty(vdata.Value))
                    {
                        _logger.Log(LogLevel.Warning, fileName, line, col, $"Variable '{varName}' not set.");
                    }
                    else
                    {
                        _logger.Log(LogLevel.Verbose, fileName, line, col, $"Replaced variable '{varName}' with '{vdata.Value}'.");
                        sb.Append(vdata.Value);
                    }
                }
            }

            return sb.ToString();
        }

    }
}