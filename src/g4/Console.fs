module g4.Console
open Models
open System

let readFiles commands =
    match commands with
    | [Piped filename] -> readLines filename |> Seq.map (fun line -> toPerson (Piped line))

                           
let sortBy _ people = people |> Seq.toList |> orderByLastNameDescending 
let format (people: Person seq) = people |> Seq.map format

let toConsole (title:string) (strings:string seq) =
    Console.WriteLine(title)
    strings |> Seq.iter (fun s -> System.Console.WriteLine(s) )
let run (args:string[]) =
    //todo read in 3 files in [x] --piped, --comma, and --space delimited. Use existing InputFormat parser common to web app"
    //todo order by --gender, --birth date, or [x] --name. Use existing orderBy* common to web app"
    //x Display dates in the format M/D/YYYY"
    //todo maybe refactor console.fs towards a separate .exe, sharing common lib of what is in Model..
    //     I take that to be intent of "Within the same code base, build a standalone REST API with the following endpoints"
    
    match args with
    | [|"--piped";filename|] -> readFiles [Piped filename] |> sortBy args |> format |> toConsole "by last name descending"
    | _ -> printfn "command line args %A" args
           printfn "console usage: --piped <inputfile> --comma <inputfile> --space <inputfile> --orderBy gender|birth|name"
    ()