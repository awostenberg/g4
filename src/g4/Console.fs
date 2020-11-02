module g4.Console

let run (args:string[]) =
    //todo parse command line args using argu http://fsprojects.github.io/Argu/
    //todo read in 3 files in --piped, --comma, and --space delimited. Use existing InputFormat parser common to web app"
    //todo order by --gender, --birth date, or --name. Use existing orderBy* common to web app"
    //todo Display dates in the format M/D/YYYY"
    //todo maybe refactor console.fs towards a separate .exe, sharing common lib of what is in Model..
    //     I take that to be intent of "Within the same code base, build a standalone REST API with the following endpoints"
    if args.Length > 0 then
        printfn "command line args %A" args
        printfn "console usage: --piped <inputfile> --comma <inputfile> --space <inputfile> --orderBy gender|birth|name"
    ()

