using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MGPG.IdeTemplateWriters
{
    public class VsTemplateWriter : IdeTemplateWriter
    {
        public override void WriteIdeTemplate(Template template, string outputFolder, Logger logger)
        {
            if (template.ProjectEntries.Count > 1)
            {
                // https://msdn.microsoft.com/en-us/library/ms185308.aspx
                logger.Log(LogLevel.Error, "VS templates with more than 1 project are not yet supported.");
            }

            var guidNr = 1;

            var rootDir = new VfsDirectory();
            rootDir.FilePath = string.Empty;

            foreach (var pe in template.ProjectEntries)
            {
                var pfile = new VfsFile(pe) {FilePath = pe.Rsrc};
                rootDir.Add(pfile);

                foreach (var fe in pe.FileEntries)
                {
                    var file = new VfsFile(fe) {FilePath = fe.Rsrc};
                    rootDir.Add(file);
                }
            }

            rootDir.Write(outputFolder,
                (path, fe) =>
                {
                    if (fe.Raw)
                    {
                        File.Copy(fe.Asrc, path, true);
                    }
                    else
                    {
                        var content = File.ReadAllText(fe.Asrc);
                        content = RenderString(template, content, logger, ref guidNr);
                        File.WriteAllText(path, content);
                    }
                }
            );

            var ns = XNamespace.Get("http://schemas.microsoft.com/developer/vstemplate/2005");
            var pes = template.ProjectEntries.Select(e => ToVsElement(e, ns));

            var xdoc = new XDocument(
                new XElement(ns + "VSTemplate",
                  new XAttribute("Version", "3.0.0"),
                  new XAttribute("Type", "Project"),
                  
                  new XElement(ns + "TemplateData",
                    new XElement(ns + "Name", template.Name),
                    new XElement(ns + "Description", template.Description),
                    new XElement(ns + "ProjectType", "CSharp"),
                    new XElement(ns + "NumberOfParentCategoriesToRollUp", 1),
                    new XElement(ns + "SortOrder", 43100),
                    new XElement(ns + "CreateNewFolder", true),
                    new XElement(ns + "DefaultName", "Game"),
                    new XElement(ns + "ProvideDefaultName", true),
                    new XElement(ns + "LocationField", "Enabled"),
                    new XElement(ns + "EnableLocationBrowseButton", true),
                    new XElement(ns + "Icon", Path.GetFileName(template.Icon)),
                    new XElement(ns + "PreviewImage", Path.GetFileName(template.PreviewImage))
                  ),
                  new XElement(ns + "TemplateContent", pes)
                )
            );
            xdoc.Save(Path.Combine(outputFolder, "template.vstemplate"));
        }

        private string RenderString(Template template, string src, Logger logger, ref int guidNr, int lineOffset = 0, int colOffset = 0)
        {
            var sb = new StringBuilder();

            var sr = new StringReader(src);
            while (sr.ReadTo("{{", sb))
            {
                var line = sr.Line + lineOffset;
                var col = sr.Column + colOffset;
                string varName;
                if (!sr.ReadTo("}}", out varName))
                    logger.Log(LogLevel.Error, template.FullPath, line, col, "No matching block end.");

                varName = varName.Trim();
                if (varName[0] == '#')
                {
                    switch (varName.Substring(1))
                    {
                        case "newGuid":
                            sb.Append($"$guid{guidNr}$");
                            guidNr++;
                            break;
                        case "year":
                            sb.Append("$year$");
                            break;
                        default:
                            logger.Log(LogLevel.Warning, template.FullPath, line, col, $"Unknown function '{varName}'.");
                            break;
                    }
                }
                else
                {
                    string type;
                    sb.Append(template.VariableTypes.TryGetValue(varName, out type)
                        ? $"${ToVsReservedVariable(type)}$"
                        : template.Variables.Get(varName));
                }
            }

            return sb.ToString();
        }

        private string ToVsReservedVariable(string type)
        {
            switch (type)
            {
                case "projectName":
                    return "safeprojectname";
                case "organization":
                    return "registeredorganization";
                default:
                    throw new ArgumentException("Unknown variable type");
            }
        }

        private static XElement ToVsElement(VfsFileSystemEntry e, XNamespace ns)
        {
            if (e is VfsDirectory)
                return ToVsElement((VfsDirectory) e, ns);
            return ToVsElement((VfsFile) e, ns);
        }

        private static XElement ToVsElement(VfsDirectory dir, XNamespace ns)
        {
            Console.WriteLine($"Creating dir {dir}");
            var folder = new XElement(ns + "Folder",
                new XAttribute("Name", dir.Name),
                new XAttribute("TargetFileName", dir.Name));

            foreach (var fe in dir.Entries.Select(e => ToVsElement(e, ns)))
                folder.Add(fe);
            return folder;
        }

        private static XElement ToVsElement(VfsFile file, XNamespace ns)
        {
            return ToVsElement(file.FileEntry, ns);
        }

        private static XElement ToVsElement(FileEntry fe, XNamespace ns)
        {
            XElement xe;
            var name = Path.GetFileName(fe.Rsrc);
            if (fe is ProjectEntry)
            {
                xe = new XElement(ns + "Project");
                foreach (var childEntry in ((ProjectEntry) fe).FileEntries)
                    xe.Add(ToVsElement(childEntry, ns));
            }
            else
            {
                xe = new XElement(ns + "ProjectItem", name);
            }

            xe.Add(new XAttribute("ReplaceParameters", !fe.Raw));
            xe.Add(new XAttribute("TargetFileName", name));
            return xe;
        }
    }
}