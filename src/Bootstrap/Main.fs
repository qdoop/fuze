
namespace Fuze
module Main=
    // [<EntryPoint>]
    let main (vargs :string[]) = 
        TracerA.trace vargs
        zlog vargs
        Core.String.productName() |> zlog
        IO.Shell.pwd() |> zlog
        vargs |> zlog

        0


// printfn ""
