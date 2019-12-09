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
        { URL : Uri
          Status : PullRequestStatus }

    type Model =
        { PullRequestInput : string
          PullRequests : List<PullRequestEntry> }

    type Msg =
        | PullRequestEntryRemoved of int
        | PullRequestEntryConfirmed
        | TextInputChanged of TextChangedEventArgs
        | UserAlerted of string * string

    let removeAt index input =
        input
        |> List.mapi (fun i el -> (i <> index, el))
        |> List.filter fst
        |> List.map snd

    let update msg model =
        match msg with
        | PullRequestEntryRemoved index -> { model with PullRequests = removeAt index model.PullRequests }, Cmd.none
        | TextInputChanged e -> { model with PullRequestInput = e.NewTextValue }, Cmd.none
        | PullRequestEntryConfirmed ->
            match Uri.TryCreate(model.PullRequestInput, UriKind.Absolute) with
            | (true, uri) ->
                { model with
                      PullRequests =
                          ({ URL = uri
                             Status = Pending }
                           :: model.PullRequests)
                      PullRequestInput = "" }, Cmd.none
            | (false, _) ->
                model, Cmd.ofMsg (UserAlerted("Invalid URL input", model.PullRequestInput + " is not valid URL"))
        | UserAlerted(title, message) ->
            Application.Current.MainPage.DisplayAlert(title, message, "Ok")
            |> Async.AwaitTask
            |> ignore
            model, Cmd.none
