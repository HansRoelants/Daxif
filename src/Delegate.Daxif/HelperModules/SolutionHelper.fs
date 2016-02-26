﻿namespace DG.Daxif.HelperModules

open System
open System.IO
open System.Text
open Microsoft.Crm.Sdk
open Microsoft.Crm.Tools.SolutionPackager
open Microsoft.Xrm.Sdk
open DG.Daxif
open DG.Daxif.HelperModules.Common
open DG.Daxif.HelperModules.Common.Utility

module internal SolutionHelper =

  let createPublisher' org ac name display prefix 
      (log : ConsoleLogger.ConsoleLogger) = 
    let m = ServiceManager.createOrgService org
    let tc = m.Authenticate(ac)
    use p = ServiceProxy.getOrganizationServiceProxy m tc

    log.WriteLine(LogLevel.Verbose, @"Service Manager instantiated")
    log.WriteLine(LogLevel.Verbose, @"Service Proxy instantiated")

    let pid = CrmData.Entities.createPublisher p name display prefix
    let msg = 
      @"Publisher was created successfully (Publisher ID: " + pid.ToString() 
      + @")"

    log.WriteLine(LogLevel.Verbose, msg)

  let create' org ac name display pubPrefix (log : ConsoleLogger.ConsoleLogger) = 
    let m = ServiceManager.createOrgService org
    let tc = m.Authenticate(ac)
    use p = ServiceProxy.getOrganizationServiceProxy m tc

    log.WriteLine(LogLevel.Verbose, @"Service Manager instantiated")
    log.WriteLine(LogLevel.Verbose, @"Service Proxy instantiated")

    let sid = CrmData.Entities.createSolution p name display pubPrefix
    let msg = 
      @"Solution was created successfully (Solution ID: " + sid.ToString() + @")"

    log.WriteLine(LogLevel.Verbose, msg)


  let delete' org ac solution (log : ConsoleLogger.ConsoleLogger) = 
    let m = ServiceManager.createOrgService org
    let tc = m.Authenticate(ac)
    use p = ServiceProxy.getOrganizationServiceProxy m tc

    log.WriteLine(LogLevel.Verbose, @"Service Manager instantiated")
    log.WriteLine(LogLevel.Verbose, @"Service Proxy instantiated")

    let s = CrmData.Entities.retrieveSolution p solution
    CrmData.CRUD.delete p s.LogicalName s.Id |> ignore
    let msg = 
      @"Solution was deleted successfully (Solution ID: " + s.Id.ToString() + @")"

    log.WriteLine(LogLevel.Verbose, msg)


  let pluginSteps' org ac solution enable (log : ConsoleLogger.ConsoleLogger) = 
    // Plugin: stateCode = 1 and statusCode = 2 (inactive), 
    //         stateCode = 0 and statusCode = 1 (active) 
    // Remark: statusCode = -1, will default the statuscode for the given statecode
    let state, status = 
      enable |> function 
      | false -> 1, (-1)
      | true -> 0, (-1)
      
    let m = ServiceManager.createOrgService org
    let tc = m.Authenticate(ac)
    use p = ServiceProxy.getOrganizationServiceProxy m tc

    log.WriteLine(LogLevel.Verbose, @"Service Manager instantiated")
    log.WriteLine(LogLevel.Verbose, @"Service Proxy instantiated")

    let s = CrmData.Entities.retrieveSolution p solution
    CrmData.Entities.retrieveAllPluginProcessingSteps p s.Id
    |> Seq.toArray
    |> Array.Parallel.iter 
          (fun e -> 
          use p' = ServiceProxy.getOrganizationServiceProxy m tc
          let en' = e.LogicalName
          let ei' = e.Id.ToString()
          try 
            CrmData.Entities.updateState p' en' e.Id state status
            log.WriteLine
              (LogLevel.Verbose, sprintf "%s:%s state was updated" en' ei')
          with ex -> 
            log.WriteLine(LogLevel.Warning, sprintf "%s:%s %s" en' ei' ex.Message))
    let msg' = 
      enable |> function 
      | true -> "enabled"
      | false -> "disabled"
      
    let msg = 
      @"The solution plugins were successfully " + msg' + @"(Solution ID: " 
      + s.Id.ToString() + @")"
    log.WriteLine(LogLevel.Verbose, msg)


  let workflow' org ac solution enable (log : ConsoleLogger.ConsoleLogger) = 
    // Workflow: stateCode = 0 and statusCode = 1 (inactive), 
    //           stateCode = 1 and statusCode = 2 (active)
    // Remark: statusCode = -1, will default the statuscode for the given statecode
    let state, status, retrievedStatus = 
      enable |> function 
      | false -> 0, (-1), 2
      | true -> 1, (-1), 1
      
    let m = ServiceManager.createOrgService org
    let tc = m.Authenticate(ac)
    use p = ServiceProxy.getOrganizationServiceProxy m tc

    log.WriteLine(LogLevel.Verbose, @"Service Manager instantiated")
    log.WriteLine(LogLevel.Verbose, @"Service Proxy instantiated")

    let s = CrmData.Entities.retrieveSolution p solution
    CrmData.Entities.retrieveWorkflowsOfStatus p s.Id retrievedStatus
    |> Seq.toArray
    |> fun w -> 
      match w.Length with
      | 0 -> log.WriteLine(LogLevel.Verbose, @"No workflows were updated")
      | _ -> 
        w 
        |> Array.Parallel.iter (fun e -> 
              use p' = ServiceProxy.getOrganizationServiceProxy m tc
              let en' = e.LogicalName
              let ei' = e.Id.ToString()
              try 
                CrmData.Entities.updateState p' en' e.Id state status
                log.WriteLine
                  (LogLevel.Verbose, sprintf "%s:%s state was updated" en' ei')
              with ex -> 
                log.WriteLine
                  (LogLevel.Warning, sprintf "%s:%s %s" en' ei' ex.Message))
        let msg' = 
          enable |> function 
          | true -> "enabled"
          | false -> "disabled"
          
        let msg = 
          @"The solution workflows were successfully " + msg' + @" (Solution ID: " 
          + s.Id.ToString() + @")"
        log.WriteLine(LogLevel.Verbose, msg)


  let export' org ac solution location managed (log : ConsoleLogger.ConsoleLogger) = 
    let m = ServiceManager.createOrgService org
    let tc = m.Authenticate(ac)
    use p = ServiceProxy.getOrganizationServiceProxy m tc
    do p.Timeout <- new TimeSpan(0, 59, 0) // 59 minutes timeout
    let req = new Messages.ExportSolutionRequest()

    log.WriteLine(LogLevel.Verbose, @"Service Manager instantiated")
    log.WriteLine(LogLevel.Verbose, @"Service Proxy instantiated")

    req.Managed <- managed
    req.SolutionName <- solution

    log.WriteLine(LogLevel.Verbose, @"Proxy timeout set to 1 hour")
    log.WriteLine(LogLevel.Verbose, @"Export solution")

    let resp = p.Execute(req) :?> Messages.ExportSolutionResponse

    log.WriteLine(LogLevel.Verbose, @"Solution was exported successfully")

    let zipFile = resp.ExportSolutionFile
    let filename =
      let managed' =
        match managed with
        | true -> "_managed"
        | false -> ""
      sprintf "%s%s.zip" solution managed'

    File.WriteAllBytes(location + filename, zipFile)

    log.WriteLine(LogLevel.Verbose, @"Solution saved to local disk")


  let import' org ac solution location managed (log : ConsoleLogger.ConsoleLogger) = 
    let m = ServiceManager.createOrgService org
    let tc = m.Authenticate(ac)
    use p = ServiceProxy.getOrganizationServiceProxy m tc
    do p.Timeout <- new TimeSpan(0, 59, 0) // 59 minutes timeout

    log.WriteLine(LogLevel.Verbose, @"Service Manager instantiated")
    log.WriteLine(LogLevel.Verbose, @"Service Proxy instantiated")

    let zipFile = File.ReadAllBytes(location)

    log.WriteLine(LogLevel.Verbose, @"Solution file loaded successfully")

    let jobId = Guid.NewGuid()
    let req = new Messages.ImportSolutionRequest()

    req.CustomizationFile <- zipFile
    req.ImportJobId <- jobId
    req.ConvertToManaged <- managed
    req.OverwriteUnmanagedCustomizations <- true
    log.WriteLine(LogLevel.Verbose, @"Proxy timeout set to 1 hour")

    let rec importHelper' exists completed progress = 
      async { 
        match exists with
        | false -> 
          // TODO: Return error if the job is never created
          // Indicated by exist always being false 
          let exists' = 
            use p' = ServiceProxy.getOrganizationServiceProxy m tc
            CrmData.Entities.existCrm p' @"importjob" jobId None
          do! importHelper' exists' completed progress
        | true -> 
          match completed with
          | true -> ()
          | false -> 
            do! Async.Sleep(10 * 1000) // 10 seconds
            let (pct, completed') = 
              use p' = ServiceProxy.getOrganizationServiceProxy m tc
              try 
                let j = CrmData.Entities.retrieveImportJob p' jobId
                let progress' = j.Attributes.["progress"] :?> double
                (progress', j.Attributes.Contains("completedon"))
              with _ -> (progress, false)
            match completed' with
            | false -> 
              let msg = 
                @"Import solution: " + solution + @" (" + string (pct |> int) 
                + @"%)"
              log.WriteLine(LogLevel.Verbose, msg)
            | true -> 
              use p' = ServiceProxy.getOrganizationServiceProxy m tc
                
              let status = 
                try 
                  let j = CrmData.Entities.retrieveImportJob p' jobId
                  let progress' = j.Attributes.["progress"] :?> double
                  progress' = 100.
                with _ -> false
              match status with
              | true -> 
                let msg = 
                  @"Solution was imported successfully (ImportJob ID: " 
                  + jobId.ToString() + @")"
                log.WriteLine(LogLevel.Verbose, msg)
              | false -> 
                failwith 
                  (@"Solution import failed (ImportJob ID: " + jobId.ToString() 
                    + @")")
              match managed with
              | true -> ()
              | false -> 
                log.WriteLine(LogLevel.Verbose, @"Publishing solution")
                CrmData.CRUD.publish p'
                log.WriteLine
                  (LogLevel.Verbose, @"The solution was successfully published")
              return ()
            do! importHelper' exists completed' pct
      }
      
    let importHelperAsync() = 
      // Added helper function in order to not having to look for the 
      // Messages.ExecuteAsyncRequest Type for MS CRM 2011 (legacy)
      let areq = new Messages.ExecuteAsyncRequest()
      areq.Request <- req
      p.Execute(areq) :?> Messages.ExecuteAsyncResponse |> ignore
      
    let importHelper() = 
      async { 
        let! progress = Async.StartChild(importHelper' false false 0.)
        match CrmData.Info.version p with
        | (_, CrmReleases.CRM2011) -> 
          p.Execute(req) :?> Messages.ImportSolutionResponse |> ignore
        | (_, _) -> importHelperAsync()
        let! waitForProgress = progress
        waitForProgress
      }
      
    log.WriteLine(LogLevel.Verbose, @"Import solution: " + solution + @" (0%)")
    importHelper()
    |> Async.Catch
    |> Async.RunSynchronously
    |> function 
    | Choice2Of2 exn -> printfn "Error: %A" exn
    | _ -> ()
    let location' = location.Replace(@".zip", "")
    let excel = location' + @"_" + Utility.timeStamp'() + @".xml"
    let req' = new Messages.RetrieveFormattedImportJobResultsRequest()
    req'.ImportJobId <- jobId
    let resp' = 
      p.Execute(req') :?> Messages.RetrieveFormattedImportJobResultsResponse
    let xml = resp'.FormattedResults
    let bytes = Encoding.UTF8.GetBytes(xml)
    let bytes' = SerializationHelper.xmlPrettyPrinterHelper' bytes
    let xml' = "<?xml version=\"1.0\"?>\n" + (Encoding.UTF8.GetString(bytes'))
    File.WriteAllText(excel, xml')
    log.WriteLine(LogLevel.Verbose, @"Import solution results saved to: " + excel)
    excel

  //TODO:
  let extract' location (customizations : string) (map : string) project 
      (log : ConsoleLogger.ConsoleLogger) logl = 
    let logl' = Enum.GetName(typeof<LogLevel>, logl)
    let pa = new PackagerArguments()
    log.WriteLine(LogLevel.Info, "Start output from SolutionPackager")
    // Use parser to ensure proper initialization of arguments
    Parser.ParseArgumentsWithUsage(
      [| "/action:Extract";
         "/packagetype:Both";
          sprintf @"/zipfile:%s" location;
          sprintf @"/folder:%s" customizations;
          sprintf @"/map:%s" map;
          sprintf @"/errorlevel:%s" logl';
          "/allowDelete:Yes";
          "/clobber"; |], pa)
    |> ignore
    try 
      let sp = new SolutionPackager(pa)
      sp.Run()
    with ex -> log.WriteLine(LogLevel.Error, sprintf "%s" ex.Message)
    log.WriteLine(LogLevel.Info, "End output from SolutionPackager")
    touch project
    ()
      
  //TODO:
  let pack' location customizations map managed (log : ConsoleLogger.ConsoleLogger) logl = 
    let logl' = Enum.GetName(typeof<LogLevel>, logl)
    let pa = new PackagerArguments()
    let managed' = match managed with | true -> "Managed" | false -> "Unmanaged"
    log.WriteLine(LogLevel.Info, "Start output from SolutionPackager")
    // Use parser to ensure proper initialization of arguments
    Parser.ParseArgumentsWithUsage(
      [| "/action:Pack";
          sprintf @"/packagetype:%s" managed'; 
          sprintf @"/zipfile:%s" location;
          sprintf @"/folder:%s" customizations;
          sprintf @"/map:%s" map;
          sprintf @"/errorlevel:%s" logl'; |], pa)
    |> ignore
    try 
      let sp = SolutionPackager(pa)
      sp.Run()
    with ex -> log.WriteLine(LogLevel.Error, ex.Message)
    log.WriteLine(LogLevel.Info, "End output from SolutionPackager")

  let updateServiceContext' org location ap usr pwd domain exe lcid (log:ConsoleLogger.ConsoleLogger) =
    let lcid:int option = lcid
    let lcid' =
      match lcid with
      | Some v -> string v
      | None -> System.String.Empty

    let csu () =
      let args = 
        (sprintf "/metadataproviderservice:\"DG.MetadataProvider.IfdMetadataProviderService, Delegate.MetadataProvider\" \
                  /url:\"%s\" \
                  /username:\"%s\" \
                  /password:\"%s\" \
                  /domain:\"%s\" \
                  /language:cs \
                  /namespace:DG.XrmFramework.BusinessDomain.ServiceContext \
                  /serviceContextName:Xrm \
                  /out:\"%s\Xrm.cs\"" (org.ToString()) usr pwd domain location)

      Utility.executeProcess(exe,args) |> Some 

    let csu' () =
      let args =
        (sprintf "/metadataproviderservice:\"DG.MetadataProvider.IfdMetadataProviderService, Delegate.MetadataProvider\" \
                  /codewriterfilter:\"Microsoft.Crm.Sdk.Samples.FilteringService, Delegate.GeneratePicklistEnums\" \
                  /codecustomization:\"Microsoft.Crm.Sdk.Samples.CodeCustomizationService, Delegate.GeneratePicklistEnums\" \
                  /namingservice:\"Microsoft.Crm.Sdk.Samples.NamingService%s, Delegate.GeneratePicklistEnums\" \
                  /url:\"%s\" \
                  /username:\"%s\" \
                  /password:\"%s\" \
                  /domain:\"%s\" \
                  /language:\"cs\" \
                  /namespace:\"DG.XrmFramework.BusinessDomain.ServiceContext.OptionSets\" \
                  /serviceContextName:\"XrmOptionSets\" \
                  /out:\"%s\XrmOptionSets.cs\"" lcid' (org.ToString()) usr pwd domain location)
      Utility.executeProcess(exe,args) |> Some 

    csu ()
    |> printProcess "MS CrmSvcUtil SDK" log

    csu' ()
    |> printProcess "MS CrmSvcUtil SDK (Option Sets)" log


  let toArgString = 
    Seq.map (fun (k, v) -> sprintf "/%s:\"%s\"" k v) >> String.concat " "
    
  let updateCustomServiceContext' org location ap usr pwd domain exe log 
      (solutions : string list) (entities : string list) extraArgs = 
    let ccs() = 
      let args = 
        [ "url", org.ToString()
          "username", usr
          "password", pwd
          "domain", domain
          "ap", ap.ToString()
          "out", location
          "solutions", (solutions |> fun ss -> String.Join(",", ss))
          "entities", (entities |> fun es -> String.Join(",", es))
          "servicecontextname", "Xrm"
          "namespace", "DG.XrmFramework.BusinessDomain.ServiceContext" ]
        
      let args = args @ extraArgs
      Utility.executeProcess (exe, args |> toArgString) |> Some
    ccs() |> printProcess "DG XrmContext" log
    
  let updateTypeScriptContext' org location ap usr pwd domain exe log 
      (solutions : string list) (entities : string list) extraArgs = 
    let dts() = 
      let args = 
        [ "url", org.ToString()
          "username", usr
          "password", pwd
          "domain", domain
          "ap", ap.ToString()
          "out", location
          "solutions", (solutions |> fun ss -> String.Join(",", ss))
          "entities", (entities |> fun es -> String.Join(",", es)) ]
        
      let args = args @ extraArgs
      Utility.executeProcess (exe, args |> toArgString)
    let (code, es, os) = dts()
    (code, es, os) |> Some |> printProcess "Delegate XrmDefinitelyTyped" log
    match code with
    | 0 -> ()
    | _ -> failwith (sprintf "Delegate XrmDefinitelyTyped failed")

  let count' org solutionName ac (log : ConsoleLogger.ConsoleLogger) = 
    let m = ServiceManager.createOrgService org
    let tc = m.Authenticate(ac)
    use p = ServiceProxy.getOrganizationServiceProxy m tc
    let solution = CrmData.Entities.retrieveSolution p solutionName
    CrmData.Entities.countEntities p solution.Id
    