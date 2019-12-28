namespace BuildBabysitter

open System.Diagnostics
open Fabulous
open System
open System.IO
open Xamarin.Forms
open Model
open StatusCheck
open Chiron

module Storage =
    let private storageFileName = "saved-pull-request-status.json"
    let private folder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "build-babysitter")
    let private storageFilePath = Path.Combine(folder, storageFileName)

    let save (pullRequestEntry : List<PullRequestEntry>) =
        let text =
            pullRequestEntry
            |> Json.serialize
            |> Json.format
        if not (Directory.Exists(folder)) then (Directory.CreateDirectory(folder)) |> ignore
        File.WriteAllText(storageFilePath, text) |> ignore

    let load : List<PullRequestEntry> =
        if (not (Directory.Exists(folder))) || (not (File.Exists(storageFilePath))) then []
        else
            File.ReadAllText(storageFilePath)
            |> Json.parse
            |> Json.deserialize
