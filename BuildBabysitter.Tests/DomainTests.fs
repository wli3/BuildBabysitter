module BuildBabysitter.Tests

open App
open Model
open View
open Fabulous
open FsUnit.Xunit
open Xunit
open System
open Fabulous.XamarinForms

[<Fact>]
let ``When PullRequestEntryRemoved it should remove the entry``() =
    let initialModel =
        { PullRequestInput = ""
          PullRequests =
              [ { Url = Uri("https://github.com/dotnet/sdk/pull/1")
                  Status = NeedAttention }
                { Url = Uri("https://github.com/dotnet/sdk/pull/2")
                  Status = NeedAttention } ] }

    let expectedModel =
        { PullRequestInput = ""
          PullRequests =
              [ { Url = Uri("https://github.com/dotnet/sdk/pull/1")
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
              [ { Url = Uri("https://github.com/dotnet/sdk/pull/1")
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

[<Fact>]
let ``Given url it should get PullRequest``() =
    match parsePullRequestEntry "https://github.com/dotnet/sdk/pull/1" with
    | Error message -> Assert.True(false, "fail" + message.Message)
    | Ok pullRequest ->
        pullRequest
        |> should equal
               { Owner = "dotnet"
                 Repo = "sdk"
                 PullNumber = 1 }

[<Fact>]
let ``Given invalid url it should get error``() =
    match parsePullRequestEntry "1" with
    | Error message ->
        message
        |> should equal
               { Title = "Invalid URL input"
                 Message = "1 is not valid URL" }
    | _ -> Assert.True(false, "fail")

[<Fact>]
let ``Given not github url it should get error``() =
    match parsePullRequestEntry "https://wrong.com/dotnet/sdk/pull/1" with
    | Error message ->
        message
        |> should equal
               { Title = "Invalid pull request URL"
                 Message = "wrong.com is not github domain" }
    | _ -> Assert.True(false, "fail")

[<Fact>]
let ``Given not pull request url it should get error``() =
    match parsePullRequestEntry "https://github.com/pull/1" with
    | Error message ->
        message
        |> should equal
               { Title = "Invalid URL input"
                 Message = "Not not valid URL. Not correct segment count" }
    | _ -> Assert.True(false, "fail")
