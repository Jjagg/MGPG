using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace MGPG
{
    internal class Template
    {
        private const string SourceFolderAttrib = "srcFolder";
        private const string VariableElement = "Var";
        private const string FileElement = "File";
        private const string NameAttrib = "name";
        private const string ValueAttrib = "value";
        private const string SourceAttrib = "src";
        private const string DestinationAttrib = "dst";
        private const string RawAttrib = "raw";

        public bool HasFatalError { get; private set; }

        public VariableCollection Variables { get; }
        public List<FileEntry> FileEntries { get; }

        public Template(string path, Logger logger)
        {
            Variables = new VariableCollection();
            FileEntries = new List<FileEntry>();
            Create(path, logger);
        }

        private void Create(string path, Logger logger)
        {
            var xdoc = XDocument.Load(path, LoadOptions.SetLineInfo);
            var te = xdoc.Element("Template");
            if (te == null)
            {
                logger.Log(LogLevel.Error, "Template file does not have a 'Template' element.");
                HasFatalError = true;
                return;
            }

            IEnumerable<string> srcDirs;
            if (te.Attribute(SourceFolderAttrib) == null)
            {
                srcDirs = new[] {Path.GetDirectoryName(path)};
            }
            else
            {
                var relativeSrcs = te.Attribute(SourceFolderAttrib).Value.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
                srcDirs = relativeSrcs.Select(s => Path.Combine(Path.GetDirectoryName(path), s));
            }

            logger.Log(LogLevel.Info, path, te, $"Using source folders '{srcDirs.Aggregate((s1, s2) => string.Join(", ", s1, s2))}'");

            foreach (var src in srcDirs)
            {
                if (!Directory.Exists(src))
                {
                    logger.Log(LogLevel.Error, $"Source folder not found at '{src}'.");
                    HasFatalError = true;
                    return;
                }
            }


            foreach (var ve in te.Elements(VariableElement))
            {
                var name = ve.Attribute(NameAttrib)?.Value;
                var value = ve.Attribute(ValueAttrib)?.Value ?? string.Empty;
                if (name == null)
                    logger.Log(LogLevel.Warning, path, ve, $"Variable is missing '{NameAttrib}' attribute.");
                else
                    Variables.Set(name, value);
            }

            var projectPaths = new List<string>();

            foreach (var fe in te.Elements(FileElement))
            {
                var rsrc = fe.Attribute(SourceAttrib)?.Value;
                if (string.IsNullOrEmpty(rsrc))
                {
                    logger.Log(LogLevel.Error, path, fe, "File elements must at least have a src attribute.");
                    continue;
                }
                var asrc = srcDirs.Select(s => Path.Combine(s, rsrc)).FirstOrDefault(File.Exists);
                if (asrc == null)
                {
                    logger.Log(LogLevel.Error, path, fe.Attribute(SourceAttrib), $"Source file '{rsrc}' not found.");
                    continue;
                }

                // default to same relative path as source file for destination
                if (fe.Attribute(DestinationAttrib) == null && Path.IsPathRooted(rsrc))
                {
                    logger.Log(LogLevel.Error, path, fe,
                        "Destination path is not set; defaulting to same relative path as source, but source is rooted.");
                    continue;
                }
                var rdst = fe.Attribute(DestinationAttrib)?.Value ?? rsrc;
                /*fdst = Path.GetFullPath(Path.Combine(dst, fdst));

                if (File.Exists(fdst))
                {
                    logger.Log(LogLevel.Error,
                        path, fe.Attribute(DestinationAttrib) ?? fe.Attribute(SourceAttrib),
                        $"Tried to render file to '{fdst}', but file exists. Check the template file for duplicate dst attributes.");
                    continue;
                }*/

                var rawstr = fe.Attribute(RawAttrib)?.Value;
                // raw defaults to false
                var raw = rawstr != null && Util.IsTrue(rawstr);
                FileEntries.Add(new FileEntry(asrc, rdst, raw, ((IXmlLineInfo)fe).LineNumber, ((IXmlLineInfo)fe).LinePosition));
            }

        }
    }

    internal class FileEntry
    {
        public string Source { get; }
        public string Destination { get; }
        public bool Raw { get; }
        public int Line { get; }
        public int Column { get; }

        public FileEntry(string source, string destination, bool raw, int line, int column)
        {
            Source = source;
            Destination = destination;
            Raw = raw;
            Line = line;
            Column = column;
        }

    }
}