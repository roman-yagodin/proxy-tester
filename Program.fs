// For more information see https://aka.ms/fsharp-console-apps

open System
open System.IO
open System.Net
open System.Net.Http

let getCredentials(userName: string, password: string) =
    if not (userName |> String.IsNullOrEmpty) then
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
            value.StatusCode.ToString()
        with
        | ex -> ex.Message
    )

let proxies = File.ReadAllLines "proxies.list"

let hosts = File.ReadAllLines "hosts.list"

let parseProxy (proxy) =
    if proxy = "noproxy" then
        ("noproxy", String.Empty, String.Empty)
    else
        let proxyUri = new Uri(proxy)
        let proxyUserParts = proxyUri.UserInfo.Split(":")
        let proxyUserName = proxyUserParts[0]
        let proxyPassword = if proxyUserParts.Length > 1 then proxyUserParts[1] else ""
        let proxy2 =
            if proxyUri.Port > 0 then
                String.Format("{0}://{1}:{2}", proxyUri.Scheme, proxyUri.Host, proxyUri.Port)
            else
                String.Format("{0}://{1}", proxyUri.Scheme, proxyUri.Host)
        (proxy2, proxyUserName, proxyPassword)

let formatUserName (userName, password) =
    if (userName |> String.IsNullOrEmpty) then
        "current user"
    else
        $"{userName}:{password}"

for proxy in proxies do
    for host in hosts do
        if not (proxy.StartsWith("#")) && not (host.StartsWith("#")) then
            let (proxy2, proxyUserName, proxyPassword) = parseProxy(proxy)
            let testResult = testConnection( proxy2, proxyUserName, proxyPassword, host)
            printf "%s via %s, %s - " host proxy2 (formatUserName(proxyUserName, proxyPassword))
            printfn "%s" testResult

printfn "Done."
