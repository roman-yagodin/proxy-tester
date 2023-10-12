// For more information see https://aka.ms/fsharp-console-apps

open System
open System.IO
open System.Net
open System.Net.Http
open System.Globalization
open CsvHelper
open CsvHelper.Configuration
open CsvHelper.Configuration.Attributes

type ProxyInfo () =
    let mutable proxyUri = ""
    let mutable proxyUserName = ""
    let mutable proxyPassword = ""

    [<Index(0)>]
    member this.ProxyUri
        with get() = proxyUri
        and set(value) = proxyUri <- value
    
    [<Index(1)>]
    member this.ProxyUserName
        with get() = proxyUserName
        and set(value) = proxyUserName <- value

    [<Index(2)>]
    member this.ProxyPassword
        with get() = proxyPassword
        and set(value) = proxyPassword <- value

let getCredentials(userName: string, password: string) =
    if not (String.IsNullOrEmpty(userName)) && not (String.IsNullOrEmpty(password)) then
        new NetworkCredential(userName, password)
    else
        CredentialCache.DefaultNetworkCredentials

let createHttpClientHandler(proxy: ProxyInfo) =
    let httpClientHandler = new HttpClientHandler()
    httpClientHandler.UseProxy <- proxy.ProxyUri <> "noproxy"
    if httpClientHandler.UseProxy then
        httpClientHandler.Proxy <- new WebProxy(proxy.ProxyUri)
        httpClientHandler.Proxy.Credentials <- getCredentials(proxy.ProxyUserName, proxy.ProxyPassword)
    httpClientHandler

let testConnection(proxy: ProxyInfo, host: string) =
    let httpClientHandler = createHttpClientHandler(proxy)
    use client = new HttpClient(httpClientHandler)
    (
        try
            let value = client.GetAsync(host) |> Async.AwaitTask |> Async.RunSynchronously
            value.StatusCode.ToString()
        with
        | ex -> ex.Message
    )

let loadProxies(fileName) =
    let config = new CsvConfiguration(CultureInfo.InvariantCulture)
    config.HasHeaderRecord <- false
    config.Delimiter <- ";"
    config.MissingFieldFound <- null
    use reader = new StreamReader(fileName: string)
    use csv = new CsvReader(reader, config)
    (
        csv.GetRecords<ProxyInfo>() |> Seq.toList
    )

let formatUserName (userName, password) =
    if (userName |> String.IsNullOrEmpty) then
        "current user"
    else
        $"{userName}:{password}"

let hosts = File.ReadAllLines "hosts.list"

let proxies = loadProxies("proxies.csv")

for proxy in proxies do
    for host in hosts do
        let testResult = testConnection(proxy, host)
        printf "%s via %s, %s - " host proxy.ProxyUri (formatUserName(proxy.ProxyUserName, proxy.ProxyPassword))
        printfn "%s" testResult

printfn "Done."
