﻿namespace Fake.Core

open System
open System.Collections.Generic
open Fake.Core

/// [omit]
type TargetDescription = string

type TargetResult =
    { Error : exn option; Time : TimeSpan; Target : Target; WasSkipped : bool }

and TargetContext =
    { PreviousTargets : TargetResult list }
    static member Empty = { PreviousTargets = [] }
    member x.HasError =
        x.PreviousTargets
        |> List.exists (fun t -> t.Error.IsSome)
    member x.TryFindPrevious name =
        x.PreviousTargets |> List.tryFind (fun t -> t.Target.Name = name)        

and TargetParameter =
    { TargetInfo : Target
      Context : TargetContext }

/// [omit]
and Target =
    { Name: string;
      Dependencies: string list;
      SoftDependencies: string list;
      Description: TargetDescription option;
      Function : TargetParameter -> unit}

/// Exception for request errors
#if !NETSTANDARD1_6
[<System.Serializable>]
#endif
type BuildFailedException =
    val private info : TargetContext option
    inherit Exception
    new (msg:string, inner:exn) = {
      inherit Exception(msg, inner)
      info = None }
    new (info:TargetContext, msg:string, inner:exn) = {
      inherit Exception(msg, inner)
      info = Some info }
#if !NETSTANDARD1_6
    new (info:System.Runtime.Serialization.SerializationInfo, context:System.Runtime.Serialization.StreamingContext) = {
      inherit Exception(info, context)
      info = None
    }
#endif
    member x.Info with get () = x.info
    member x.Wrap() =
        match x.info with
        | Some info ->
            BuildFailedException(info, x.Message, x:>exn)
        | None ->
            BuildFailedException(x.Message, x:>exn)

