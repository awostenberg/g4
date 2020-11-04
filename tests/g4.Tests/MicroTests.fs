module Micro        //why micro tests? https://www.geepawhill.org/2018/04/14/tdd-the-lump-of-coding-fallacy/

open Models
open System
open Xunit

[<Fact>] let ``john smith from pipe``() =
        let result =  Piped "smith|john|m|blue|12/25/1985" |> toPerson    
        Assert.Equal({firstName="john"
                      lastName="smith"
                      gender=Male
                      favoriteColor="blue"
                      dateOfBirth=DateTime.Parse("25-Dec-1985")},result)
[<Fact>] let ``jane doe from pipe``() =
        let result =  (Piped "doe|jane|f|green|4/23/1985") |> toPerson
        Assert.Equal( {firstName="jane"
                       lastName="doe"
                       gender=Female
                       favoriteColor="green"
                       dateOfBirth=DateTime.Parse("23-apr-1985")},result)
       
[<Fact>] let ``trim spaces before and after pipe``() =
        let result =  Piped "smith | john | m| blue | 12/25/1985" |> toPerson
        Assert.Equal({firstName="john"
                      lastName="smith"
                      gender=Male
                      favoriteColor="blue"
                      dateOfBirth=DateTime.Parse("25-Dec-1985")},result)
[<Fact>] let ``john smith from comma``() =
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

[<Fact>] let ``comparison to int``() =
        Assert.Equal(-1,comparisonToInt Lt)
        Assert.Equal(0,comparisonToInt Eq)
        Assert.Equal(1,comparisonToInt Gt)

[<Fact>] let ``sort by gender then last name``() =
    let jane = {jsmith with firstName="jane";gender=Female}
    let result = orderByGenderThenLastName [jsmith;jane]
    Assert.Equal(jane,result.Head)

        
[<Fact>] let ``sort by birth date, ascending``() =
    let younger = {jsmith with
                   firstName="younger"
                   dateOfBirth=DateTime.Parse("12-dec-1985")}
    let elder = {jsmith with
                 firstName="elder"
                 dateOfBirth=DateTime.Parse("1-dec-1985")}
    
    let result = orderByBirthDateAscending [younger;elder]
    
    Assert.Equal(elder,result.Head)
    
[<Fact>] let ``sort by last name descending``() =
    let a = {jsmith with lastName="a"}
    let z = {jsmith with lastName="z"}
    
    let result = orderByLastNameDescending [a;z]
    
    Assert.Equal(z,result.Head)
    

[<Fact>] let ``format m/d/yyy``() =
    
    let result = format jsmith
    Assert.Equal("smith\tjohn\tmale\tblue\t12/25/1985",result)    

//todo not microtests as these do I/O... put elsewhere


let tempFileWith lines fn =
    let tmp = System.IO.Path.GetTempFileName()
    System.IO.File.WriteAllLines(tmp,lines |> Seq.toArray)
    fn tmp
    System.IO.File.Delete tmp


   
[<Fact>] let ``IO file lines as seq``() =
            tempFileWith ["hello";"world"] (fun tmpFile -> 
                let result = readLines tmpFile
                Assert.StrictEqual(["hello";"world"],Seq.toList result)
            )
    
[<Fact>] let ``IO read piped file``() =
            tempFileWith ["smith|john|m|blue|12/25/1985"]
                (fun tmpFile ->
                    let result = g4.Console.readFiles [(Piped tmpFile)]
                    Assert.StrictEqual([jsmith],Seq.toList result)
        )
                    
     