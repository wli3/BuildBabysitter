namespace BuildBabysitter

open System.Diagnostics
open Fabulous
open System
open Xamarin.Forms
open Chiron

module Model =
    type PullRequestStatus =
        | InProgress
        | Completed
        | NeedAttention
        | InternalError

    type PullRequestEntry =
        { Url : Uri
          Status : PullRequestStatus }

        static member ToJson(x : PullRequestEntry) =
            json {
                do! Json.write "status" (x.Status.ToString())
                do! Json.write "url" x.Url.AbsoluteUri
            }

        static member FromJson(_ : PullRequestEntry) =
            json {
                let! status = Json.read "status"
                let! url = Json.read "url"
                let matchStatus s =
                    match s with
                    | "InProgress" -> InProgress
                    | "Completed" -> Completed
                    | "NeedAttention" -> NeedAttention
                    | "InternalError" -> InternalError
                    | _ -> InternalError
                return { Status = matchStatus status
                         Url = Uri url }
            }

    type Model =
        { PullRequestInput : string
          PullRequests : List<PullRequestEntry> }

    type AlertInfo =
        { Title : string
          Message : string }

    type Msg =
        | PullRequestEntryRemoved of int
        | PullRequestEntryConfirmed
        | StatusesRefreshed
        | TextInputChanged of TextChangedEventArgs
        | UserAlerted of AlertInfo
        | TimedTick
        | SaveToStorage

    let removeAt index input =
        input
        |> List.mapi (fun i el -> (i <> index, el))
        |> List.filter fst
        |> List.map snd

    type PullRequest =
        { Owner : string
          Repo : string
          PullNumber : int }

    let parsePullRequestEntry (userInput : String) : Result<PullRequest, AlertInfo> =
        match Uri.TryCreate(userInput, UriKind.Absolute) with
        | (true, uri) ->
            match uri.Authority with
            | "github.com" ->
                match uri.Segments.Length with
                | 5 ->
                    match Int32.TryParse(uri.Segments.[4]) with
                    | (true, pullNumber) ->
                        Ok
                            { Owner = (uri.Segments.[1]).TrimEnd('/')
                              Repo = (uri.Segments.[2]).TrimEnd('/')
                              PullNumber = pullNumber }
                    | _ ->
                        Result.Error
                            { Title = "Invalid pull request URL"
                              Message = "Not valid pull request URL. Cannot find pull number" }
                | _ ->
                    Result.Error
                        { Title = "Invalid URL input"
                          Message = "Not not valid URL. Not correct segment count" }
            | _ ->
                Result.Error
                    { Title = "Invalid pull request URL"
                      Message = uri.Authority + " is not github domain" }
        | (false, _) ->
            Result.Error
                { Title = "Invalid URL input"
                  Message = userInput + " is not valid URL" }

    let timerCmd =
        async {
            do! Async.Sleep(10 * 60 * 1000)
            return TimedTick
        }
        |> Cmd.ofAsyncMsg

    [<Struct>]
    type OptionalBuilder =

        member __.Bind(opt, binder) =
            match opt with
            | Some value -> binder value
            | None -> None

        member __.Return(value) = Some value

    let optional = OptionalBuilder()
