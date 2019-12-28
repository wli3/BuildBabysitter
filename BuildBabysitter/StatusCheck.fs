namespace BuildBabysitter

open System.Diagnostics
open Fabulous
open System
open Xamarin.Forms
open Model
open Chiron
open Fetch

module StatusCheck =
    type PullRequestHead =
        { Sha : string }
        static member FromJson(_ : PullRequestHead) = json { let! sha = Json.read "sha"
                                                             return { Sha = sha } }

    type GithubPullRequestDetail =
        { Head : PullRequestHead }
        static member FromJson(_ : GithubPullRequestDetail) = json { let! head = Json.read "head"
                                                                     return { Head = head } }

    type PullRequestChecksStatus =
        { Status : string
          Conclusion : Option<string> }
        static member FromJson(_ : PullRequestChecksStatus) =
            json {
                let! status = Json.read "status"
                let! conclusion = Json.read "conclusion"
                return { Status = status
                         Conclusion = conclusion }
            }

    type PullRequestCheckRuns =
        { CheckRuns : PullRequestChecksStatus [] }
        static member FromJson(_ : PullRequestCheckRuns) = json { let! status = Json.read "check_runs"
                                                                  return { CheckRuns = status } }

    let getGithubPullRequestHeadSha (json : string) : PullRequestHead =
        (json
         |> Json.parse
         |> Json.deserialize).Head

    let getPullRequestApiCheckRuns (json : string) : PullRequestCheckRuns =
        json
        |> Json.parse
        |> Json.deserialize

    let githubPullRequestDetail (pullRequest : PullRequest) : Uri =
        Uri
            (sprintf "https://api.github.com/repos/%s/%s/pulls/%i" pullRequest.Owner pullRequest.Repo
                 pullRequest.PullNumber)

    let commitCheckRuns (pullRequest : PullRequest) (pullRequestHead : PullRequestHead) : Uri =
        Uri
            (sprintf "https://api.github.com/repos/%s/%s/commits/%s/check-runs" pullRequest.Owner pullRequest.Repo
                 pullRequestHead.Sha)

    let getPullRequestStatus (optionPullRequestCheckRuns : Option<PullRequestCheckRuns>) : PullRequestStatus =
        let checksStatuses (statuses : PullRequestChecksStatus []) =
            if statuses |> Array.exists (fun s -> s.Conclusion = Some "failure") then NeedAttention
            elif statuses
                 |> Array.forall (fun s ->
                     s = { Status = "completed"
                           Conclusion = Some "success" })
            then Completed
            else InProgress

        match optionPullRequestCheckRuns with
        | Some pullRequestCheckRuns ->
            match pullRequestCheckRuns.CheckRuns with
            | [||] -> InternalError
            | _ -> pullRequestCheckRuns.CheckRuns |> checksStatuses
        | None -> InternalError

    let pullRequestStatus (pullRequest : PullRequest) : PullRequestStatus =
        let pullRequestApiCheckRuns =
            optional {
                let! githubPullRequestDetailContent = pullRequest
                                                      |> githubPullRequestDetail
                                                      |> fetch
                let headSha = githubPullRequestDetailContent |> getGithubPullRequestHeadSha
                let! commitShaStatusContent = headSha
                                              |> commitCheckRuns pullRequest
                                              |> fetch
                return commitShaStatusContent |> getPullRequestApiCheckRuns
            }
        getPullRequestStatus pullRequestApiCheckRuns

    let updateStatuses (pullRequests : List<PullRequestEntry>) : List<PullRequestEntry> =
        pullRequests
        |> List.map (fun p ->
            match parsePullRequestEntry p.Url.AbsoluteUri with
            | Ok entry -> { p with Status = entry |> pullRequestStatus }
            | _ -> { p with Status = InternalError })
