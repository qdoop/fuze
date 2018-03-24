namespace Fuze.Gulp

type GulpContext()=
    let data="context"
    do 
        printfn "Gulp Context"

    member this.pipe endp =
        printfn "context pipe"
        this

    member this.src globs =
        printfn "context src"
        this

    member this.dest globs =
        printfn "context dest"
        this

    member this.task name deps fn =
        printfn "context task"
        this


type Gulp() =
    let context=GulpContext()
    do
        printfn "%A" context

    member this.src globs  =
        printfn "gulp.src %A" globs 
        context.src globs
    member this.dest globs =
        printfn "gulp.dest %A" globs 
        context.dest globs
    member this.task name deps fn =
        printfn "gulp.task %s %A %A"  name deps fn
        context.task name deps fn

module Gulp=
    printfn "Gulp"