module Target =

    type private DependencyType =
        | Hard = 1
        | Soft = 2

    /// [omit]
    //let mutable PrintStackTraceOnError = false
    let private printStackTraceOnErrorVar = "Fake.Core.Target.PrintStackTraceOnError"
    let private getPrintStackTraceOnError, _, (setPrintStackTraceOnError:bool -> unit) = 
        Fake.Core.Context.fakeVar printStackTraceOnErrorVar
    
    /// [omit]
    //let mutable LastDescription = null
    let private lastDescriptionVar = "Fake.Core.Target.LastDescription"
    let private getLastDescription, removeLastDescription, setLastDescription = 
        Fake.Core.Context.fakeVar lastDescriptionVar

    /// Sets the Description for the next target.
    /// [omit]
    let Description text =
        match getLastDescription() with
        | Some (v:string) ->
            failwithf "You can't set the description for a target twice. There is already a description: %A" v
        | None ->
           setLastDescription text

    /// TargetDictionary
    /// [omit]
    let internal getVarWithInit name f =
        let varName = sprintf "Fake.Core.Target.%s" name
        let getVar, _, setVar = 
            Fake.Core.Context.fakeVar varName
        fun () ->
            match getVar() with
            | Some d -> d
            | None ->
                let d = f () // new Dictionary<_,_>(StringComparer.OrdinalIgnoreCase)
                setVar d
                d
            
    let internal getTargetDict =
        getVarWithInit "TargetDict" (fun () -> new Dictionary<_,_>(StringComparer.OrdinalIgnoreCase))

    /// Final Targets - stores final targets and if they are activated.
    let internal getFinalTargets =
        getVarWithInit "FinalTargets" (fun () -> new Dictionary<_,_>(StringComparer.OrdinalIgnoreCase))

    /// BuildFailureTargets - stores build failure targets and if they are activated.
    let internal getBuildFailureTargets =
        getVarWithInit "BuildFailureTargets" (fun () -> new Dictionary<_,_>(StringComparer.OrdinalIgnoreCase))


    /// Resets the state so that a deployment can be invoked multiple times
    /// [omit]
    let internal reset() =
        getTargetDict().Clear()
        getBuildFailureTargets().Clear()
        getFinalTargets().Clear()

    /// Returns a list with all target names.
    let internal getAllTargetsNames() = getTargetDict() |> Seq.map (fun t -> t.Key) |> Seq.toList

    /// Gets a target with the given name from the target dictionary.
    let Get name =
        let d = getTargetDict()
        match d.TryGetValue (name) with
        | true, target -> target
        | _  ->
            Trace.traceError <| sprintf "Target \"%s\" is not defined. Existing targets:" name
            for target in d do
                Trace.traceError  <| sprintf "  - %s" target.Value.Name
            failwithf "Target \"%s\" is not defined." name
    
    let internal runSimple context target =
        let name = target.Name
        let watch = System.Diagnostics.Stopwatch.StartNew()
        let error =
            try
                target.Function { TargetInfo = target; Context = context }
                None
            with e -> Some e
        watch.Stop()
        { Error = error; Time = watch.Elapsed; Target = target; WasSkipped = false }
    let internal runSimpleContext target context =
        let result = runSimple context target
        { context with PreviousTargets = context.PreviousTargets @ [result] }


    /// This simply runs the function of a target without doing anything (like tracing, stopwatching or adding it to the results at the end)
    let RunSimple name =
        Get name
        |> runSimple TargetContext.Empty

    /// Returns the DependencyString for the given target.
    let internal dependencyString target =
        if target.Dependencies.IsEmpty then String.Empty else
        target.Dependencies
          |> Seq.map (fun d -> (Get d).Name)
          |> String.separated ", "
          |> sprintf "(==> %s)"

    /// Returns the soft  DependencyString for the given target.
    let internal softDependencyString target =
        if target.SoftDependencies.IsEmpty then String.Empty else
        target.SoftDependencies
          |> Seq.map (fun d -> (Get d).Name)
          |> String.separated ", "
          |> sprintf "(?=> %s)"

    /// Do nothing - Can be used to define empty targets.
    let DoNothing = (fun (_:TargetParameter) -> ())

    /// Checks whether the dependency (soft or normal) can be added.
    /// [omit]
    let internal checkIfDependencyCanBeAddedCore fGetDependencies targetName dependentTargetName =
        let target = Get targetName
        let dependentTarget = Get dependentTargetName

        let rec checkDependencies dependentTarget =
              fGetDependencies dependentTarget
              |> List.iter (fun dep ->
                   if String.toLower dep = String.toLower targetName then
                      failwithf "Cyclic dependency between %s and %s" targetName dependentTarget.Name
                   checkDependencies (Get dep))

        checkDependencies dependentTarget
        target,dependentTarget

    /// Checks whether the dependency can be added.
    /// [omit]
    let internal checkIfDependencyCanBeAdded targetName dependentTargetName =
       checkIfDependencyCanBeAddedCore (fun target -> target.Dependencies) targetName dependentTargetName

    /// Checks whether the soft dependency can be added.
    /// [omit]
    let internal checkIfSoftDependencyCanBeAdded targetName dependentTargetName =
       checkIfDependencyCanBeAddedCore (fun target -> target.SoftDependencies) targetName dependentTargetName

    /// Adds the dependency to the front of the list of dependencies.
    /// [omit]
    let internal dependencyAtFront targetName dependentTargetName =
        let target,dependentTarget = checkIfDependencyCanBeAdded targetName dependentTargetName

        getTargetDict().[targetName] <- { target with Dependencies = dependentTargetName :: target.Dependencies }

    /// Appends the dependency to the list of dependencies.
    /// [omit]
    let internal dependencyAtEnd targetName dependentTargetName =
        let target,dependentTarget = checkIfDependencyCanBeAdded targetName dependentTargetName

        getTargetDict().[targetName] <- { target with Dependencies = target.Dependencies @ [dependentTargetName] }


    /// Appends the dependency to the list of soft dependencies.
    /// [omit]
    let internal softDependencyAtEnd targetName dependentTargetName =
        let target,dependentTarget = checkIfDependencyCanBeAdded targetName dependentTargetName

        getTargetDict().[targetName] <- { target with SoftDependencies = target.SoftDependencies @ [dependentTargetName] }

    /// Adds the dependency to the list of dependencies.
    /// [omit]
    let internal dependency targetName dependentTargetName = dependencyAtEnd targetName dependentTargetName

    /// Adds the dependency to the list of soft dependencies.
    /// [omit]
    let internal softDependency targetName dependentTargetName = softDependencyAtEnd targetName dependentTargetName

    /// Adds the dependencies to the list of dependencies.
    /// [omit]
    let internal Dependencies targetName dependentTargetNames = dependentTargetNames |> List.iter (dependency targetName)

    /// Adds the dependencies to the list of soft dependencies.
    /// [omit]
    let internal SoftDependencies targetName dependentTargetNames = dependentTargetNames |> List.iter (softDependency targetName)

    /// Backwards dependencies operator - x is dependent on ys.
    let inline internal (<==) x ys = Dependencies x ys

    /// Creates a target from template.
    /// [omit]
    let internal addTarget target name =
        getTargetDict().Add(name, target)
        name <== target.Dependencies
        removeLastDescription()
        
    /// add a target with dependencies
    /// [omit]
    let internal addTargetWithDependencies dependencies body name =
        let template =
            { Name = name
              Dependencies = dependencies
              SoftDependencies = []
              Description = getLastDescription()
              Function = body }
        addTarget template name

    /// Creates a Target.
    let Create name body = addTargetWithDependencies [] body name

    /// Runs all activated final targets (in alphabetically order).
    /// [omit]
    let internal runFinalTargets context =
        getFinalTargets()
          |> Seq.filter (fun kv -> kv.Value)     // only if activated
          |> Seq.map (fun kv -> kv.Key)
          |> Seq.fold (fun context name ->
               Trace.tracefn "Starting FinalTarget: %s" name
               let target = Get name
               runSimpleContext target context) context  

    /// Runs all build failure targets.
    /// [omit]
    let internal runBuildFailureTargets (context) =
        getBuildFailureTargets()
          |> Seq.filter (fun kv -> kv.Value)     // only if activated
          |> Seq.map (fun kv -> kv.Key)
          |> Seq.fold (fun context name ->
               Trace.tracefn "Starting BuildFailureTarget: %s" name
               let target = Get name
               runSimpleContext target context) context

    /// List all targets available.
    let ListAvailable() =
        Trace.log "The following targets are available:"
        for t in getTargetDict().Values do
            Trace.logfn "   %s%s" t.Name (match t.Description with Some s -> sprintf " - %s" s | _ -> "")


    // Maps the specified dependency type into the list of targets
    let private withDependencyType (depType:DependencyType) targets =
        targets |> List.map (fun t -> depType, t)

    // Helper function for visiting targets in a dependency tree. Returns a set containing the names of the all the
    // visited targets, and a list containing the targets visited ordered such that dependencies of a target appear earlier
    // in the list than the target.
    let private visitDependencies fVisit targetName =
        let visit fGetDependencies fVisit targetName =
            let visited = new HashSet<_>()
            let ordered = new List<_>()
            let rec visitDependenciesAux level (depType,targetName) =
                let target = Get targetName
                let isVisited = visited.Contains targetName
                visited.Add targetName |> ignore
                fVisit (target, depType, level, isVisited)
                (fGetDependencies target) |> Seq.iter (visitDependenciesAux (level + 1))
                if not isVisited then ordered.Add targetName
            visitDependenciesAux 0 (DependencyType.Hard, targetName)
            visited, ordered

        // First pass is to accumulate targets in (hard) dependency graph
        let visited, _ = visit (fun t -> t.Dependencies |> withDependencyType DependencyType.Hard) ignore targetName

        let getAllDependencies (t: Target) =
             (t.Dependencies |> withDependencyType DependencyType.Hard) @
             // Note that we only include the soft dependency if it is present in the set of targets that were
             // visited.
             (t.SoftDependencies |> List.filter visited.Contains |> withDependencyType DependencyType.Soft)

        // Now make second pass, adding in soft depencencies if appropriate
        visit getAllDependencies fVisit targetName

    /// <summary>Writes a dependency graph.</summary>
    /// <param name="verbose">Whether to print verbose output or not.</param>
    /// <param name="target">The target for which the dependencies should be printed.</param>
    let PrintDependencyGraph verbose target =
        match getTargetDict().TryGetValue (target) with
        | false,_ -> ListAvailable()
        | true,target ->
            Trace.logfn "%sDependencyGraph for Target %s:" (if verbose then String.Empty else "Shortened ") target.Name

            let logDependency ((t: Target), depType, level, isVisited) =
                if verbose ||  not isVisited then
                    let indent = (String(' ', level * 3))
                    if depType = DependencyType.Soft then
                        Trace.log <| sprintf "%s<=? %s" indent t.Name
                    else
                        Trace.log <| sprintf "%s<== %s" indent t.Name

            let _, ordered = visitDependencies logDependency target.Name

            Trace.log ""
            Trace.log "The resulting target order is:"
            Seq.iter (Trace.logfn " - %s") ordered

    /// <summary>Writes a build time report.</summary>
    /// <param name="total">The total runtime.</param>
    let internal WriteTaskTimeSummary total context =
        Trace.traceHeader "Build Time Report"
        let executedTargets = context.PreviousTargets        
        if executedTargets.Length > 0 then
            let width =
                executedTargets
                  |> Seq.map (fun (tres) -> tres.Target.Name.Length)
                  |> Seq.max
                  |> max 8

            let alignedString (name:string) (duration) extra =
                let durString = sprintf "%O" duration
                if (String.IsNullOrEmpty extra) then
                    sprintf "%s   %s" (name.PadRight width) durString
                else sprintf "%s   %s   (%s)" (name.PadRight width) (durString.PadRight "00:00:00.0000824".Length) extra
            let aligned (name:string) duration extra = alignedString name duration extra |> Trace.trace
            let alignedWarn (name:string) duration extra = alignedString name duration extra |> Trace.traceFAKE "%s"
            let alignedError (name:string) duration extra = alignedString name duration extra |> Trace.traceError

            aligned "Target" "Duration" null
            aligned "------" "--------" null
            executedTargets
              |> Seq.iter (fun (tres) ->
                    let name = tres.Target.Name
                    let time = tres.Time
                    match tres.Error with
                    | None when tres.WasSkipped -> alignedWarn name time "skipped" // Yellow
                    | None -> aligned name time null
                    | Some e -> alignedError name time e.Message)

            aligned "Total:" total null
            if not context.HasError then aligned "Status:" "Ok" null
            else
                alignedError "Status:" "Failure" null
        else
            Trace.traceError "No target was successfully completed"

        Trace.traceLine()

    /// [omit]
    let internal isListMode = Environment.hasEnvironVar "list"

    // Instead of the target can be used the list dependencies graph parameter.
    let internal doesTargetMeanListTargets target = target = "--listTargets"  || target = "-lt"


    /// Determines a parallel build order for the given set of targets
    let internal determineBuildOrder (target : string) =

        let t = Get target

        let targetLevels = new Dictionary<_,_>()
        let addTargetLevel ((target: Target), _, level, _ ) =
            match targetLevels.TryGetValue target.Name with
            | true, mapLevel when mapLevel >= level -> ()
            | _ -> targetLevels.[target.Name] <- level

        let visited, ordered = visitDependencies addTargetLevel target

        // the results are grouped by their level, sorted descending (by level) and
        // finally grouped together in a list<TargetTemplate<unit>[]>
        let result =
            targetLevels
            |> Seq.map (fun pair -> pair.Key, pair.Value)
            |> Seq.groupBy snd
            |> Seq.sortBy (fun (l,_) -> -l)
            |> Seq.map (snd >> Seq.map fst >> Seq.distinct >> Seq.map Get >> Seq.toArray)
            |> Seq.toList

        // Note that this build order cannot be considered "optimal"
        // since it may introduce order where actually no dependencies
        // exist. However it yields a "good" execution order in practice.
        result

    /// Runs a single target without its dependencies... only when no error has been detected yet.
    let internal runSingleTarget (target : Target) (context:TargetContext) =
        if not context.HasError then
            use t = Trace.traceTarget target.Name (match target.Description with Some d -> d | _ -> "NoDescription") (dependencyString target)
            runSimpleContext target context
        else
            { context with PreviousTargets = context.PreviousTargets @ [{ Error = None; Time = TimeSpan.Zero; Target = target; WasSkipped = true }] }


    /// Runs the given array of targets in parallel using count tasks
    let internal runTargetsParallel (count : int) (targets : Target[]) context =
        let known =
            context.PreviousTargets
            |> Seq.map (fun tres -> tres.Target.Name, tres)
            |> dict
        let filterKnown targets =
            targets
            |> List.filter (fun tres -> not (known.ContainsKey tres.Target.Name))
        targets
        |> Array.map (fun t -> async { return runSingleTarget t context })
        |> Async.Parallel
        |> Async.RunSynchronously
        |> Seq.reduce (fun ctx1 ctx2 ->
            { PreviousTargets = 
                context.PreviousTargets @ filterKnown ctx1.PreviousTargets @ filterKnown ctx2.PreviousTargets })

    /// Runs a target and its dependencies.
    let internal run targetName =
        if doesTargetMeanListTargets targetName then ListAvailable(); TargetContext.Empty else
        match getLastDescription() with
        | Some d -> failwithf "You set a task description (%A) but didn't specify a task. Make sure to set the Description above the Target." d
        | None -> ()

        let rec runTargets (targets: Target array) (context:TargetContext) =
            let lastTarget = targets |> Array.last
            //if not context.HasErrors && context.TryFindPrevious(lastTarget.Name).IsNone then
            if Environment.hasEnvironVar "single-target" then
                Trace.traceImportant "Single target mode ==> Skipping dependencies."
                runSingleTarget lastTarget context
            else
               targets |> Array.fold (fun context target -> runSingleTarget target context) context
           // else                

        printfn "run %s" targetName
        let watch = new System.Diagnostics.Stopwatch()
        watch.Start()
        let context = TargetContext.Empty
        let context =
            Trace.tracefn "Building project with version: %s" BuildServer.buildVersion
            let parallelJobs = Environment.environVarOrDefault "parallel-jobs" "1" |> int

            // Figure out the order in in which targets can be run, and which can be run in parallel.
            if parallelJobs > 1 then
                Trace.tracefn "Running parallel build with %d workers" parallelJobs

                // determine a parallel build order
                let order = determineBuildOrder targetName

                // run every level in parallel
                order
                    |> Seq.fold (fun context par -> runTargetsParallel parallelJobs par context) context
            else
                // single threaded build.
                PrintDependencyGraph false targetName

                // Note: we could use the ordering resulting from flattening the result of determineBuildOrder
                // for a single threaded build (thereby centralizing the algorithm for build order), but that
                // ordering is inconsistent with earlier versions of FAKE (and PrintDependencyGraph).
                let _, ordered = visitDependencies ignore targetName

                runTargets (ordered |> Seq.map Get |> Seq.toArray) context

        let context =        
            if context.HasError then
                runBuildFailureTargets context
            else context            
        let context = runFinalTargets context
        WriteTaskTimeSummary watch.Elapsed context
        
        if context.HasError then
            let errorTargets =
                context.PreviousTargets
                |> List.choose (fun tres ->
                    match tres.Error with
                    | Some er -> Some (er, tres.Target)
                    | None -> None)
            let targets = errorTargets |> Seq.map (fun (er, target) -> target.Name) |> Seq.distinct
            let targetStr = String.Join(", ", targets)
            let errorMsg =
                if errorTargets.Length = 1 then
                    sprintf "Target '%s' failed." targetStr
                else
                    sprintf "Targets '%s' failed." targetStr          
            let inner = AggregateException(AggregateException().Message, errorTargets |> Seq.map fst)
            BuildFailedException(context, errorMsg, inner)                
            |> raise

        context

    /// Creates a target in case of build failure (not activated).
    let CreateBuildFailure name body =
        Create name body
        getBuildFailureTargets().Add(name,false)

    /// Activates the build failure target.
    let ActivateBuildFailure name =
        let t = Get name // test if target is defined
        getBuildFailureTargets().[name] <- true

    /// Creates a final target (not activated).
    let CreateFinal name body =
        Create name body
        getFinalTargets().Add(name,false)

    /// Activates the final target.
    let ActivateFinal name =
        let t = Get name // test if target is defined
        getFinalTargets().[name] <- true

    /// Runs a target and its dependencies, used for testing - usually not called in scripts.
    let RunAndGetContext targetName = run targetName

    /// Runs a target and its dependencies
    let Run targetName = run targetName |> ignore

    /// Runs the target given by the target parameter or the given default target
    let RunOrDefault defaultTarget = Environment.environVarOrDefault "target" defaultTarget |> Run

    /// Runs the target given by the target parameter or lists the available targets
    let RunOrList() =
        if Environment.hasEnvironVar "target" then Environment.environVar "target" |> Run
        else ListAvailable()