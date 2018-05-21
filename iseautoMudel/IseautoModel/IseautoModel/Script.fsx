// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

#r "../data/bin/FSharp.Data.dll"
open FSharp.Data

let waypoint=CsvFile.Load("waypoint_paths.csv",hasHeaders=true)
type MyCsvType = CsvProvider<Schema = "x (float), y (float), z (float), velocity (float)", HasHeaders=false>
printfn "%A" (waypoint.NumberOfColumns)
let test = CsvProvider<"../wp.csv",Schema = "x (float), y (float), z (float), yaw (float),velocity(float),change_flag(float)", HasHeaders=false>.Load("../wp.csv")
printfn "%A" (test.NumberOfColumns)
let a=CsvProvider<"../wp.csv", Schema="x (float), y (float), z (float), velocity (float)">.Load("../map2_clicked_waypoints.csv")

let waypoint3=MyCsvType.Load("../map2_clicked_waypoints.csv")

let waypoint2=new CsvProvider<"map2_clicked_waypoints.csv">()

printfn "%A" ((test.Rows|>Seq.item 1).X)

printfn "%A" (a.NumberOfColumns)

printfn "%A" test.Headers
for row in test.Rows do
   printfn "%A " row

for row in test.Rows do
  let asFloatArray = Array.map (fun x -> row)
  printfn "%A" asFloatArray // TODO: Do something useful here :-)

#if INTERACTIVE
#r @"..\..\..\bin\NModel.dll"
#r @"..\..\..\bin\NModel.Visualization.dll"
#r @"bin\Debug\IseautoModel.dll"
#r "../data/bin/FSharp.Data.dll"
#endif
open FSharp.Data


NModel.Visualization.Interactive.Run(IseautoModel.Contract.Create())