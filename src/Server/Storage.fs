module Storage

open System
open System.Collections.Generic
open Microsoft.Extensions.Caching.Memory;
open Shared

type CacheManager() =
    let options =
        let o = new MemoryCacheOptions()
        o.TrackStatistics <- true
        o
    let cache = new MemoryCache(options)
    member this.Get(key: Guid) = cache.Get<DataResponse>(key) 
    member this.TryGet(key: Guid) : DataResponse option =
        let exists, item = cache.TryGetValue<DataResponse>(key)
        if exists then Some item else None
    member this.Set(key: Guid, value: DataResponse) = cache.Set(key, value, Environment.python_service_storage_timespan) |> ignore
    member this.Update(key: Guid, f: DataResponse -> DataResponse) =
        let value = cache.Get<DataResponse>(key)
        cache.Set(key, f value, Environment.python_service_storage_timespan) |> ignore
    member this.Remove(key: Guid) = cache.Remove(key)
    member this.KeyExists(key: Guid) = cache.TryGetValue(key) |> fst
    member this.Count = cache.Count

let Storage = CacheManager()

let generateNewGuid() =
    let rec generate () =
        let nextGuid = System.Guid.NewGuid()
        if Storage.KeyExists nextGuid then generate() else nextGuid
    generate()

