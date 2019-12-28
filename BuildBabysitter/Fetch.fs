module Fetch

open System
open System.Threading
open System.Net.Http

let fetch (target : Uri) =
    let httpClient = new HttpClient()
    let requestMessage = new HttpRequestMessage(HttpMethod.Get, target.AbsoluteUri)
    requestMessage.Headers.Add("User-Agent", "BuildBabysitter")
    requestMessage.Headers.Add("Accept", "application/vnd.github.antiope-preview+json")

    let response = httpClient.SendAsync(requestMessage).Result
    if response.IsSuccessStatusCode then
        let content = response.Content.ReadAsStringAsync().Result
        Some content
    else None
