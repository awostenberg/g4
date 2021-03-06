module g4.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive

open Microsoft.AspNetCore.Http


// ---------------------------------
// Models
// ---------------------------------


let mutable people:Models.Person list = []

type Message =
    {
        Text : string
    }

// ---------------------------------
// Views
// ---------------------------------

module Views =
    open GiraffeViewEngine

    let layout (content: XmlNode list) =
        html [] [
            head [] [
                title []  [ encodedText "g4" ]
                link [ _rel  "stylesheet"
                       _type "text/css"
                       _href "/main.css" ]
            ]
            body [] content
        ]

    let partial () =
        h1 [] [ encodedText "g4" ]

    let index (model : Message) =
        [
            partial()
            p [] [ encodedText model.Text ]
            p [] [ a [ _href "/records/name" ] [ str "records/name" ] ]
            p [] [ a [ _href "/records/birthdate" ] [ str "records/birthdate" ] ]
            p [] [ a [ _href "/records/gender" ] [ str "records/gender" ] ] 
        ] |> layout

// ---------------------------------
// Web app
// ---------------------------------

let indexHandler (name : string) =
    let greetings = sprintf "Hello %s, from Giraffe! " name
    let model     = {Text = greetings }
    let view      = Views.index model
    htmlView view

let byGenderThenLastName ctx =
    json (Models.orderByGenderThenLastName people) ctx

let byBirthDateAscending ctx =
    json (Models.orderByBirthDateAscending people) ctx
let byNameDescending ctx =
    json (Models.orderByLastNameDescending people) ctx

let addPerson  =
    fun (next:HttpFunc) (ctx:HttpContext) ->
        task {
            let! description = ctx.BindModelAsync<Models.InputFormat>() //todo better error msg
            let person = Models.toPerson description 
            people <- person::people    //todo thread safety
            return! (json person) next ctx
        }
let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> indexHandler "world"
                routef "/hello/%s" indexHandler
                route "/records/gender" >=> warbler (fun _ -> byGenderThenLastName)
                route "/records/birthdate" >=> warbler (fun _ -> byBirthDateAscending)
                route "/records/name" >=> warbler (fun _ -> byNameDescending)
            ]
        POST >=>
            choose [
                route "/records" >=> addPerson
               
            ]
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8080")
           .AllowAnyMethod()
           .AllowAnyHeader()
           |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    (match env.EnvironmentName with
    | "Development" -> app.UseDeveloperExceptionPage()
    | _ -> app.UseGiraffeErrorHandler(errorHandler))
        .UseHttpsRedirection()
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddFilter(fun l -> l.Equals LogLevel.Information)
           .AddConsole()
           .AddDebug() |> ignore


let webapp() =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    // this builder can take command line args for ports and such (I think), which argues for separate webapp.exe
    Host.CreateDefaultBuilder() 
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .UseContentRoot(contentRoot)
                    .UseWebRoot(webRoot)
                    .Configure(Action<IApplicationBuilder> configureApp)
                    .ConfigureServices(configureServices)
                    .ConfigureLogging(configureLogging)
                    |> ignore)
        .Build()
        .Run()
[<EntryPoint>]
let main args =
    if args.Length > 0 then g4.Console.run(args) else webapp()
    

    0