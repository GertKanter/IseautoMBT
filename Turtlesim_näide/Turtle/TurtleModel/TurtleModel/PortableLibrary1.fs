namespace TurtleModel

open NModel.Attributes
open NModel.Execution
open System
open RosSharp.RosBridgeClient

//0- liigutame konna 1- konn on liikunud 2- konnal on sein ees
type turtleState=Move=0|didMove=1


type TurtleModelTest()=
  static member val velocity=0.5 //<-kilpkonna liikumise kiirus
  static member val angular=0.0  //<-kilpkonna liikumise nurk
  static member val turtleState=turtleState.didMove with get,set //kilpkonna positsioonid

  //Kõik olekud on lubatud mille tulemusna kilpkonn ei ole seina sõitnud


  [<Action>]
  static member MoveTurtle([<Domain("Coordinates")>] xcoord:float32, [<Domain("CoordinatesX")>] zcoord2:float32)=
     TurtleModelTest.turtleState<-turtleState.didMove
   
   //Kilpkonn saab liikuda siis ja ainult siis kui tingimuseks on määratud Move, ehk kilpkonn ei ole seina sõitnud
   static member MoveTurtleEnabled()=(TurtleModelTest.turtleState=turtleState.Move)
   
   //Kilpkonna võimalikud andmed liikumiseks, kiirus ja suund
   static member Coordinates () : NModel.Set<float32> =
        new NModel.Set<float32>((float32)0.5,(float32)0.6,(float32)0.2,(float32)0.4,(float32)0.5)
   static member CoordinatesX () : NModel.Set<float32> =
        new NModel.Set<float32>((float32)0.7,(float32)0.6,(float32)0.4,(float32)0.4,(float32)0.5)

  //Saame vastuse adapterist, kas konnn on vastu seina või mitte toimingu sooritades
  [<Action>]
  static member checkCollision()=
    TurtleModelTest.turtleState<-turtleState.Move

  //Kilpkonna on liigutatud või soovitakse liigutada ja me saame kontrollida kas ta on kokku põrganud või mitte
  static member checkCollisionEnabled()=(TurtleModelTest.turtleState=turtleState.didMove)

  //Mudeli konstruktor
type Contract()=
    static member Create()=
        LibraryModelProgram(typedefof<Contract>.Assembly,"TurtleModel")
    