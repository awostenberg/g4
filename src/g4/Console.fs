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
        //todo maybe refactor console.fs towards a separate .exe, sharing common lib of what is in Model..
        //     I take that to be intent of "Within the same code base, build a standalone REST API with the following endpoints"
        
        let appendFile accum format = List.append accum (readFiles format |> Seq.toList)
 
        match args with
        | "--piped"::filename::rest -> run' (appendFile accum [Piped filename])  rest
        | "--comma"::filename::rest -> run' (appendFile accum [Comma filename])  rest
        | "--space"::filename::rest -> run' (appendFile accum [Space filename])  rest
    
        | "--orderBy"::"name"::rest -> accum |> orderByLastNameDescending |> format |> toConsole "by last name descending"
                                       run' accum rest
        | "--orderBy"::"birth"::rest -> accum |> orderByBirthDateAscending |> format |> toConsole "by birth date ascending"
                                        run' accum rest    
        | "--orderBy"::"gender"::rest -> accum |> orderByGenderThenLastName |> format |> toConsole "by gender (females first) then last name"
                                         run' accum rest
        | [] -> ()
        | "--help"::_ 
        | "help"::_ -> usage() // alias because /dotnet run/ consumes --help
        | oops::_ -> printfn "Error: unknown at %s" oops
                     usage()
                              
    run' [] args
     