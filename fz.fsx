printfn "Started..."

let timer = System.Diagnostics.Stopwatch()
timer.Start()

printfn "Running..."



#load @"fuze.fsscript"

printfn "done!!!! %A"  (Fuze.Main.main [|"-arg0"|])



printfn "Elapsed Time: %A" timer.Elapsed