module Storage

open System
open System.Collections.Generic
open Microsoft.Extensions.Caching.Memory;
open Shared

type CacheManager<'A>() =
    let options =
        let o = new MemoryCacheOptions()
        o.TrackStatistics <- true
        o
    let cache = new MemoryCache(options)
    member this.Get(key: Guid) = cache.Get<'A>(key) 
    member this.TryGet(key: Guid) : 'A option =
        let exists, item = cache.TryGetValue<'A>(key)
        if exists then Some item else None
    member this.Set(key: Guid, value: 'A) = cache.Set(key, value, Environment.python_service_storage_timespan) |> ignore
    member this.Update(key: Guid, f: 'A -> 'Ä) =
        let value = cache.Get<'A>(key)
        cache.Set(key, f value, Environment.python_service_storage_timespan) |> ignore
    member this.Remove(key: Guid) = cache.Remove(key)
    member this.KeyExists(key: Guid) = cache.TryGetValue(key) |> fst
    member this.Count = cache.Count

let Storage = CacheManager<DataResponse>()
let EmailStorage = CacheManager<string>()

let generateNewGuid() =
    let rec generate () =
        let nextGuid = System.Guid.NewGuid()
        if Storage.KeyExists nextGuid then generate() else nextGuid
    generate()

