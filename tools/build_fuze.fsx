
open System.IO


//nodemon -e fsx   -w tools  --exec   cmd /C tools\build.cmd



printfn "Building..."

let files =[
    "shell/shell.fsx"
    "watch/watch.fsx"
]

let srcDir="src/"

let readLines (filePath:string) = seq {
    use sr = new StreamReader (filePath)
    while not sr.EndOfStream do
        yield sr.ReadLine ()
}


for file  in files do
    printfn "reading... %A" file

    for line in readLines (srcDir+file) do
        printfn "%s" line

    ()




()