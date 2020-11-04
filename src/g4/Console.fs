module g4.Console
open Models
open System

type Command = ByName | ByBirth | ByGender
type Inputs = {files:InputFormat list;error:string option;order:Command list}

let toCommands args =
    //todo consider 3rd party parser http://fsprojects.github.io/Argu/
    let rec toCommands' accum args =
        match args with
        |"--piped"::file::rest -> toCommands' {accum with files=Piped file::accum.files} rest
        |"--comma"::file::rest -> toCommands' {accum with files=Comma file::accum.files} rest
        |"--space"::file::rest -> toCommands' {accum with files=Space file::accum.files} rest
        |"--orderBy"::"name"::rest -> toCommands' {accum with order=ByName::accum.order} rest
        |"--orderBy"::"birth"::rest -> toCommands' {accum with order=ByBirth::accum.order} rest
        |"--orderBy"::"gender"::rest -> toCommands' {accum with order=ByGender::accum.order} rest
        |"--orderBy"::oops::_ -> { accum with error=Some (sprintf "Error: unknown --orderBy %s" oops)}         
        | [] -> accum
        | oops::_ -> { accum with error=Some (sprintf "Error: unknown option %s" oops)}
    let result = toCommands' {files=[];error=None;order=[]} args
    {result with files=List.rev result.files;order=List.rev result.order}

let rec readFiles inputs =

    let readFile one =
        match one with
        | Piped filename -> readLines filename |> Seq.map (fun line -> toPerson (Piped line))
        | Comma filename -> readLines filename |> Seq.map (fun line -> toPerson (Comma line))
        | Space filename -> readLines filename |> Seq.map (fun line -> toPerson (Space line))
    seq {for item in inputs do
             yield! (readFile item)}
                                      
let format (people: Person seq) = people |> Seq.map format

let toConsole (title:string) (strings:string seq) =
    Console.WriteLine(title)
    strings |> Seq.iter (fun s -> System.Console.WriteLine(s) )

let run (args:string[]) =

    let usage() = printfn "console usage: --piped <inputfile> --comma <inputfile> --space <inputfile> --orderBy gender|birth|name"
    let commands = args |> Seq.toList |> toCommands
    match commands.error with
    | Some e -> printfn "%s" e
                usage()
    | None -> let inputs = readFiles commands.files |> Seq.toList
              let sortAndOutput item =
                  match item with
                  | ByName -> inputs |> orderByLastNameDescending |> format |> toConsole "by last name descending" 
                  | ByBirth -> inputs |> orderByBirthDateAscending |> format |> toConsole "by birth date ascending"
                  | ByGender -> inputs |> orderByGenderThenLastName |> format |> toConsole "by gender (females first) then last name"
              commands.order |> List.iter sortAndOutput

     