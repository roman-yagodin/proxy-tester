// For more information see https://aka.ms/fsharp-console-apps

open System.IO
open System.Net
open System.Net.Http

let getCredentials(userName: string, password: string) =
    if userName <> "default" then
        new NetworkCredential(userName, password)
    else
        CredentialCache.DefaultNetworkCredentials

let createHttpClientHandler(proxy: string, proxyUserName: string, proxyPassword: string) =
    let httpClientHandler = new HttpClientHandler()
    httpClientHandler.UseProxy <- proxy <> "noproxy"
    if httpClientHandler.UseProxy then
        httpClientHandler.Proxy <- new WebProxy(proxy)
        httpClientHandler.Proxy.Credentials <- getCredentials(proxyUserName, proxyPassword)
    httpClientHandler

let testConnection(proxy: string, proxyUserName: string, proxyPassword: string, host: string) =
    let httpClientHandler = createHttpClientHandler(proxy, proxyUserName, proxyPassword)
    use client = new HttpClient(httpClientHandler)
    (
        try
            let value = client.GetAsync(host) |> Async.AwaitTask |> Async.RunSynchronously
            value.StatusCode
        with
            | ex -> HttpStatusCode.InternalServerError
    )

let proxies = File.ReadAllLines "proxies.list"

let proxyUsers= File.ReadAllLines "proxy-users.list"

let hosts = File.ReadAllLines "hosts.list"

for proxy in proxies do
    for host in hosts do
        for proxyUser in proxyUsers do
            let proxyUserParts = proxyUser.Split(":")
            let proxyUserName = proxyUserParts[0]
            let proxyPassword = if proxyUserParts.Length > 1 then proxyUserParts[1] else ""
            let testResult = testConnection(proxy, proxyUserName, proxyPassword, host)
            if testResult = HttpStatusCode.OK then
                printfn "passed: %s via %s, %s:%s" host proxy proxyUserName proxyPassword 
            else
                printfn "not passed: %s via %s, %s:%s" host proxy proxyUserName proxyPassword 



