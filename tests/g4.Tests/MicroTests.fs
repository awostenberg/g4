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
       
let jsmith = {firstName="john"
              lastName="smith"
              gender=Male
              favoriteColor="blue"
              dateOfBirth=DateTime.Parse("25-Dec-1985")}

[<Fact>]
let ``by gender female first``() =
    Assert.Equal(Eq,compareGenderThenLastName jsmith jsmith)
    Assert.Equal(Gt,compareGenderThenLastName jsmith {jsmith with gender=Female})
    Assert.Equal(Lt,compareGenderThenLastName {jsmith with gender=Female} jsmith)


[<Fact>]
let ``by gender female first then last name asc``() =
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

        
     