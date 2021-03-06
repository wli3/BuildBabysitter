namespace BuildBabysitter

open System.Diagnostics
open Fabulous
open System
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms
open Model
open View
open Update

module App =
    let initModel =
        { PullRequestInput = ""
          PullRequests =
              Storage.load }

    let init() = initModel, Cmd.batch [timerCmd; (Cmd.ofMsg StatusesRefreshed)] 

    // Note, this declaration is needed if you enable LiveUpdate
    let program = Program.mkProgram init update view

type App() as app =
    inherit Application()

    let runner =
        App.program
        |> Program.withConsoleTrace
        |> XamarinFormsProgram.run app

#if DEBUG
    // Uncomment this line to enable live update in debug mode.
    // See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/tools.html#live-update for further  instructions.
    //
    do runner.EnableLiveUpdate()
#endif
