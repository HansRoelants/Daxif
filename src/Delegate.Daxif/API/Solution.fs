﻿namespace DG.Daxif

open DG.Daxif.Common
open DG.Daxif.Modules.Solution
open Utility

type Solution private () =

  /// <summary>Imports a solution package from a given environment</summary>
  /// <param name="env">Environment the action should be performed against.</param>
  static member Import(env: Environment, pathToSolutionZip, ?activatePluginSteps, ?extended, ?logLevel) =
    let usr, pwd, dmn = env.getCreds()
    let logLevel = logLevel ?| LogLevel.Verbose
    let extended = extended ?| false

    match extended with
    | true  -> Main.importWithDGSolution
    | false -> Main.import
    |> fun f -> f env.url pathToSolutionZip env.apToUse usr pwd dmn logLevel
    
    match activatePluginSteps with
    | Some true -> 
      let solutionName, _ = CrmUtility.getSolutionInformationFromFile pathToSolutionZip
      Main.pluginSteps env.url solutionName true env.apToUse usr pwd dmn logLevel
    | _ -> ()


  /// <summary>Exports a solution package from a given environment</summary>
  /// <param name="env">Environment the action should be performed against.</param>
  static member Export(env: Environment, solutionName, outputDirectory, managed, ?extended, ?deltaFromDate, ?logLevel) =
    let usr, pwd, dmn = env.getCreds()
    let logLevel = logLevel ?| LogLevel.Verbose
    let extended = extended ?| false

    match extended with
    | true  -> Main.exportWithDGSolution 
    | false -> Main.export
    |> fun f -> f env.url solutionName outputDirectory managed env.apToUse usr pwd dmn logLevel


  /// <summary>Generates TypeScript context from a given environment and settings using XrmDefinitelyTyped</summary>
  /// <param name="env">Environment the action should be performed against.</param>
  static member GenerateTypeScriptContext(env: Environment, pathToXDT, outputDir, ?solutions, ?entities, ?extraArguments, ?logLevel) =
    let usr, pwd, dmn = env.getCreds()
    let logLevel = logLevel ?| LogLevel.Verbose
    
    let solutions = solutions ?| []
    let entities = entities ?| []
    let extraArguments = extraArguments ?| []

    Main.updateTypeScriptContext env.url outputDir env.apToUse usr pwd dmn pathToXDT logLevel solutions entities extraArguments
    
  /// <summary>Generates C# context from a given environment and settings using XrmContext</summary>
  /// <param name="env">Environment the action should be performed against.</param>
  static member GenerateCSharpContext(env: Environment, pathToXrmContext, outputDir, ?solutions, ?entities, ?extraArguments, ?logLevel) =
    let usr, pwd, dmn = env.getCreds()
    let logLevel = logLevel ?| LogLevel.Verbose
    
    let solutions = solutions ?| []
    let entities = entities ?| []
    let extraArguments = extraArguments ?| []

    Main.updateCustomServiceContext env.url outputDir env.apToUse usr pwd dmn pathToXrmContext logLevel solutions entities extraArguments


  /// <summary>Counts the amount of entities in a solution</summary>
  /// <param name="env">Environment the action should be performed against.</param>
  static member Count(env: Environment, solutionName, ?logLevel) =
    let usr, pwd, dmn = env.getCreds()
    let logLevel = logLevel ?| LogLevel.Verbose
    
    Main.count env.url solutionName env.apToUse usr pwd dmn logLevel


  /// <summary>Activates or deactivates all plugin steps of a solution</summary>
  /// <param name="env">Environment the action should be performed against.</param>
  static member EnablePluginSteps(env: Environment, solutionName, ?enable, ?logLevel) =
    Main.enablePluginSteps env solutionName enable logLevel


  /// <summary>Creates a solution in the given environment</summary>
  /// <param name="env">Environment the action should be performed against.</param>
  static member Create(env: Environment, name, displayName, publisherPrefix, ?logLevel) =
    let usr, pwd, dmn = env.getCreds()
    let logLevel = logLevel ?| LogLevel.Verbose
    
    Main.create env.url name displayName publisherPrefix env.apToUse usr pwd dmn logLevel


  /// <summary>Creates a publish in the given environment</summary>
  /// <param name="env">Environment the action should be performed against.</param>
  static member CreatePublisher(env: Environment, name, displayName, prefix, ?logLevel) =
    let usr, pwd, dmn = env.getCreds()
    let logLevel = logLevel ?| LogLevel.Verbose
    
    Main.createPublisher env.url name displayName prefix env.apToUse usr pwd dmn logLevel
