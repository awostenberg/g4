module Micro        //why micro tests? https://www.geepawhill.org/2018/04/14/tdd-the-lump-of-coding-fallacy/

open Models
open System
open Xunit

[<Fact>]
let ``john smith from pipe``() =
     let result =  Piped "smith|john|m|blue|12/25/1985" |> toPerson    
     Assert.Equal( {firstName="john"
                    lastName="smith"
                    gender=Male
                    favoriteColor="blue"
                    dateOfBirth=DateTime.Parse("25-Dec-1985")},result)
[<Fact>]
let ``jane doe from pipe``() =
    let result =  (Piped "doe|jane|f|green|4/23/1985") |> toPerson
    Assert.Equal( {firstName="jane"
                   lastName="doe"
                   gender=Female
                   favoriteColor="green"
                   dateOfBirth=DateTime.Parse("23-apr-1985")},result)
   
[<Fact>]
let ``trim spaces before and after pipe``() =
    let result =  Piped "smith | john | m| blue | 12/25/1985" |> toPerson
    Assert.Equal({firstName="john"
                  lastName="smith"
                  gender=Male
                  favoriteColor="blue"
                  dateOfBirth=DateTime.Parse("25-Dec-1985")},result)
[<Fact>]
let ``john smith from comma``() =
    let result =  Comma "smith,john,m,blue,12/25/1985" |> toPerson    
    Assert.Equal({firstName="john"
                  lastName="smith"
                  gender=Male
                  favoriteColor="blue"
                  dateOfBirth=DateTime.Parse("25-Dec-1985")},result)
[<Fact>]
let ``john smith from space separators``() =
    let result =  Space "smith john m blue 12/25/1985" |> toPerson    
    Assert.Equal({firstName="john"
                  lastName="smith"
                  gender=Male
                  favoriteColor="blue"
                  dateOfBirth=DateTime.Parse("25-Dec-1985")},result)
                       

let jsmith = {firstName="john"
              lastName="smith"
              gender=Male
              favoriteColor="blue"
              dateOfBirth=DateTime.Parse("25-Dec-1985")}

[<Fact>]
let ``compare people by gender, female first``() =
    Assert.Equal(Eq,compareGenderThenLastName jsmith jsmith)
    Assert.Equal(Gt,compareGenderThenLastName jsmith {jsmith with gender=Female})
    Assert.Equal(Lt,compareGenderThenLastName {jsmith with gender=Female} jsmith)


[<Fact>]
let ``compare people by gender, female first, then last name ascending``() =
    Assert.Equal(Eq,compareGenderThenLastName jsmith jsmith)
    Assert.Equal(Lt,compareGenderThenLastName jsmith {jsmith with lastName="zurich"})
    Assert.Equal(Gt,compareGenderThenLastName jsmith {jsmith with lastName="zurich";gender=Female})
    Assert.Equal(Gt,compareGenderThenLastName jsmith {jsmith with lastName="adams"})

[<Fact>]
let ``comparison to int``() =
    Assert.Equal(-1,comparisonToInt Lt)
    Assert.Equal(0,comparisonToInt Eq)
    Assert.Equal(1,comparisonToInt Gt)

[<Fact>]
let ``sort by gender then last name``() =
    let jane = {jsmith with firstName="jane";gender=Female}
    let result = orderByGenderThenLastName [jsmith;jane]
    Assert.Equal(jane,result.Head)

        
[<Fact>]
let ``sort by birth date, ascending``() =
    let younger = {jsmith with
                    firstName="younger"
                    dateOfBirth=DateTime.Parse("12-dec-1985")}
    let elder = {jsmith with
                  firstName="elder"
                  dateOfBirth=DateTime.Parse("1-dec-1985")}
    
    let result = orderByBirthDateAscending [younger;elder]
    
    Assert.Equal(elder,result.Head)
    
[<Fact>]
let ``sort by last name descending``() =
    let a = {jsmith with lastName="a"}
    let z = {jsmith with lastName="z"}
    
    let result = orderByLastNameDescending [a;z]
    
    Assert.Equal(z,result.Head)
    

