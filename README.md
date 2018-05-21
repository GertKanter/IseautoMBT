# Self-driving car model-based testing 


## Kaustad ja nende sisu lühikirjeldus

* Kaustas "Tulemused" asuvad logifailid.
* Kaustas "FsharpData" asuvad F# Data teegid, mille abil loeme ka csv failidest infot.
* Kaustas "Reset" asuvad resetimisega seotud skriptid.
* Kaustas "Turtlesim_näide" on MBT ja ROS'iga tutvumise jaoks loodud näide kilpkonnaga.
* Kaustas "bin" asuvad NModel'i mugantatud versiooni DLL-teegid ja vahendid (ct,mpv,otg jne).
* Kaustas "iseautoMudel" on isesõitva auto MBT test ja adapter.
    1. MBT Mudel - iseautoMudel->IseautoModel->ProtableLibrary1.fs
    2. MBT Adapter - IseautoMudel -> IseautoStepper->Class1.cs
    3. MPV skript - iseautoMudel->IseautoModel->Script.fsx (Mudeli visualiseerimiseks)
    4. Samas kaustas ka näidised teekonnapunktide failidest .csv faili formaadis
* Kaustas "ros-sharp" asub ros# versioon koos tehtud muudatustega.
* Kaustas "Test" asuvad iseauto testide käivitamisega seonduvad NModel vahendid teegid, argumendifailid, mudeli ja adapteri DLL-teegid
* Kaustas "Koodikatvus_Pildid" asuvad pildid sõlmede graafist ja koodikatvuse tulemused pure_pursuit sõlmest
* Koodikatvus kaustas Koodikatvus_Pildid->Pure_pursuit

## Directories and short description of what is inside of them

* Directory named "Tulemused" contains log files.
* Directory named  "FsharpData" contains F# Data library.
* Directory named  "Reset" contains scripts for resetting the car pos and world (Currently world reset is commented out).
* Directory named  "Turtlesim_näide" contains example made with turtlesim for MBT testing.
* Directory named  "bin" contains  NModels version that supports F# language.
* Directory named  "iseautoMudel" contains model and stepper (adapter).
⋅⋅* MBT Model - iseautoMudel->IseautoModel->ProtableLibrary1.fs
⋅⋅* MBT Adapter - IseautoMudel -> IseautoStepper->Class1.cs
⋅⋅* MPV script - iseautoMudel->IseautoModel->Script.fsx (for visualizing the model with NModel's MPV)
⋅⋅* The above folder contains also .csv failis with waypoints
* Directory named  "ros-sharp" contains ros# version with modifications to support Autoware and other needed messages.
* Directory named  "Test" contains  necessary tools, argument files, model and adapter dll's  related to MBT testing with Autoware.
* Directory named  "Koodikatvus_Pildid" contains picture of graph that shows the nodes that where involved in moving the car (taken with rqt) and code coverage results from pure_pursuit node.  
 * Code coverage report can be found in  Koodikatvus_Pildid->Pure_pursuit
