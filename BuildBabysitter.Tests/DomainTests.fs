module BuildBabysitter.Tests

open App
open Model
open View
open Fabulous
open FsUnit
open Xunit
open System
open Fabulous.XamarinForms

[<Fact>]
let ``When PullRequestEntryRemoved it should remove the entry``() =
    let initialModel =
        { PullRequestInput = ""
          PullRequests =
              [ { URL = Uri("https://github.com/dotnet/sdk/pull/1")
                  Status = NeedAttention }
                { URL = Uri("https://github.com/dotnet/sdk/pull/2")
                  Status = NeedAttention } ] }

    let expectedModel =
        { PullRequestInput = ""
          PullRequests =
              [ { URL = Uri("https://github.com/dotnet/sdk/pull/1")
                  Status = NeedAttention } ] }

    match update (PullRequestEntryRemoved 1) initialModel with
    | (state, _) -> state |> should equal expectedModel

[<Fact>]
let ``When PullRequestEntryConfirmed it should add the entry``() =
    let initialModel =
        { PullRequestInput = "https://github.com/dotnet/sdk/pull/1"
          PullRequests = [] }

    let expectedModel =
        { PullRequestInput = ""
          PullRequests =
              [ { URL = Uri("https://github.com/dotnet/sdk/pull/1")
                  Status = Pending } ] }

    match update PullRequestEntryConfirmed initialModel with
    | (state, _) -> state |> should equal expectedModel

[<Fact>]
let ``Given invalid PullRequestInput When PullRequestEntryConfirmed it should not add the entry``() =
    let initialModel =
        { PullRequestInput = "1"
          PullRequests = [] }

    let expectedModel =
        { PullRequestInput = "1"
          PullRequests = [] }

    match update PullRequestEntryConfirmed initialModel with
    | (state, _) -> state |> should equal expectedModel
