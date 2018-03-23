


(*

  nodemon -e fsx,fs,fsproj   -w tools -w src       --exec   cmd /C tools\build.cmd

*)

open System
open System.IO

let srcDir="src/"




printfn "//// Build: %s" (DateTime.UtcNow.ToString("yyMMddTHHmmss.fffZ"))

let files =[
    "Core/String.fs"
    "shell/Path.fs"
]

let fsFiles = seq {
    use sr = new StreamReader (srcDir + "src.fsproj")
    while not sr.EndOfStream do
        let line = sr.ReadLine ()
        if -1 = line.IndexOf("<Compile Include=") then
            ()
        else
            yield line.Replace("<Compile Include=","")
                      .Replace("/>", "")
                      .Replace("\"", "")
                      .Trim()
}



let readLines (filePath:string) = seq {
    use sr = new StreamReader (filePath)
    while not sr.EndOfStream do
        yield sr.ReadLine ()
}

let mutable tab=""

for file  in fsFiles do
    // printfn "reading... %A" file
    printfn "\n\n\n//// //// //// //// %s\n" file
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

        printfn "%s%s" tab (line.Replace("Fake.","Fuze."))



    ()



()