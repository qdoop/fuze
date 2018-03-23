# FUZE *(wip)*

Project automation as a standalone dense .fsscript.

No additional package downloads/installs needed.<br>
Lightweight and portable plain scripting.


## Motivation
Provide selected functionality from

[gulpjs](https://gulpjs.com/), 
[nodemon](https://nodemon.io/),
[shelljs](http://documentup.com/shelljs/shelljs),
[fake5](https://fake.build/),
[workflow](https://github.com/deis/builder) ...

as a standalone dense F# script for most of my projects automation needs.

# Installation and Usage

Download [Nuget.exe](https://www.nuget.org/downloads) inside an empty folder and run  

```
 NuGet.exe install fuze 
```

Inside the extracted package folder `fuze.x.y.z` locate `fuze.x.y.z.fsscript`. 
Copy this file preferably at the root folder of your project.

This file is ALL you need. You can safely delete rest of the extracted files. 

**THIS FILE IS SAFE FOR USE IN A PRODUCTION ENVIROMENT. NEVER DOWNLOADS OR INSTALLS ADDITIONAL PACKAGES**

See `examples` how to use this file from your custom F# scripts.

We assume preinstalled a working version of [dotnet Sdk](https://www.microsoft.com/net), and [F# Sdk](http://fsharp.org/) 
 


# How it is build

The various independent scripts inside the src folder
are stripped and concatenated as a single large amalgamation file deliberately named `.fsscript` to indicate its significance. 

To build this file just run

```
fsi.exe  tools/build_fuse.fsx
``` 

and package it with

```
NuGet.exe pack
```

## Building for Windows
See `tools` folder

## Building for Linux
See `tools` folder


## Quick and Dirty
I hope to mprove my F# knowledge through this project so if you see something really stupid please correct me.

This work serves basically my own needs and also as a proof of concept.

Just and functionality the quit and dirty way leaving clean up for latter.

Testing? hmmm... At least we have a folder.

# Copyright Notices

**Blocks of F# code may be included with minor changes from similar projects such as `FAKE` ...**

# License

Copyright (c) qdoop. All rights reserved.

Licensed under the [MIT](LICENSE.md) License.

