﻿module SqlServer.SelectAsyncTests

open SqlHydra.Query
open Expecto
open DB

#if NET5_0
open SqlServer.AdventureWorksNet5
#endif
#if NET6_0_OR_GREATER
open SqlServer.AdventureWorksNet6
#endif

let openContext() = 
    let compiler = SqlKata.Compilers.SqlServerCompiler()
    let conn = openConnection()
    new QueryContext(conn, compiler)

// Tables
let personTable =           table<Person.Person>                    |> inSchema (nameof Person)
let customerTable =         table<Sales.Customer>                   |> inSchema (nameof Sales)
let orderHeaderTable =      table<Sales.SalesOrderHeader>           |> inSchema (nameof Sales)
let orderDetailTable =      table<Sales.SalesOrderDetail>           |> inSchema (nameof Sales)
let productTable =          table<Production.Product>               |> inSchema (nameof Production)
let subCategoryTable =      table<Production.ProductSubcategory>    |> inSchema (nameof Production)
let categoryTable =         table<Production.ProductCategory>       |> inSchema (nameof Production)
let errorLogTable =         table<dbo.ErrorLog>


[<Tests>]
let selectTests = 
    categoryList "SqlServer" "selectAsync" [
        
        testAsync "selectAsync" {
            let! results = 
                selectAsync HydraReader.Read (Create openContext) {
                    for o in orderHeaderTable do
                    join d in orderDetailTable on (o.SalesOrderID = d.SalesOrderID)
                    take 10
                    mapArray $"{o.SalesOrderNumber} - {d.LineTotal} - {d.ModifiedDate.ToShortDateString()}"
                }
        
            gt0 results
        }
        
        testAsync "selectAsync - select" {
            let! results = 
                selectAsync HydraReader.Read (Create openContext) {
                    for p in personTable do
                    take 10
                    select p
                }
        
            gt0 results
        }

        testAsync "selectAsync - toArray" {
            let! results = 
                selectAsync HydraReader.Read (Create openContext) {
                    for p in personTable do
                    take 10
                    toArray
                }
        
            gt0 results
        }
        
        testAsync "selectAsync - mapList column" {
            let! results = 
                selectAsync HydraReader.Read (Create openContext) {
                    for p in personTable do
                    take 10
                    mapList p.FirstName
                }
                
            gt0 results
        }

        testAsync "selectAsync - select entity - mapSeq column" {
            let! results = 
                selectAsync HydraReader.Read (Create openContext) {
                    for p in personTable do
                    take 10
                    select p
                    mapSeq $"{p.FirstName} {p.LastName}"
                }
        
            gt0 results
        }
        
        testAsync "selectAsync - select columns into - mapList column" {
            let! results = 
                selectAsync HydraReader.Read (Create openContext) {
                    for p in personTable do
                    take 10
                    select (p.FirstName, p.LastName) into (fname, lname)
                    mapList $"{fname} {lname}"
                }
                
            gt0 results
        }
        
        testAsync "selectAsync - count" {
            let! results = 
                selectAsync HydraReader.Read (Create openContext) {
                    for p in personTable do
                    count
                }
        
            Expect.isTrue (results > 0) ""
        }
        
        testAsync "selectAsync - tryHead - Selected" {
            let! result = 
                selectAsync HydraReader.Read (Create openContext) {
                    for p in personTable do
                    take 1
                    tryHead
                }
                
            Expect.isSome result ""
        }
        
        testAsync "selectAsync - tryHead - Mapped" {
            let! result = 
                selectAsync HydraReader.Read (Create openContext) {
                    for p in personTable do
                    take 1
                    mapSeq $"{p.FirstName} {p.LastName}"
                    tryHead
                }
                
            Expect.isSome result ""
        }
        
    ]
    