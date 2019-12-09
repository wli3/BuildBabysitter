namespace BuildBabysitter

open System.Diagnostics
open System
open Model
open Fabulous.XamarinForms
open Xamarin.Forms

module View =
    let listViewItems (pullRequests : List<PullRequestEntry>) dispatch =
        pullRequests
        |> List.mapi
            (fun index pullRequestEntry ->
            View.ViewCell
                (view = View.StackLayout
                            (children = [ View.Label
                                              (text = pullRequestEntry.URL.AbsoluteUri,
                                               horizontalOptions = LayoutOptions.Fill)
                                          View.Button
                                              (text = "Remove",
                                               command = (fun () -> dispatch (PullRequestEntryRemoved index)),
                                               horizontalOptions = LayoutOptions.End) ])))

    let view (model : Model) dispatch =
        View.ContentPage
            (content = View.StackLayout
                           (children = [ View.Entry
                                             (text = model.PullRequestInput, placeholder = "Pull request to watch",
                                              textChanged = (fun e -> dispatch (TextInputChanged e)))
                                         View.ListView(items = listViewItems model.PullRequests dispatch)
                                         View.Button
                                             (text = "Add", command = (fun () -> dispatch PullRequestEntryConfirmed),
                                              horizontalOptions = LayoutOptions.Center) ]))
