#if INTERACTIVE
#r @"..\..\..\..\bin\NModel.dll"
#r @"..\..\..\..\bin\NModel.Visualization.dll"
#r @"bin\Debug\TurtleModel.dll"
#endif


NModel.Visualization.Interactive.Run(TurtleModel.Contract.Create())
