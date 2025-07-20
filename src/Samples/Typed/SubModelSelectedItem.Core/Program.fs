module Elmish.WPF.Samples.SubModelSelectedItem.Program

open System
open Serilog
open Serilog.Extensions.Logging
open Elmish
open Elmish.WPF

type Entity = { Id: int; Name: string }

type Model =
    { Entities: Entity list
      Selected: int option }

let init () =
    { Entities = [ 0..10 ] |> List.map (fun i -> { Id = i; Name = sprintf "Entity %i" i })
      Selected = Some 4 }

type Msg = Select of int option

let update msg m =
    match msg with
    | Select entityId -> { m with Selected = entityId }

[<AllowNullLiteral>]
type EntityViewModel(args) =
    inherit ViewModelBase<Model * Entity, unit>(args)

    member _.Name =
        base.Get () (Binding.OneWayT.id >> Binding.addLazy (=) >> Binding.mapModel (fun (_, e) -> e.Name))

    member _.SelectedLabel =
        base.Get
            ()
            (Binding.OneWayT.id
             >> Binding.addLazy (=)
             >> Binding.mapModel (fun (m, e) -> if m.Selected = Some e.Id then " - SELECTED" else ""))

[<AllowNullLiteral>]
type MainViewModel(args) =
    inherit ViewModelBase<Model, Msg>(args)

    let createEntityVm (args: ViewModelArgs<Model * Entity, unit>) = EntityViewModel(args)

    member _.SelectRandom =
        base.Get
            ()
            (Binding.CmdT.set (fun m -> m.Entities.Length > 0)
                (fun m ->
                    m.Entities.Item(Random().Next(m.Entities.Length)).Id
                    |> Some
                    |> Select))

    member _.Deselect = base.Get () (Binding.CmdT.setAlways (Select None))

    member _.Entities =
        base.Get
            ()
            (Binding.SubModelSeqKeyedT.id createEntityVm (fun (_, e) -> e.Id)
             >> Binding.mapModel (fun m -> m.Entities |> List.map (fun e -> (m, e))))

    member _.SelectedEntity =
        base.Get
            ()
            (Binding.SubModelSelectedItem.opt "Entities"
             >> Binding.mapModel (fun m -> m.Selected)
             >> Binding.mapMsg Select)

let main window =
    let logger =
        LoggerConfiguration()
            .MinimumLevel.Override("Elmish.WPF.Update", Events.LogEventLevel.Verbose)
            .MinimumLevel.Override("Elmish.WPF.Bindings", Events.LogEventLevel.Verbose)
            .MinimumLevel.Override("Elmish.WPF.Performance", Events.LogEventLevel.Verbose)
            .WriteTo.Console()
            .CreateLogger()

    let createVm args = MainViewModel(args)

    WpfProgram.mkSimpleT init update createVm
    |> WpfProgram.withLogger (new SerilogLoggerFactory(logger))
    |> WpfProgram.startElmishLoop window