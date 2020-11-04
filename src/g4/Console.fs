module g4.Console
open Models
open System

let readFiles commands =
    match commands with
    | [Piped filename] -> readLines filename |> Seq.map (fun line -> toPerson (Piped line))

                           
let format (people: Person seq) = people |> Seq.map format

let toConsole (title:string) (strings:string seq) =
    Console.WriteLine(title)
    strings |> Seq.iter (fun s -> System.Console.WriteLine(s) )

let run (args:string[]) =
    printfn "console usage: --piped <inputfile> --comma <inputfile> --space <inputfile> --orderBy gender|birth|name"
    let args = Seq.toList args
    let rec run' accum args =
        printfn "command line args %A" args
        //todo read in 3 files in [x] --piped, --comma, and --space delimited. Use existing InputFormat parser common to web app"
        //todo order by --gender, --birth date, or [x] --name. Use existing orderBy* common to web app"
        //x Display dates in the format M/D/YYYY"
        //todo maybe refactor console.fs towards a separate .exe, sharing common lib of what is in Model..
        //     I take that to be intent of "Within the same code base, build a standalone REST API with the following endpoints"
        
        match args with
        | "--piped"::filename::rest -> run' (readFiles [Piped filename] |> Seq.toList) rest
        | "--orderBy"::"name"::rest -> accum |> orderByLastNameDescending |> format |> toConsole "by last name descending"
                                       run' accum rest
        | _ -> ()         
        //| [|"--orderBy";_|] -> Console.WriteLine "order by"
        //| [|"--order";_|] -> people |> orderByLastNameDescending |> format |> toConsole "by last name descending"
        //| [|"--orderBy";"birth"|] -> people |> orderByBirthDateAscending |> format |> toConsole "by birth date ascending"
//        | _ -> printfn "command line args %A" args
//              
    run' [] args
     