[<Fact>]
let ``format m/d/yyy``() =
    
    let result = format jsmith
    Assert.Equal("smith\tjohn\tmale\tblue\t12/25/1985",result)    

//todo next few are not micro tests as they do I/O... put elsewhere

let tempFileWith lines fn =
    let tmp = System.IO.Path.GetTempFileName()
    System.IO.File.WriteAllLines(tmp,lines |> Seq.toArray)
    fn tmp
    System.IO.File.Delete tmp


open g4.Console
[<Fact>]
let ``IO file lines as seq``() =
            tempFileWith ["hello";"world"] (fun tmpFile -> 
                let result = readLines tmpFile
                Assert.StrictEqual(["hello";"world"],Seq.toList result)
            )
    
[<Fact>]
let ``IO read piped file``() =

    tempFileWith ["smith|john|m|blue|12/25/1985"]
        (fun tmp1File ->
            tempFileWith ["smythe,john,m,blue,12/25/1985"]
                (fun tmp2File ->
                    let result = readFiles [Piped tmp1File;Comma tmp2File]
                    Assert.StrictEqual([jsmith;{jsmith with lastName="smythe"}],Seq.toList result)))
            
[<Fact>]
let ``IO read comma file``() =
    tempFileWith ["smith,john,m,blue,12/25/1985"]
        (fun tmpFile ->
            let result = readFiles [(Comma tmpFile)]
            Assert.StrictEqual([jsmith],Seq.toList result))
          
[<Fact>]
let ``IO read space file``() =
     tempFileWith ["smith john m blue 12/25/1985"]
        (fun tmpFile ->
            let result = readFiles [(Space tmpFile)]
            Assert.StrictEqual([jsmith],Seq.toList result))      
          
[<Fact>]
let ``IO read multiple files``() =
     tempFileWith ["smith john m blue 12/25/1985"]
        (fun tmpFile ->
            let result = readFiles [(Space tmpFile)]
            Assert.StrictEqual([jsmith],Seq.toList result))      
             
                

[<Fact>]
let ``cli parse --piped `` () =

    let result = toCommands ["--piped";"foo.bar"]

    Assert.Equal({files=[Piped "foo.bar"];error=None;order=[]},result)

[<Fact>]
let ``cli parse --comma `` () =

    let result = toCommands ["--comma";"foo.comma"]

    Assert.Equal({files=[Comma "foo.comma"];error=None;order=[]},result)
[<Fact>]
let ``cli parse --space `` () =

    let result = toCommands ["--space";"foo.space"]

    Assert.Equal({files=[Space "foo.space"];error=None;order=[]},result)
[<Fact>]
let ``cli parse multiple file inputs`` () =

    let result = toCommands ["--space";"foo.space";"--piped";"foo.pipe";"--comma";"foo.comma"]

    Assert.Equal({files=[Space "foo.space";Piped "foo.pipe";Comma "foo.comma"];error=None;order=[]},result)

[<Fact>]
let ``cli parse error - unknown option``() =
    let result = toCommands ["--oops"]
    
    Assert.True(result.error.IsSome)

[<Fact>]
let ``cli parse error - unknown orderby``() =
    let result = toCommands ["--orderBy";"whatever"]
    
    Assert.True(result.error.IsSome)
    let msg = result.error.Value
    Assert.Contains ("whatever",msg)    


[<Fact>]
let ``cli parse --orderBy name ``() =
    let result = toCommands ["--orderBy";"name"]
    
    Assert.StrictEqual([ByName],result.order)

[<Fact>]
let ``cli parse --orderBy birth ``() =
    let result = toCommands ["--orderBy";"birth"]
    
    Assert.StrictEqual([ByBirth],result.order)

[<Fact>]
let ``cli parse --orderBy gender ``() =
    let result = toCommands ["--orderBy";"gender"]
    
    Assert.StrictEqual([ByGender],result.order)
[<Fact>]
let ``cli parse many --orderBy ``() =
    let result = toCommands ["--orderBy";"gender";"--orderBy";"birth";"--orderBy";"name"]
    
    Assert.StrictEqual([ByGender;ByBirth;ByName],result.order)

   
   