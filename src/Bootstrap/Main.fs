
namespace Fuze





[<AutoOpen>]
module Main=

    open Fuze.Gulp
    
    // [<EntryPoint>]
    let main vargs = 
        TracerA.trace vargs
        zlog vargs
        Core.String.productName() |> zlog
        IO.Shell.pwd() |> zlog
        vargs |> zlog


        let zgulp=Fuze.Gulp.Gulp()
        zgulp.task "task0" ["*.fs", "!*.fsx"] ( fun cxt ->
            printfn "fun %A" cxt 

            zgulp.src(["x"]).pipe(
                zgulp.dest ["yyy"]
            )


            ()
            )
        
        0


// printfn ""
