# MonoGame Project Generator (MGPG)

MGPG is a simple, lightweight tool to generate projects from templates.

## CLI

MGPG offers a simple command-line interface (CLI). The CLI offers two function
- Print information about a template. <br> Usage: `MGPG <template>`
- Generate a project from a template. <br>
Usage: `MGPG <template> <destinationDir> [<solution>] (<variable>:<value> )*` <br>
If the solution does not exist it will be created. Generated projects will be added to the solution.

## Template Format Specification

Templates are defined in XML.
The root element is [Template](#template).

### Template Element

The template element is the root element of a template specification.

#### Child Elements

| Name                            | Required | Multiple | Notes             |
|---------------------------------|----------|----------|-------------------|
| Name                            | No       | No       | /                 |
| Description                     | No       | No       | /                 |
| [SrcFolder](#srcfolder-element) | No       | Yes      | /                 |
| Icon                            | No       | No       | Only used by GUI. |
| PreviewImage                    | No       | No       | Only used by GUI. |
| [Var](#var-element)             | No       | Yes      | /                 |
| [Project](#project-element)     | Yes      | Yes      | /                 |

## SrcFolder Element

The folders relative to the template where the generator will look for 
source files. The folder containing the template is used when this 
element is omitted. The generator will look for a source file in each 
of these folders in order.

### Var Element

Define a variable. Use the content to set a default value.

#### Attributes

- **name**: Name of the variable.
-  **type**: Hint for the variable type. One of string, boolean.
- **hidden**: If set to "true" this variable will not be shown when printing template details using the CLI.
- **semantic**: Unused.

### Project Element

A project to include in the template.

#### Attributes

- **src**: Source location of the project file. (may contain variables and functions). Required.
- **dst**: Target location of the project file. (may contain variables and functions). Defaults to src.
- **raw**: Set to true to not replace the contents of a project file. Defaults to false.

#### Child Elements

- [File](#file-element)

### File Element

A file to include in the template.

#### Attributes

- **src**: Source location of the project file. (may contain variables and functions). Required.
- **dst**: Target location of the project file. (may contain variables and functions). Defaults to src.
- **raw**: Set to true to not replace the contents of a project file. Defaults to false.

### Source Language

The generator takes a SourceLanguage as an argument. The source language can be CSharp, FSharp or VisualBasic. For convenience a variable is added with the extension of source files for the used source language (.cs for CSharp, .fs for FSharp and .vb for Visual Basic).

