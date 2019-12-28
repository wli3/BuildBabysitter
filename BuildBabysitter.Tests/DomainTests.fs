module BuildBabysitter.Tests

open App
open Model
open View
open Fabulous
open FsUnit.Xunit
open Xunit
open System
open Update
open Fabulous.XamarinForms
open StatusCheck
open System.IO

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
                  Status = InProgress } ] }

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

[<Fact>]
let ``it can parser the GithubPullRequestDetail``() =
    let deserializeResult = getGithubPullRequestHeadSha (File.ReadAllText("GithubPullRequestDetailSample.json"))
    deserializeResult |> should equal { Sha = "c4ef8697b2755826fbd651f30d442be4df92b847" }

[<Fact>]
let ``it can construct GithubPullRequestDetail``() =
    let result =
        githubPullRequestDetail
            { Owner = "dotnet"
              Repo = "sdk"
              PullNumber = 4021 }
    result |> should equal (Uri("https://api.github.com/repos/dotnet/sdk/pulls/4021"))

[<Fact>]
let ``it can construct commitCheckRuns``() =
    let result =
        commitCheckRuns
            { Owner = "dotnet"
              Repo = "sdk"
              PullNumber = 4021 } { Sha = "c4ef8697b2755826fbd651f30d442be4df92b847" }
    result
    |> should equal
           (Uri("https://api.github.com/repos/dotnet/sdk/commits/c4ef8697b2755826fbd651f30d442be4df92b847/check-runs"))

[<Fact(Skip = "Integration test")>]
let ``it can get PullRequestStatus``() =
    pullRequestStatus
        { Owner = "dotnet"
          Repo = "toolset"
          PullNumber = 3919 }
    |> should equal NeedAttention

[<Fact>]
let ``it can map Check runs status - NeedAttention``() =
    Some
        { CheckRuns =
              [| { Status = "in_progress"
                   Conclusion = None }
                 { Status = "completed"
                   Conclusion = Some "failure" }
                 { Status = "completed"
                   Conclusion = Some "failure" }
                 { Status = "completed"
                   Conclusion = Some "success" } |] }
    |> getPullRequestStatus
    |> should equal NeedAttention

[<Fact>]
let ``it can map Check runs status - InProgress``() =
    Some
        { CheckRuns =
              [| { Status = "in_progress"
                   Conclusion = None }
                 { Status = "in_progress"
                   Conclusion = None } |] }
    |> getPullRequestStatus
    |> should equal InProgress

[<Fact>]
let ``it can map Check runs status - Completed``() =
    Some
        { CheckRuns =
              [| { Status = "completed"
                   Conclusion = Some "success" }
                 { Status = "completed"
                   Conclusion = Some "success" } |] }
    |> getPullRequestStatus
    |> should equal Completed

[<Fact>]
let ``it can map Check runs status - InternalError``() =
    None
    |> getPullRequestStatus
    |> should equal InternalError

[<Fact()>]
let ``it can updateStatuses``() =
    updateStatuses
        [ { Url = Uri("https://github.com/dotnet/toolset/pull/391")
            Status = NeedAttention } ]
    |> should equal
           [ { Url = (Uri "https://github.com/dotnet/toolset/pull/391")
               Status = Completed } ]
