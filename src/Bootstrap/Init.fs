namespace Fuze

#nowarn "20" "44"

open System.Diagnostics
open System.Runtime.CompilerServices
open System.Linq
type TracerA() =
    static member trace(  args,
                            [<CallerMemberName>] ?memberName: string,
                            [<CallerFilePath>]   ?path: string,
                            [<CallerLineNumber>] ?line: int            )  =
        match (memberName, path, line) with
        | Some m, Some p, Some l ->
            printfn "[:%13s:%5i] %s( %A )" (p.ToString().Split('\\').Last()) l m args
        | _,_,_ -> ()

type zlog(args :obj) =
    do
        printfn "zlog: %A" args



module Fuze =
    open System.Diagnostics
    open System.Runtime.CompilerServices


module Init =
    let init args =
        printfn "init( %A )" args
