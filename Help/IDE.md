# IDE Configuration

An Integrated Development Environment (IDE) which works well with this repository is Visual Studio Code (VSC) by Microsoft. It is a free open source IDE that works on Linux and can be configured to build and debug dotnet core solutions effectively with the proper extensions installed.

## Visual Studio Code Installation

Up to date versions of VSC can be downloaded from its [official website](https://code.visualstudio.com/). If you are running a Debian based distribution after downloading you can install VSC by running this command and replacing the version number with your actual download version number:

````console
sudo dpkg -i code_version_number.deb
````

You can also use the above command to update VSC as newer versions become available.

## Extensions

The following extensions are either required or suggested by VSC to work with dotnet core and code from this repository.

To install these extensions you may using the VSC command pallet (Ctrl+P).

<details>
  <summary>List of VSC required or suggested extensions</summary>

### [C# for Visual Studio Code (powered by OmniSharp)](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)

This extension is required to build C# projects in VSC.

Install using:

````ext install ms-dotnettools.csharp```` 

### [Visual Studio Code Solution Explorer](https://marketplace.visualstudio.com/items?itemName=fernandoescolar.vscode-solution-explorer)

This extension allows users to open and manage C# solutions in VSC. 

Install using:

````ext install fernandoescolar.vscode-solution-explorer```` 

### [Roslynator](https://marketplace.visualstudio.com/items?itemName=josefpihrt-vscode.roslynator)

This extension contains a collection of more than 500 analyzers, refactorings, and fixes for C#. It is powered by Roslyn code analysis APIs. 

Install using:

````ext install josefpihrt-vscode.roslynator```` 

### [Numbered Bookmarks](https://marketplace.visualstudio.com/items?itemName=alefragnani.numbered-bookmarks)

This extension helps you to navigate in your code using numbered bookmarks. Moving between important positions easily and quickly.

Install using:

````ext install alefragnani.numbered-bookmarks```` 

### [Code Spell Checker](https://marketplace.visualstudio.com/items?itemName=streetsidesoftware.code-spell-checker)

This extension contains a checks the spelling of your code and comments. 

Install using:

````ext install streetsidesoftware.code-spell-checker```` 

### [C# XML Documentation Comments](https://marketplace.visualstudio.com/items?itemName=k--kato.docomment)

This extension allows you to adorn you code with documentation comments. 

Install using:

````ext install k--kato.docomment```` 

</details>

## Building Tasks

You can edit your workspace ``.vscode/tasks.json`` to combine additional tasks to be run after building a project with VSC. Be sure to replace PROJECT withe the name of the project within your solution which you want to build.

<details>
  <summary>Example of a build and deploy task configuration</summary>

````json
{
    "version": "2.0.0",
    "tasks": [
        {
            "label": "build",
            "command": "dotnet",
            "type": "process",
            "args": [
                "build",
                "/property:GenerateFullPaths=true",
                "/consoleloggerparameters:NoSummary"
            ],
            "options": {
                "cwd": "${workspaceFolder}/PROJECT"
            },      
            "problemMatcher": "$msCompile",
            "group": {
                "kind": "build",
                "isDefault": true
            }
        },
        {
            "label": "build and deploy",
            "command": "deploy",
            "type": "process",
            "options": {
                "cwd": "${workspaceFolder}/PROJECT"
            },      
            "dependsOn": [
                "build"
            ],
            "problemMatcher": [],
            "group": "build"
        }
    ]
}
````

</details>

## Debug Launching 

You can edit your workspace ``.vscode/launch.json`` to allow the special built Microsoft debugger to launch and debug your projects. In most cases you will not be doing this as you will be writing code on your Desktop computer and running on your Raspberry Pi. But if you would like to test some non Pi specific project you can make these changes being sure to replace PROJECT withe the name of the project within your solution which you want to debug.

<details>
  <summary>Example of a launch and debug a console project configuration</summary>

````json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Launch (console)",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            // If you have changed target frameworks, make sure to update the program path.
            "program": "${workspaceFolder}/PROJECT/bin/Debug/netcoreapp3.1/Test.dll",
            "args": [],
            "cwd": "${workspaceFolder}/PROJECT,
            "console": "externalTerminal",
            "stopAtEntry": false
        }
    ]
}
````

</details>
<details>
  <summary>Example of a launch and debug a web project configuration</summary>

````json
{
   "version": "0.2.0",
   "configurations": [
        {
            "name": ".NET Core Launch (web)",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            // If you have changed target frameworks, make sure to update the program path.
            "program": "${workspaceFolder}/PROJECT/bin/Debug/netcoreapp3.1/PROJECT.dll",
            "args": [],
            "cwd": "${workspaceFolder}/PROJECT",
            "stopAtEntry": false,
            // Enable launching a web browser when ASP.NET Core starts
            // For more information: https://aka.ms/VSCode-CS-LaunchJson-WebBrowser
            "serverReadyAction": {
                "action": "openExternally",
                "pattern": "^\\s*Now listening on:\\s+(https?://\\S+)"
            },
            "env": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            },
            "sourceFileMap": {
                "/Views": "${workspaceFolder}/Views"
            }
        }
    ]
}
````

</details>

### See also

[Table of Contents](README.md)

