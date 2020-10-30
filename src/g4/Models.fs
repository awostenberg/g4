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
                 

type Comparison = Lt|Eq|Gt
let  compare a b = if a = b then Eq else (if a < b then Lt else Gt)
let compareGenderThenLastName a b =
    match compare a.gender b.gender with
    | Eq -> compare a.lastName b.lastName
    | other -> other
let comparisonToInt comparison =
    match comparison with
    | Lt -> -1
    | Eq -> 0
    | Gt -> 1
let compareGenderThenLastName' p1 p2 = compareGenderThenLastName p1 p2 |> comparisonToInt

let orderByGenderThenLastName people = people |> List.sortWith compareGenderThenLastName'

let orderByBirthDateAscending people = people |> List.sortBy (fun p -> p.dateOfBirth)