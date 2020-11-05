module g4.Console
open Models
open System

type Command = ByName | ByBirth | ByGender | Input of InputFormat | Error of string
                member this.isInput  = match this with (Input _) -> true | _ -> false

let toCommands args =

    //todo consider 3rd party parser http://fsprojects.github.io/Argu/
    let rec toCommands' accum args =
        match args with
        | [] -> accum
        |"--piped"::file::rest -> toCommands' (Input (Piped file)::accum) rest
        |"--comma"::file::rest -> toCommands' (Input (Comma file)::accum) rest
        |"--space"::file::rest -> toCommands' (Input (Space file)::accum) rest
        |"--orderBy"::"name"::rest -> toCommands' (ByName::accum) rest
        |"--orderBy"::"birth"::rest -> toCommands' (ByBirth::accum) rest
        |"--orderBy"::"gender"::rest -> toCommands' (ByGender::accum) rest
        |"--orderBy"::oops::_ -> [Error (sprintf "Error: unknown --orderBy %s" oops)]         
        | oops::_ -> [Error (sprintf "Error: unknown option %s" oops)]
    toCommands' List.empty args |> List.rev       

let rec readFiles inputs =

    let readFile one =
        match one with
        | Piped filename -> readLines filename |> Seq.map (fun line -> toPerson (Piped line))
        | Comma filename -> readLines filename |> Seq.map (fun line -> toPerson (Comma line))
        | Space filename -> readLines filename |> Seq.map (fun line -> toPerson (Space line))
    seq {for item in inputs do
             yield! (readFile item)}
                                      
let format (people: Person seq) = people |> Seq.map format

let toConsole' out  (title:string) (strings:string seq) =
    out title
    strings |> Seq.iter out
let toConsole title strings =  toConsole' (fun s -> printfn "%s" s) title strings

let getInputs commands = commands
                         |> Seq.map (fun item -> match item with (Input x) -> Some x | _ -> None)
                         |> Seq.filter (fun item -> item.IsSome)
                         |> Seq.map (fun item -> item.Value)

let run' (args:string[]) report =

    let usage() = printfn "console usage: --piped <inputfile> --comma <inputfile> --space <inputfile> --orderBy gender|birth|name"
    let commands = args |> Seq.toList |> toCommands
    let error = commands |> Seq.tryPick (fun x -> match x with Error msg -> Some msg | _ -> None)  
    match error with
    | Some msg -> printfn "%s" msg
                  usage()
    | None ->
        let inputs = commands |> getInputs |> readFiles |> Seq.toList
        let sortAndOutput item =
          match item with
          | ByName -> inputs |> orderByLastNameDescending |> format |> report "by last name descending" 
          | ByBirth -> inputs |> orderByBirthDateAscending |> format |> report "by birth date ascending"
          | ByGender -> inputs |> orderByGenderThenLastName |> format |> report "by gender (females first) then last name"
          | _ -> ()
        List.iter sortAndOutput commands        

let run (args:string[]) = run' args toConsole
