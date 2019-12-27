module Fetch

open System
open Hopac
open HttpFs.Client
open System.Threading

let fetch (target : Uri) =
    let request = Request.createUrl Get target.AbsoluteUri
                  |> Request.setHeader (UserAgent "BuildBabysitter")
                  |> Request.setHeader (Accept "application/vnd.github.antiope-preview+json")
    job {
        use! response = getResponse request
        let! bodyStr = Response.readBodyAsString response
        match response.statusCode with
        | 200 -> return Some(bodyStr)
        | _ ->
            sprintf "Failed to fetch %s %s" target.AbsoluteUri bodyStr |> ignore
            return None
    }
    |> run
