
open System.IO

(*

  nodemon -e fsx   -w tools  --exec   cmd /C tools\build.cmd

*)


printfn "///////Building..."

let files =[
    "Core/String.fs"
    "shell/Path.fs"
    "Shell/FileSystemOperators.fs"
    "Shell/DirectoryInfo.fs"
    "Shell/FileInfo.fs"
    "Shell/Directory.fs" 
    "Shell/File.fs"
    "Shell/FileSystemInfo.fs"
    "Shell/Globbing.fs"
    "Shell/GlobbingFileSystem.fs"
    "Shell/Templates.fs"
    "Shell/Shell.fs"
    "Shell/ChangeWatcher.fs"

]

let srcDir="src/"

let readLines (filePath:string) = seq {
    use sr = new StreamReader (filePath)
    while not sr.EndOfStream do
        yield sr.ReadLine ()
}

let mutable tab=""

for file  in files do
    // printfn "reading... %A" file
    printfn "\n\n\n/////////////////// %s\n" file
    for line in readLines (srcDir+file) do
        // if false then
        //     ()
        // elif 0=line.IndexOf("module") then
        //     tab<-""
        //     printfn "%s%s" tab line

        // elif 0=line.IndexOf("namespace") then
        //     tab<-""
        //     printfn "%s%s" tab line

        // elif -1<line.IndexOf("module") then
        //     tab<-"____"
        //     printfn "%s%s" tab line

        // elif -1<line.IndexOf("namespace") then
        //     tab<-"____"
        //     printfn "%s%s" tab line


        // else
        //     ()

        printfn "%s%s" tab line


    ()



()