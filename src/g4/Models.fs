module Models
open System

type Gender = Female|Male
type Person = {
    lastName: string
    firstName: string
    gender: Gender
    favoriteColor: string
    dateOfBirth: DateTime
}

type InputType = Piped of String

let toPerson s =
    match s with
    | Piped s -> let tokens = s.Split('|')
                 {lastName=tokens.[0]
                  firstName=tokens.[1]
                  gender=if tokens.[2]="m" then Male else Female
                  favoriteColor=tokens.[3]
                  dateOfBirth=DateTime.Parse(tokens.[4])}
                 




