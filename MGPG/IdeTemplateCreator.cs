using System;
using System.IO;

namespace MGPG
{
    public abstract class IdeTemplateCreator
    {
        public void Create(string templateFile, string outputFolder, Settings settings)
        {
            var logger = new Logger
            {
                LogLevel = settings.LogLevel,
                LogWriter = settings.LogWriter ?? Console.Out,
                SupressErrors = settings.SupressErrors
            };
            if (!File.Exists(templateFile))
            {
                logger.Log(LogLevel.Error, $"Template file not found at '{templateFile}'.");
                return;
            }

            var template = new Template(templateFile, logger);
        }

        public abstract string MapVariable();
        public abstract string MapFunction(string fun);
    }
}