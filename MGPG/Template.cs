// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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
        internal const string SourceFileExtensionVariable = "_sourceExt";

        private const string NameElement = "Name";
        private const string DescriptionElement = "Description";
        private const string IconElement = "Icon";
        private const string PreviewImageElement = "PreviewImage";
        private const string SourceFolderElement = "SrcFolder";
        private const string VariableElement = "Var";
        private const string ProjectElement = "Project";
        private const string FileElement = "File";

        private const string VarNameAttrib = "name";
        private const string VarSemanticAttrib = "semantic";
        private const string VarTypeAttrib = "type";
        private const string VarHiddenAttrib = "hidden";
        private const string VarDescrAttrib = "description";

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

        public string Directory => Path.GetDirectoryName(FullPath);

        public List<ProjectEntry> ProjectEntries;

        public Template(string fullPath, Logger logger)
        {
            FullPath = fullPath;
            Variables = new VariableCollection();
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

            logger.Log(LogLevel.Info, $"Using source folders '{srcDirs.Aggregate((s1, s2) => string.Join(", ", s1, s2))}'");

            foreach (var src in srcDirs)
            {
                if (!System.IO.Directory.Exists(src))
                {
                    logger.Log(LogLevel.Error, $"Source folder not found at '{src}'.");
                    HasFatalError = true;
                    return;
                }
            }

            Variables.Add(SourceFileExtensionVariable, "Extension of source files.", SourceLanguage.CSharp.GetFileExtension(), null, VariableType.String, true);
            foreach (var ve in te.Elements(VariableElement))
            {
                var name = ve.Attribute(VarNameAttrib)?.Value;
                var description = ve.Attribute(VarDescrAttrib)?.Value;
                var value = ve.Value;
                if (name == null)
                    logger.Log(LogLevel.Warning, path, ve, $"Variable is missing '{VarNameAttrib}' attribute.");
                else
                {
                    var semantic = ve.Attribute(VarSemanticAttrib)?.Value;
                    var ta = ve.Attribute(VarTypeAttrib);
                    var type = VariableType.String;
                    if (ta != null)
                    {
                        if (!Enum.TryParse(ta.Value, true, out type))
                            logger.Log(LogLevel.Error, FullPath, ta, $"Invalid variable type '{ta.Value}'.");
                    }

                    var hidden = Util.IsTrue(ve.Attribute(VarHiddenAttrib)?.Value); 
                    Variables.Add(name, description, value, semantic, type, hidden);
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
            var rsrcs = srcDirs.Select(s => Path.Combine(s, rawsrc));
            var rdst = element.Attribute(DestinationAttrib)?.Value ?? rawsrc;

            var rawstr = element.Attribute(RawAttrib)?.Value;
            // raw defaults to false
            var raw = Util.IsTrue(rawstr);
            return new FileEntry(rawsrc, rsrcs, rdst, raw, ((IXmlLineInfo) element).LineNumber, ((IXmlLineInfo) element).LinePosition);
        }
    }

    public class FileEntry
    {
        public string RawRelativeSrc;
        public IEnumerable<string> PossibleRawSrcPaths { get; }
        public string RawRelativeDst { get; }
        public bool Raw { get; }
        public int Line { get; }
        public int Column { get; }

        public string AbsoluteSrc { get; set; }

        public FileEntry(string rawRelativeSrc, IEnumerable<string> possibleRawSrcPaths, string rawRelativeDst, bool raw, int line, int column)
        {
            RawRelativeSrc = rawRelativeSrc;
            PossibleRawSrcPaths = possibleRawSrcPaths;
            RawRelativeDst = rawRelativeDst;
            Raw = raw;
            Line = line;
            Column = column;
        }
    }

    public class ProjectEntry : FileEntry
    {
        public List<FileEntry> FileEntries { get; }

        public ProjectEntry(FileEntry fe, List<FileEntry> fileEntries)
            : base(fe.RawRelativeSrc, fe.PossibleRawSrcPaths, fe.RawRelativeDst, fe.Raw, fe.Line, fe.Column)
        {
            FileEntries = fileEntries;
        }
    }

}