namespace BuildBabysitter

open System.Diagnostics
open Fabulous
open System
open Xamarin.Forms

module Model =
    type PullRequestStatus =
        | Pending
        | Done
        | NeedAttention

    type PullRequestEntry =
        { Url : Uri
          Status : PullRequestStatus }

    type Model =
        { PullRequestInput : string
          PullRequests : List<PullRequestEntry> }

    type AlertInfo =
        { Title : string
          Message : string }

    type Msg =
        | PullRequestEntryRemoved of int
        | PullRequestEntryConfirmed
        | TextInputChanged of TextChangedEventArgs
        | UserAlerted of AlertInfo

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
                        Error
                            { Title = "Invalid pull request URL"
                              Message = "Not valid pull request URL. Cannot find pull number" }
                | _ ->
                    Error
                        { Title = "Invalid URL input"
                          Message = "Not not valid URL. Not correct segment count" }
            | _ ->
                Error
                    { Title = "Invalid pull request URL"
                      Message = uri.Authority + " is not github domain" }
        | (false, _) ->
            Error
                { Title = "Invalid URL input"
                  Message = userInput + " is not valid URL" }

    let update msg model =
        match msg with
        | PullRequestEntryRemoved index -> { model with PullRequests = removeAt index model.PullRequests }, Cmd.none
        | TextInputChanged e -> { model with PullRequestInput = e.NewTextValue }, Cmd.none
        | PullRequestEntryConfirmed ->
            match Uri.TryCreate(model.PullRequestInput, UriKind.Absolute) with
            | (true, uri) ->
                { model with
                      PullRequests =
                          ({ Url = uri
                             Status = Pending }
                           :: model.PullRequests)
                      PullRequestInput = "" }, Cmd.none
            | (false, _) ->
                model,
                Cmd.ofMsg
                    (UserAlerted
                        { Title = "Invalid URL input"
                          Message = model.PullRequestInput + " is not valid URL" })
        | UserAlerted alertInfo ->
            Application.Current.MainPage.DisplayAlert(alertInfo.Title, alertInfo.Message, "Ok")
            |> Async.AwaitTask
            |> ignore
            model, Cmd.none
