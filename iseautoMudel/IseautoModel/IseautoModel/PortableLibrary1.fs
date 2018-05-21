namespace IseautoModel

open NModel.Terms
open NModel.Attributes
open NModel.Execution
open FSharp.Data
open FSharp.Core
open System
open System.Collections.Generic
open FSharp.Data.HttpMethod
open System.IO

type PositionState=Start=0|Finish=1|ReacheWp=2
type Transitions=Move=0|Stop=1|Wait=2|ObserveMsg=3

type iseautoTest()=
 static let mutable pass : double=1.0
 static let waypointsFilePath="../wp.csv" // <- Sisseloetav teekonnapunktide fail
 static let _waypointData = CsvProvider<Schema = "x (float), y (float), z (float), yaw (float),velocity(float),change_flag(float)", HasHeaders=false>.Load(waypointsFilePath)
 static let waypointCount=(_waypointData.Rows|>Seq.length)
 static member val positionState=PositionState.Start with get,set
 static member val transition=Transitions.Move with get, set
 static member val stepcount=(int)0 with get,set
 static member val numOfWpPubAtOnce=1 with get,set
 static member val startedWaitingMsg=false with get,set
 //Lubatud olekud
 [<AcceptingStateCondition>]
 static member StoppedAndFinished() = (true)
 //Alg positsiooni toiming
 [<Action>]
 static member StartPosition([<Domain("CoordinatesX")>] xcoord:string,[<Domain("CoordinatesY")>] xcoord2:string,[<Domain("CoordinatesZ")>] ycoord2:string,[<Domain("Velocity")>] zcoord2:string,[<Domain("Yaw")>] yaw:string,[<Domain("CountOfWp")>]wp:int32)=
   iseautoTest.stepcount<-iseautoTest.stepcount+1
   iseautoTest.numOfWpPubAtOnce<-2 //<-- Ette antavate teekonnapunktide arvu muutmine
   iseautoTest.transition<-Transitions.Move
   iseautoTest.positionState<-PositionState.ReacheWp
   iseautoTest.startedWaitingMsg<-true
    
 static member StartPositionEnabled()= (iseautoTest.positionState=PositionState.Start) 
 //Auto liigutamise toiming
 [<Action>]
  static member MoveCar([<Domain("CoordinatesX")>] xcoord:string,[<Domain("CoordinatesY")>] xcoord2:string,[<Domain("CoordinatesZ")>] ycoord2:string,[<Domain("Velocity")>] zcoord2:string,[<Domain("Yaw")>] yaw:string,[<Domain("CountOfWp")>]wp:int32)=
   iseautoTest.stepcount<-iseautoTest.stepcount+iseautoTest.numOfWpPubAtOnce
   if iseautoTest.positionState=PositionState.ReacheWp then
     iseautoTest.transition <-Transitions.Wait
     
   else
     if iseautoTest.stepcount>waypointCount-1 then
      iseautoTest.positionState<-PositionState.Finish
      iseautoTest.transition<-Transitions.Stop
  
     else 
      iseautoTest.transition<-Transitions.Wait
      
   

   static member MoveCarEnabled()=( iseautoTest.transition=Transitions.Move && iseautoTest.positionState=PositionState.ReacheWp  && iseautoTest.stepcount<=waypointCount-1)
 

    static member CoordinatesX () : NModel.Set<string> =
        let waypointData2 =CsvProvider<Schema="x (float), y (float), z (float), yaw (float), velocity (float), change_flag (float)",HasHeaders=false>.Load(waypointsFilePath) 
        let aSequence =waypointData2.Rows|>Seq.skip (iseautoTest.stepcount) |>Seq.take  (if iseautoTest.stepcount+iseautoTest.numOfWpPubAtOnce-1<waypointCount then iseautoTest.numOfWpPubAtOnce else if iseautoTest.stepcount>waypointCount then 0  else waypointCount-iseautoTest.stepcount )
        let result= aSequence|>Seq.fold(fun acc x -> acc.ToString() + x.X.ToString() + ",") ""
        if result.Length>0 then
           let resultList=result.Remove(result.Length-1,1)
           new NModel.Set<string>(resultList)
        else 
           new NModel.Set<string>(result)
      
   static member CoordinatesY () : NModel.Set<string> =
        let waypointData2 =CsvProvider<Schema="x (float), y (float), z (float), yaw (float), velocity (float), change_flag (float)",HasHeaders=false>.Load(waypointsFilePath) 
        let aSequence =waypointData2.Rows|>Seq.skip (iseautoTest.stepcount) |>Seq.take  (if iseautoTest.stepcount+iseautoTest.numOfWpPubAtOnce-1<waypointCount then iseautoTest.numOfWpPubAtOnce else if iseautoTest.stepcount>waypointCount then 0  else waypointCount-iseautoTest.stepcount )
        let result= aSequence|>Seq.fold(fun acc x -> acc.ToString() + x.Y.ToString() +  ",") ""
        if result.Length>0 then
           let resultList=result.Remove(result.Length-1,1)
           new NModel.Set<string>(resultList)
        else 
           new NModel.Set<string>(result)

   static member CoordinatesZ () : NModel.Set<string> =
        let waypointData2 =CsvProvider<Schema="x (float), y (float), z (float), yaw (float), velocity (float), change_flag (float)",HasHeaders=false>.Load(waypointsFilePath) 
        let aSequence =waypointData2.Rows|>Seq.skip (iseautoTest.stepcount) |>Seq.take  (if iseautoTest.stepcount+iseautoTest.numOfWpPubAtOnce-1<waypointCount then iseautoTest.numOfWpPubAtOnce else if iseautoTest.stepcount>waypointCount then 0  else waypointCount-iseautoTest.stepcount )
        let result= aSequence|>Seq.fold(fun acc x -> acc.ToString() + x.Z.ToString() + ",") ""
        if result.Length>0 then
           let resultList=result.Remove(result.Length-1,1)
           new NModel.Set<string>(resultList)
        else 
           new NModel.Set<string>(result)

   static member Velocity () : NModel.Set<string> =
        let waypointData2 =CsvProvider<Schema="x (float), y (float), z (float), yaw (float), velocity (float), change_flag (float)",HasHeaders=false>.Load(waypointsFilePath) 
        let aSequence =waypointData2.Rows|>Seq.skip (iseautoTest.stepcount) |>Seq.take (if iseautoTest.stepcount+iseautoTest.numOfWpPubAtOnce-1<waypointCount then iseautoTest.numOfWpPubAtOnce else if iseautoTest.stepcount>waypointCount then 0  else waypointCount-iseautoTest.stepcount )
        let result= aSequence|>Seq.fold(fun acc x -> acc.ToString() + x.Velocity.ToString() + ",") ""
        if result.Length>0 then
           let resultList=result.Remove(result.Length-1,1)
           new NModel.Set<string>(resultList)
        else 
           new NModel.Set<string>(result)
      
   static member Yaw () : NModel.Set<string> =
        let waypointData2 =CsvProvider<Schema="x (float), y (float), z (float), yaw (float), velocity (float), change_flag (float)",HasHeaders=false>.Load(waypointsFilePath) 
        let aSequence =waypointData2.Rows|>Seq.skip (iseautoTest.stepcount) |>Seq.take  (if iseautoTest.stepcount+iseautoTest.numOfWpPubAtOnce-1<waypointCount then iseautoTest.numOfWpPubAtOnce else if iseautoTest.stepcount>waypointCount then 0  else waypointCount-iseautoTest.stepcount )
        let result= aSequence|>Seq.fold(fun acc x -> acc.ToString() + x.Yaw.ToString() + ",") ""
        if result.Length>0 then
           let resultList=result.Remove(result.Length-1,1)
           new NModel.Set<string>(resultList)
        else 
           new NModel.Set<string>(result)
   static member CountOfWp () : NModel.Set<int32> =
           new NModel.Set<int32>((if iseautoTest.stepcount+iseautoTest.numOfWpPubAtOnce-1<waypointCount then iseautoTest.numOfWpPubAtOnce else if iseautoTest.stepcount>waypointCount then 0  else waypointCount-iseautoTest.stepcount ))
   //Auto teekonnapunkti jõudmise kontrollimise toimingud   
  [<Action>]
  static member didReacheWaypoint_Start()=
    iseautoTest.transition<-Transitions.ObserveMsg
    iseautoTest.positionState<-PositionState.ReacheWp
    iseautoTest.startedWaitingMsg<-true
    

  static member didReacheWaypoint_StartEnabled()=(iseautoTest.transition=Transitions.Wait )

  [<Action>]
  static member didReacheWaypoint_Finish()=
      iseautoTest.transition<-Transitions.Move
      iseautoTest.startedWaitingMsg<-false
  
    
    
  static member didReacheWaypoint_FinishEnabled()=(iseautoTest.startedWaitingMsg && iseautoTest.transition=Transitions.ObserveMsg)
  
  //Toiming mis viitab et sellele et kõik teekonna punktid on edukalt läbitud
  [<Action>]
  static member Goal()=
   iseautoTest.positionState<-PositionState.Finish
   iseautoTest.transition<-Transitions.Stop

  static member GoalEnabled()=(iseautoTest.transition=Transitions.Move && iseautoTest.positionState=PositionState.ReacheWp && iseautoTest.stepcount>=waypointCount)



type Contract()=
    static member Create()=
        LibraryModelProgram(typedefof<Contract>.Assembly,"IseautoModel")