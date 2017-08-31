﻿using System;
using System.IO;
using System.Linq;

namespace MGPG.IdeTemplateWriters
{
    public abstract class IdeTemplateWriter
    {
        public void Write(string templateFile, string outputFolder, Settings settings)
        {
            var logger = new Logger
            {
                LogLevel = settings.LogLevel,
                LogWriter = settings.LogWriter ?? Console.Out,
                SupressErrors = settings.SupressErrors
            };
            if (File.Exists(outputFolder) ||
                (Directory.Exists(outputFolder) && Directory.EnumerateFileSystemEntries(outputFolder).Any() && !settings.Overwrite))
            {
                logger.Log(LogLevel.Error, $"The destination directory '{outputFolder}' already exists and is not empty. Set the Overwrite flag to write anyway.");
                return;
            }
            if (!File.Exists(templateFile))
            {
                logger.Log(LogLevel.Error, $"Template file not found at '{templateFile}'.");
                return;
            }

            templateFile = Path.GetFullPath(templateFile);
            var template = new Template(templateFile, logger);
            if (template.HasFatalError)
                return;

            outputFolder = Path.GetFullPath(outputFolder);
            WriteIdeTemplate(template, outputFolder, logger);
        }

        public abstract void WriteIdeTemplate(Template template, string outputFolder, Logger logger);
    }
}