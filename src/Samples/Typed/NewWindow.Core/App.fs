module Elmish.WPF.Samples.NewWindow.AppModule

open System.Windows
open Elmish.WPF

open Window1Module
open Window2Module

type App =
    { Window1: WindowState<string>
      Window2: Window2 option }

type AppMsg =
    | Window1Show
    | Window1Hide
    | Window1Close
    | Window1SetInput of string
    | Window2Show
    | Window2Close
    | Window2Msg of Window2Msg

module App =
    module Window1 =
        let get m = m.Window1
        let set v m = { m with Window1 = v }
        let map = map get set

    module Window2 =
        let get m = m.Window2
        let set v m = { m with Window2 = v }
        let map = map get set

        let mapOutMsg =
            function
            | Window2OutMsg.Close -> Window2Close

        let mapInOutMsg = InOut.cata Window2Msg mapOutMsg

    let init =
        { Window1 = WindowState.Closed
          Window2 = None }

    let update =
        function
        | Window1Show -> "" |> WindowState.toVisible |> Window1.map
        | Window1Hide -> "" |> WindowState.toHidden |> Window1.map
        | Window1Close -> WindowState.Closed |> Window1.set
        | Window1SetInput s -> s |> WindowState.set |> Window1.map
        | Window2Show -> Window2.init |> Some |> Window2.set
        | Window2Close -> None |> Window2.set
        | Window2Msg msg -> msg |> Window2.update |> Option.map |> Window2.map

[<AllowNullLiteral>]
type AppViewModel(args) =
    inherit ViewModelBase<App, AppMsg>(args)

    let window1Vm (args: ViewModelArgs<string, string>) = Window1ViewModel(args)
    let window2Vm (args: ViewModelArgs<Window2, InOut<Window2Msg, Window2OutMsg>>) = Window2ViewModel(args)

    member _.Window1Show = base.Get () (Binding.CmdT.setAlways Window1Show)
    member _.Window1Hide = base.Get () (Binding.CmdT.setAlways Window1Hide)
    member _.Window1Close = base.Get () (Binding.CmdT.setAlways Window1Close)
    member _.Window2Show = base.Get () (Binding.CmdT.setAlways Window2Show)

    member _.Window1 =
        base.Get
            ()
            (Binding.SubModelWinT.id window1Vm
             >> Binding.mapModel (fun m -> m.Window1)
             >> Binding.mapMsg (fun s -> Window1SetInput s))

    member _.Window2 =
        base.Get
            ()
            (Binding.SubModelWinT.id window2Vm
             >> Binding.mapModel (fun m -> App.Window2.get m |> WindowState.ofOption)
             >> Binding.mapMsg App.Window2.mapInOutMsg)