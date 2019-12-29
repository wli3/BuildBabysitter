namespace BuildBabysitter

open System.Diagnostics
open Fabulous
open System
open System.IO
open Xamarin.Forms
open Model
open StatusCheck
open Chiron

type public IStorage =
    abstract member SaveText: string -> unit
    abstract member LoadText: option<string>

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
        if Device.RuntimePlatform = Device.UWP then
            let service = DependencyService.Get<IStorage>();
            service.SaveText text |> ignore
        else
            if not (Directory.Exists(folder)) then (Directory.CreateDirectory(folder)) |> ignore
            File.WriteAllText(storageFilePath, text) |> ignore

    let load : List<PullRequestEntry> =
        if Device.RuntimePlatform = Device.UWP then
            let service = DependencyService.Get<IStorage>();
            match service.LoadText with
            | Some text ->
                if String.IsNullOrWhiteSpace(text) then []
                else text |> Json.parse |> Json.deserialize
            | None -> []
        else
            if (not (Directory.Exists(folder))) || (not (File.Exists(storageFilePath))) then []
            else
                File.ReadAllText(storageFilePath)
                |> Json.parse
                |> Json.deserialize
                
