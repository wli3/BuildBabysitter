namespace BuildBabysitter

open System.Diagnostics
open Fabulous
open System
open Xamarin.Forms
open Model
open StatusCheck

module Update =
    let update msg model =
        match msg with
        | PullRequestEntryRemoved index -> { model with PullRequests = removeAt index model.PullRequests }, Cmd.none
        | TextInputChanged e -> { model with PullRequestInput = e.NewTextValue }, Cmd.none
        | PullRequestEntryConfirmed ->
            match parsePullRequestEntry model.PullRequestInput with
            | Ok _ ->
                { model with
                      PullRequests =
                          ({ Url = Uri(model.PullRequestInput)
                             Status = InProgress }
                           :: model.PullRequests)
                      PullRequestInput = "" }, Cmd.ofMsg StatusesRefreshed
            | Error alertInfo -> model, Cmd.ofMsg (UserAlerted alertInfo)
        | UserAlerted alertInfo ->
            Application.Current.MainPage.DisplayAlert(alertInfo.Title, alertInfo.Message, "Ok")
            |> Async.AwaitTask
            |> ignore
            model, Cmd.none
        | StatusesRefreshed ->
            let newState = updateStatuses model.PullRequests
            { model with PullRequests = newState }, Cmd.none
        |  TimedTick ->  model, Cmd.batch([timerCmd; Cmd.ofMsg StatusesRefreshed])
