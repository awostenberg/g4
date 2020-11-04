module g4.Console
open Models
open System

let readFiles commands =
    match commands with
    | [Piped filename] -> readLines filename |> Seq.map (fun line -> toPerson (Piped line))
    | [Comma filename] -> readLines filename |> Seq.map (fun line -> toPerson (Comma line))
    | [Space filename] -> readLines filename |> Seq.map (fun line -> toPerson (Space line))
                                      
let format (people: Person seq) = people |> Seq.map format

let toConsole (title:string) (strings:string seq) =
    Console.WriteLine(title)
    strings |> Seq.iter (fun s -> System.Console.WriteLine(s) )

let run (args:string[]) =
    let usage() = printfn "console usage: --piped <inputfile> --comma <inputfile> --space <inputfile> --orderBy gender|birth|name"
    let args = Seq.toList args
    let rec run' accum args =
        //printfn "command line args %A" args
        //todo read in 3 files in [x] --piped, --comma, and --space delimited. Use existing InputFormat parser common to web app"
        //todo order by --gender, --birth date, or [x] --name. Use existing orderBy* common to web app"
        //x Display dates in the format M/D/YYYY"
        //todo maybe refactor console.fs towards a separate .exe, sharing common lib of what is in Model..
        //     I take that to be intent of "Within the same code base, build a standalone REST API with the following endpoints"
        
        match args with
        | "--piped"::filename::rest -> let people = List.append accum (readFiles [Piped filename] |> Seq.toList)
                                       run' people rest
        | "--comma"::filename::rest -> let people = List.append accum (readFiles [Comma filename] |> Seq.toList)
                                       run' people rest
        | "--space"::filename::rest -> let people = List.append accum (readFiles [Space filename] |> Seq.toList)
                                       run' people rest
    
        | "--orderBy"::"name"::rest -> accum |> orderByLastNameDescending |> format |> toConsole "by last name descending"
                                       run' accum rest
        | "--orderBy"::"birth"::rest -> accum |> orderByBirthDateAscending |> format |> toConsole "by birth date ascending"
                                        run' accum rest    
        | "--orderBy"::"gender"::rest -> accum |> orderByGenderThenLastName |> format |> toConsole "by gender (females first) then last name"
                                         run' accum rest
        | [] -> ()                     
        | "help"::_ -> usage() // can't easily do --help because dotnet run consumes that
        | oops::_ -> printfn "Error: unknown at %s" oops
                     usage()
                              
    run' [] args
     