﻿(**
SolutionImportTest
=================
*)

#load @"_Config.fsx"
open _Config
open DG.Daxif
open DG.Daxif.Common.Utility

let zip = Path.Daxif.crmSolutionsFolder ++ (XrmSolution.name + @".zip")

Solution.Import(Env.test, zip, activatePluginSteps = true, extended = true)
