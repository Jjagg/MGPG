using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace MGPG
{
    public class Template
    {
        private const string NameElement = "Name";
        private const string DescriptionElement = "Description";
        private const string IconElement = "Icon";
        private const string PreviewImageElement = "PreviewImage";
        private const string SourceFolderElement = "SrcFolder";
        private const string VariableElement = "Var";
        private const string ProjectElement = "Project";
        private const string FileElement = "File";
        private const string NameAttrib = "name";
        private const string ValueAttrib = "value";
        private const string TypeAttrib = "type";
        private const string SourceAttrib = "src";
        private const string DestinationAttrib = "dst";
        private const string RawAttrib = "raw";

        public bool HasFatalError { get; private set; }

        public string FullPath { get; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Icon { get; private set; }
        public string PreviewImage { get; private set; }

        public VariableCollection Variables { get; }
        public Dictionary<string, string> VariableTypes { get; }
        public List<ProjectEntry> ProjectEntries;

        public Template(string fullPath, Logger logger)
        {
            FullPath = fullPath;
            Variables = new VariableCollection();
            VariableTypes = new Dictionary<string, string>();
            ProjectEntries = new List<ProjectEntry>();
            Create(fullPath, logger);
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

            Name = te.Element(NameElement)?.Value;
            Description = te.Element(DescriptionElement)?.Value;
            Icon = te.Element(IconElement)?.Value;
            PreviewImage = te.Element(PreviewImageElement)?.Value;

            IEnumerable<string> srcDirs;
            if (te.Element(SourceFolderElement) == null)
            {
                srcDirs = new[] {string.Empty};
            }
            else
            {
                srcDirs = te.Elements(SourceFolderElement).Select(s => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), s.Value)));
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
                {
                    Variables.Set(name, value);
                    var ta = ve.Attribute(TypeAttrib);
                    if (ta != null)
                        VariableTypes.Add(name, ta.Value);
                }
            }

            foreach (var pe in te.Elements(ProjectElement))
            {
                var project = ParseFileEntry(path, pe, srcDirs, logger);

                var fes = pe.Elements(FileElement)
                    .Select(fe => ParseFileEntry(path, fe, srcDirs, logger))
                    .Where(file => file != null)
                    .ToList();

                ProjectEntries.Add(new ProjectEntry(project, fes));
            }
        }

        private static FileEntry ParseFileEntry(string templatePath, XElement element, IEnumerable<string> srcDirs, Logger logger)
        {
            var rawsrc = element.Attribute(SourceAttrib)?.Value;
            if (string.IsNullOrEmpty(rawsrc) || Path.IsPathRooted(rawsrc))
            {
                logger.Log(LogLevel.Error, templatePath, element, "File elements must at least have a src attribute which may not be a rooted path.");
                return null;
            }
            var rsrc = srcDirs.Select(s => Path.Combine(s, rawsrc)).FirstOrDefault(File.Exists);
            if (rsrc == null)
            {
                logger.Log(LogLevel.Error, templatePath, element.Attribute(SourceAttrib),
                    $"Source file '{element.Attribute(SourceAttrib).Value}' not found.");
                return null;
            }

            var rdst = element.Attribute(DestinationAttrib)?.Value ?? rawsrc;

            var asrc = Path.Combine(Path.GetDirectoryName(templatePath), rsrc);
            var rawstr = element.Attribute(RawAttrib)?.Value;
            // raw defaults to false
            var raw = rawstr != null && Util.IsTrue(rawstr);
            return new FileEntry(rawsrc, asrc, rdst, raw, ((IXmlLineInfo) element).LineNumber, ((IXmlLineInfo) element).LinePosition);
        }
    }

    public class FileEntry
    {
        public string Rsrc { get; }
        public string Asrc { get; }
        public string Rdst { get; }
        public bool Raw { get; }
        public int Line { get; }
        public int Column { get; }

        public FileEntry(string rsrc, string asrc, string rdst, bool raw, int line, int column)
        {
            Rsrc = rsrc;
            Asrc = asrc;
            Rdst = rdst;
            Raw = raw;
            Line = line;
            Column = column;
        }
    }

    public class ProjectEntry : FileEntry
    {
        public List<FileEntry> FileEntries { get; }
        public ProjectEntry(FileEntry fe, List<FileEntry> fileEntries)
            : base(fe.Rsrc, fe.Asrc, fe.Rdst, fe.Raw, fe.Line, fe.Column)
        {
            FileEntries = fileEntries;
        }
    }

}