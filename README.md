# PawnBuild

## Intro
A very simple yet powerful command line tool which can be used to build Pawn scripts in the form of a project, using a json-based configuration file. 

It needs the pawn compiler (in PATH or in the same directory).
Working on Windows, Linux and MacOSX.

## Usage
```
PawnBuild <buildFileName.json> [options]
```

## Options
```
-r (--run): executes the run instruction after building
-v (--verbose): prints more information
-f (--force): does not skip any file
```

## Example build.json file
```
{
    "ProjectName" : "Roleplay",
    "BuildFolders" : [
        {
            "SourceFolder" : "src\\gamemodes",
            "OutputFolder" : "bin\\gamemodes"
        },
        {
            "SourceFolder" : "src\\filterscripts",
            "OutputFolder" : "bin\\filterscripts"
        },
        {
            "SourceFolder" : "src\\npcmodes",
            "OutputFolder" : "bin\\npcmodes"
        }
    ],

    "IncludeFolders" : [
        "C:\\pawncc\\include",
        "src\\include"
    ],

    "Files" : [
        "RP.pwn",
        "RP_Admin.pwn",
        "RP_Clothes.pwn",
        "RP_Dealers.pwn",
        "RP_Houses.pwn",
        "RP_Phone.pwn",
        "RP_NPC_Intro1.pwn"
    ],

    "Args" : "-O1 -;+ -(+ -w239 -w214",

    "Run" : [
        "bin\\samp-server.exe",
        "bin\\debug.exe"
    ]
}
```

**ProjectName**: the displayed project name.

**BuildFolders**: every `BuildFolder` entry contains a source and an output folder, which defines that files found in the source folder should be compiled to the output folder. I personally prefer to keep the source separate from the binaries, but if you prefer the default SA-MP server folder structure, you would use the same name for both the source and output settings.

**IncludeFolders**: defines the folders where the compiler should search for include files.

**Files**: the list of source code file names, without directory and with the extension.

**Args**: any additional args you wish to pass to the compiler. For example, pass -w239 to disable warning 239.

**Run**: the executable files to run after completion, only if the -r arg is passed to the program.

## Example output
```
------ Build started - Project: Roleplay ------
* Compiling src\gamemodes\RP.pwn -> bin\gamemodes\RP.amx
    src\gamemodes\RP.pwn(271) : warning 204: symbol is assigned a value that is never used: "mapandreasAddress"
    Pawn compiler 3.10.10                       Copyright (c) 1997-2006, ITB CompuPhase


    1 Warning.

* Compiling src\filterscripts\RP_Admin.pwn -> bin\filterscripts\RP_Admin.amx
* Compiling src\filterscripts\RP_Clothes.pwn -> bin\filterscripts\RP_Clothes.amx
    src\filterscripts\RP_Clothes.pwn(475) : error 017: undefined symbol "tmpScore"
    src\filterscripts\RP_Clothes.pwn(475) : warning 215: expression has no effect
    Pawn compiler 3.10.10                       Copyright (c) 1997-2006, ITB CompuPhase


    1 Error.

* Skipping src\filterscripts\RP_Dealers.pwn
* Skipping src\filterscripts\RP_Friendship.pwn
* Skipping src\filterscripts\RP_Drugs.pwn
* Skipping src\filterscripts\RP_Houses.pwn
* Skipping src\filterscripts\RP_Phone.pwn
* Skipping src\npcmodes\RP_NPC_Intro1.pwn
========== Build: 2 succeeded, 1 failed, 6 skipped ==========
```

If no errors or warnings were encountered, the pawncc output is ignored, otherwise it is shown after the corresponding line.
If a file has been skipped it means that it had no changes since the last build (the .amx file is more recent than the source file).

## Configuring VSCode
VSCode can be configure to build the script via this program in the tasks.json file.
To open this file, simply go to the folder of your project and locate the .vscode folder, or more simply by clicking Task->Configure Tasks in the VSCode menu.

The following is an example tasks.json:
```
{
    "version": "2.0.0",
    "tasks": [
        {
            "label": "Build",
            "type": "shell",
            "command": "PawnBuild.exe",
            "args": [ "${workspaceRoot}\\build.json" ],
            "group": {
                "kind": "build",
                "isDefault": true
            }
        },
        {
            "label": "Build and Run",
            "type": "shell",
            "command": "PawnBuild.exe",
            "args": [ "${workspaceRoot}\\build.json", "-r" ],
            "group": {
                "kind": "test",
                "isDefault": true
            }
        }
    ]
}
```
