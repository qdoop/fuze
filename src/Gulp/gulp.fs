namespace Fuze.Gulp

open System
type GulpContext(x)=

    do 
        printfn "new Gulp Context"

    member this.data=x

    member this.pipe endp =
        printfn "context pipe"
        this


    // static member (|>) ((a:obj), (b:GulpContext)) =
    //     printfn "op_BarBar"
    //     match a with
    //     | :?GulpContext as cxt -> cxt.pipe(b)
    //     |  _                   -> (GulpContext(a)).pipe(b)
    // static member op_BooleanOr ((a:obj), (b:GulpContext)) =
    //     printfn "op_BarBar"
    //     match a with
    //     | :?GulpContext as cxt -> cxt.pipe(b)
    //     |  _                   -> (GulpContext(a)).pipe(b)
    static member (+) ((a:obj), (b:GulpContext)) =
        printfn "op_BarBar"
        match a with
        | :?GulpContext as cxt -> cxt.pipe(b)
        |  _                   -> (GulpContext(a)).pipe(b)
    // member this.src globs =
    //     printfn "context src"
    //     this
    // member this.dest globs =
    //     printfn "context dest"
    //     this

    // member this.task name deps fn =
    //     printfn "context task"
    //     this

type GulpTaskFn = GulpContext -> GulpContext




type zgulp =

    static member task(name, [<ParamArray>] args: System.Object[]  )=
        // let ox = defaultArg opts (fun _ -> GulpContext())
        // let ox = defaultArg opts (fun _ -> GulpContext())
        // if opts

        // match (opts, fn) with
        // | ( Some (fx:gulpfn), _ )     ->
        //                         printfn "gulp.task %s %A %A"  name deps  (fx (GulpContext())) 
        //                         GulpContext()
        // | ( Some (ox:obj), Some fx )  ->
        //                         printfn "gulp.task %s %A %A %A"  name deps ox (fx (GulpContext())) 
        //                         GulpContext()

        // |  ( _, _ )   -> GulpContext()

        for arg in args do
            printfn "task %A arg %A" name arg
            if(arg :? GulpTaskFn) then
                // arg (  GulpContext() )

                let fx=  arg :?> GulpTaskFn

                let z = fx (  GulpContext() )
                printfn "hitttttt %A" z


            if(arg :? obj -> obj) then
                // arg (GulpContext() )
                printfn "hitttttt"

        GulpContext()

        // let fx = defaultArg fn (fun _ -> GulpContext())

    static member src globs  =
        zgulp.source globs []
    static member dst globs =
        zgulp.target globs []
    static member source glps opts=
        printfn "gulp.source %A %A" glps opts
        GulpContext()
    static member target glps opts=
        printfn "gulp.target %A %A" glps opts
        GulpContext()




module Gulp=
    printfn "Gulp"