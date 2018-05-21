# Self-driving car model-based testing 


## Kaustad ja nende sisu (Folders and short description of what is inside of them)

* Kaustas "Tulemused" asuvad logifailid.
* Kaustas "FsharpData" asuvad F# Data teegid, mille abil loeme ka csv failidest infot.
* Kaustas "Reset" asuvad resetimisega seotud skriptid.
* Kaustas "Turtlesim_näide" on MBT ja ROS'iga tutvumise jaoks loodud näide kilpkonnaga.
* Kaustas "bin" asuvad NModel'i mugantatud versiooni DLL-teegid ja vahendid (ct,mpv,otg jne).
* Kaustas "iseautoMudel" on isesõitva auto MBT test ja adapter.
 * MBT Mudel - iseautoMudel->IseautoModel->ProtableLibrary1.fs
 * MBT Adapter - IseautoMudel -> IseautoStepper->Class1.cs
 * MPV skript - iseautoMudel->IseautoModel->Script.fsx (Mudeli visualiseerimiseks)
 * Samas kaustas ka näidised teekonnapunktide failidest .csv faili formaadis
* Kaustas "ros-sharp" asub ros# versioon koos tehtud muudatustega.
* Kaustas "Test" asuvad iseauto testide käivitamisega seonduvad NModel vahendid teegid, argumendifailid, mudeli ja adapteri DLL-teegid
* Kaustas "Koodikatvus_Pildid" asuvad pildid sõlmede graafist ja koodikatvuse tulemused pure_pursuit sõlmest
 * Koodikatvus kaustas Koodikatvus_Pildid->Pure_pursuit
