﻿namespace DG.Daxif.HelperModules.Common

#nowarn "40"

open System
open System.IO
open System.Reflection
open System.Threading

open DG.Daxif

// TODO: 
module internal ConsoleLogger = 
  type private cc = ConsoleColor
  
  type Agent = MailboxProcessor<LogLevel * string>

  type ConsoleLogger(logLevel : LogLevel) = 
    let threadSafe = ref String.Empty
    let level = logLevel
    let ts'' = 
      let ts = DateTime.Now.ToString("o")
      ts.Substring(0, ts.IndexOf("T"))
    let file' = 
      #if INTERACTIVE
        Path.GetTempPath() +
      #else
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
      #endif
        Path.DirectorySeparatorChar.ToString() + ts'' + "_DAXIF#.log"
    let ofc = Console.ForegroundColor
    let obc = Console.BackgroundColor
    
    let prettyPrint fc bc level' str = 
      Monitor.Enter threadSafe
      try 
        let ts = DateTime.Now.ToString("o") // ISO-8601
        let (level'' : LogLevel) = level'
        let pre = ts + " - " + level''.ToString() + ": "
        Console.ForegroundColor <- fc
        Console.BackgroundColor <- bc
        match level with
        | LogLevel.Warning | LogLevel.Error -> eprintfn "%s" (pre + str)
        | _ -> printfn "%s" (pre + str)
      finally
        Console.ForegroundColor <- ofc
        Console.BackgroundColor <- obc
        Monitor.Exit threadSafe
    
    let logger logLevel str = 
      match logLevel with
      | LogLevel.Info -> prettyPrint cc.White obc LogLevel.Info str
      | LogLevel.Warning -> prettyPrint cc.Yellow obc LogLevel.Warning str
      | LogLevel.Error -> prettyPrint cc.White cc.Red LogLevel.Error str
      | LogLevel.Verbose -> prettyPrint cc.Gray obc LogLevel.Verbose str
      | LogLevel.File -> failwith "LogLevel.File is @deprecated."
      | LogLevel.Debug -> prettyPrint cc.DarkYellow obc LogLevel.Debug str
      | _ -> ()
    
    member t.WriteLine(logLevel, str) = 
      match (level.HasFlag logLevel) with
      | false -> ()
      | true -> logger logLevel str

    member t.overwriteLastLine() = 
      Console.SetCursorPosition(0,Console.CursorTop-1)

    member t.LogLevel =
      level
