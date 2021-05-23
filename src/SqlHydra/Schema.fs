﻿module SqlHydra.Schema

open System.Text.Json
open System.Diagnostics
open System.Text.Json.Serialization

type Column = {
    Name: string
    DataType: string
    ClrType: string
    IsNullable: bool
}

type TableType = 
    | Table = 0
    | View = 1

type Table = {
    Catalog: string
    Schema: string
    Name: string
    Type: TableType
    Columns: Column array
}

type Schema = {
    Tables: Table array
}

let jsonOptions = 
    let opt = JsonSerializerOptions()
    opt.Converters.Add(JsonStringEnumConverter())
    opt

let serialize (schemaPath: string) (schema: Schema) =
    let json = JsonSerializer.Serialize(schema, jsonOptions)
    System.IO.File.WriteAllText(schemaPath, json)

let deserialize (schemaPath: string) =
    let json = System.IO.File.ReadAllText(schemaPath)
    JsonSerializer.Deserialize<Schema>(json, jsonOptions)

/// Calls the console app that pulls schema and saves as a json file.
let callSchemaProvider(exePath: string, connStr: string, schemaPath: string) = 
    let msSqlProvider = ProcessStartInfo(exePath, sprintf "\"%s\" \"%s\"" connStr schemaPath)
    msSqlProvider.UseShellExecute <- false
    msSqlProvider.CreateNoWindow <- true
    msSqlProvider.WindowStyle <- ProcessWindowStyle.Hidden
    use exeProcess = Process.Start(msSqlProvider)
    exeProcess.WaitForExit()