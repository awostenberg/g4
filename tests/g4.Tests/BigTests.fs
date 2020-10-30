module BigTests

open System
open System.IO
open System.Net
open System.Net.Http
open Xunit
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks.V2.ContextInsensitive

// ---------------------------------
// Helper functions (extend as you need)
// ---------------------------------

let createHost() =
    WebHostBuilder()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .Configure(Action<IApplicationBuilder> g4.App.configureApp)
        .ConfigureServices(Action<IServiceCollection> g4.App.configureServices)

let runTask task =
    task
    |> Async.AwaitTask
    |> Async.RunSynchronously

let httpGet (path : string) (client : HttpClient) =
    path
    |> client.GetAsync
    |> runTask
 
let httpPostJson (path : string) (client : HttpClient) (json: string) =
    let sc = new StringContent(json,System.Text.Encoding.UTF8, "application/json")   
    client.PostAsync(path,sc)
    |> runTask
    
    //mabye try http://fssnip.net/a7/title/Send-HTTP-POST-request

    
    

 


let isStatus (code : HttpStatusCode) (response : HttpResponseMessage) =
    Assert.Equal(code, response.StatusCode)
    response

let ensureSuccess (response : HttpResponseMessage) =
    if not response.IsSuccessStatusCode
    then response.Content.ReadAsStringAsync() |> runTask |> failwithf "%A"
    else response

let readText (response : HttpResponseMessage) =
    response.Content.ReadAsStringAsync()
    |> runTask

let shouldEqual expected actual =
    Assert.Equal(expected, actual)

let shouldContain (expected : string) (actual : string) =
    Assert.True(actual.Contains expected,sprintf "expected /%s/ \nin string /%s/" expected actual)
    
let shouldOrderBy (a:string,b:string) (actual: string) =
    Assert.True(actual.Contains a,(sprintf "expect %s" a))
    Assert.True(actual.Contains b,(sprintf "expect %s" b) )
    let a',b' = actual.IndexOf a, actual.IndexOf b
    Assert.True(a'<b',(sprintf "expect %s before %s" a b))
    
    

// ---------------------------------
// Tests
// ---------------------------------

[<Fact>]
let ``Route / returns "Hello world, from Giraffe!"`` () =
    use server = new TestServer(createHost())
    use client = server.CreateClient()

    client
    |> httpGet "/"
    |> ensureSuccess
    |> readText
    |> shouldContain "Hello world, from Giraffe!"

[<Fact>]
let ``Route /hello/fooBar returns "Hello fooBar, from Giraffe!"`` () =
    use server = new TestServer(createHost())
    use client = server.CreateClient()

    client
    |> httpGet "/hello/fooBar"
    |> ensureSuccess
    |> readText
    |> shouldContain "Hello fooBar, from Giraffe!"

[<Fact>]
let ``Route which doesn't exist returns 404 Page not found`` () =
    use server = new TestServer(createHost())
    use client = server.CreateClient()

    client
    |> httpGet "/route/which/does/not/exist"
    |> isStatus HttpStatusCode.NotFound
    |> readText
    |> shouldEqual "Not Found"
    
[<Fact>] 
let ``records by gender`` () =
    let people = ["smith|john|m|blue|12/25/1985";
                  "smythe|jane|f|green|12/25/1985" ]
                 |> List.map (Models.Piped >> Models.toPerson)
    g4.App.people <- people
    
    use server = new TestServer(createHost())
    use client = server.CreateClient()
    
    client
    |> httpGet "/records/gender"
    |> ensureSuccess
    |> readText
    |> shouldOrderBy ("smythe","smith")

[<Fact>] 
let ``add person -- post /record`` () =
    use server = new TestServer(createHost())
    use client = server.CreateClient()
    g4.App.people <- []        //todo thread safety
    let jsonPayload = """{"case":"Piped","fields":["piper|peter|m|blue|12/25/1984"]}"""
    
    httpPostJson "/records" client jsonPayload
    |> ensureSuccess
    |> readText
    |> shouldContain "peter"
    Assert.True(g4.App.people.Length=1,sprintf "Expected one person have %d" g4.App.people.Length)
    
