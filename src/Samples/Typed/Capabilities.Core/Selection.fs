module Elmish.WPF.Samples.Capabilities.Selection

open Elmish.WPF

type Tree<'a> = { Data: 'a; Children: Tree<'a> list }

module Tree =
    let create a ma = { Data = a; Children = ma }
    let createLeaf a = create a []

    module Data =
        let get m = m.Data

    module Children =
        let get m = m.Children

type Selection =
    { SelectedIndex: int option
      SelectedIndexData: string list
      SelectedValue: string option
      SelectedValueData: Tree<string> list }

type SelectionMsg =
    | SetSelectedIndex of int option
    | SetSelectedValue of string option

module Selection =
    module SelectedIndex =
        let get m = m.SelectedIndex
        let set v m = { m with SelectedIndex = v }

    module SelectedIndexData =
        let get m = m.SelectedIndexData

    module SelectedValue =
        let get m = m.SelectedValue
        let set v m = { m with SelectedValue = v }

    module SelectedValueData =
        let get m = m.SelectedValueData

    let init =
        { SelectedIndex = None
          SelectedIndexData = [ "A"; "B" ]
          SelectedValue = None
          SelectedValueData =
            [ Tree.create "A" [ Tree.createLeaf "Aa"; Tree.createLeaf "Ab" ]
              Tree.create "B" [ Tree.createLeaf "Ba"; Tree.createLeaf "Bb" ] ] }

    let update =
        function
        | SetSelectedIndex x -> x |> SelectedIndex.set
        | SetSelectedValue x -> x |> SelectedValue.set

[<AllowNullLiteral>]
type TreeViewModel(args) =
    inherit ViewModelBase<Tree<string>, unit>(args)

    let createTreeVm (args: ViewModelArgs<Tree<string>, unit>) = TreeViewModel(args)

    member _.Data =
        base.Get () (Binding.OneWayT.id >> Binding.mapModel Tree.Data.get)

    member _.SelectedValueChildren =
        base.Get
            ()
            (Binding.SubModelSeqKeyedT.id createTreeVm (fun t -> t.Data)
             >> Binding.mapModel (Tree.Children.get))

[<AllowNullLiteral>]
type SelectionViewModel(args) =
    inherit ViewModelBase<Selection, SelectionMsg>(args)

    let createTreeVm (args: ViewModelArgs<Tree<string>, unit>) = TreeViewModel(args)

    member _.SelectedIndex =
        base.Get
            ()
            (Binding.SelectedIndexT.id
             >> Binding.mapModel Selection.SelectedIndex.get
             >> Binding.mapMsg SetSelectedIndex)

    member _.DeselectIndex =
        base.Get
            ()
            (Binding.CmdT.setIf
             >> Binding.mapModel (Selection.SelectedIndex.get >> Option.map (fun _ -> SetSelectedIndex None)))

    member _.SelectedIndexData =
        base.Get () (Binding.OneWayT.id >> Binding.mapModel Selection.SelectedIndexData.get)

    member _.SelectedValue =
        base.Get
            ()
            (Binding.TwoWayOptT.id
             >> Binding.mapModel Selection.SelectedValue.get
             >> Binding.mapMsg SetSelectedValue)

    member _.SelectedValueData =
        base.Get
            ()
            (Binding.SubModelSeqKeyedT.id createTreeVm (fun t -> t.Data)
             >> Binding.mapModel Selection.SelectedValueData.get)