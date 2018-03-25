
namespace Fuze





[<AutoOpen>]
module Main=
    open System
    open Fuze.Gulp
    
    
    // [<EntryPoint>]
    let main vargs = 
        TracerA.trace vargs
        zlog vargs
        Core.String.productName() |> zlog
        IO.Shell.pwd() |> zlog
        vargs |> zlog


        // let zgulp=Fuze.Gulp.Gulp()
        zgulp.task ("task0", ["*.fs"; "!*.fsx"],  fun cxt ->
            printfn "fun %A" cxt 

            cxt       
            + zgulp . src ["x"]
            + zgulp.dst ["yyy"]
            + zgulp.src ["x"]
            + zgulp.dst ["yyy"]

        )


        zgulp.task ("task0", ["*.fs"; "!*.fsx"])

        zgulp.task ("task0", ["*.fs"; "!*.fsx"], [], fun cxt ->
        
            cxt
        )
        


        //playing with operators
        // let f x = x
        // let x="x"
        // printfn "xxxxx %A %A %A"    1  (f(1))  3
        // printfn "test0 %A %A %A"   1   x.ToString     2 
        // printfn "test1 %A %A %A"   1   x . ToString   2  

        // let ftest2  a b c =
        //     printfn "test2 %A %A %A" a b c

        // ftest2  1   x . ToString  2  

        // ftest2  1   f(1)   2
        // let inline (.)  a 0 =
        //     a()
        // ftest2  1   x.0  2  

        // let x= "x"
        // let f x y = printfn "%A %A" x y

        // f x.ToString ()
        // f x . ToString (1)

        // let inline (~%) (a:obj) =  
        //     sprintf "yy%A" a
        //     // string a


        // let inline (.) (a:obj) (b:string) =
        //     a

        // let z
        // f %x ()

        // f %System ()
        // printfn "xxxx%A"  1+2 //x.ToString.%1


        0


// printfn ""
