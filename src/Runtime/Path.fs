    /// Contains basic functions for string manipulation.
namespace Fake.Runtime
module Path=
    open System.IO

    // Normalizes path for different OS
    let inline normalizePath (path : string) = 
        path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar)

    let getCurrentDirectory () =
        System.IO.Directory.GetCurrentDirectory()