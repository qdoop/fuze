printfn "Started..."
open System.Threading

let timer = System.Diagnostics.Stopwatch()
timer.Start()

printfn "Running..."

if false then
    printfn "ddd"


#load @"fuze.fsscript"
open Fuze



zlog <| testx ()
ztrc <| testx "vvvv"
TracerA.trace <| testx "vvvv"

// open System


printfn "done!!!! %A"  (main [|System.Environment.CommandLine, System.Environment.GetCommandLineArgs() |])

