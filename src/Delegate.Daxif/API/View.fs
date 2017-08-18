﻿namespace DG.Daxif

open DG.Daxif.Common
open DG.Daxif.Modules.View
open Utility
open DG.Daxif.Modules.View.TypeDeclarations
open Microsoft.Xrm.Sdk.Query

type View private () =

  /// <summary>Generates the files needed for View Extender</summary>
  /// <param name="env">Environment the action should be performed against.</param>
  static member Generate(env: Environment, daxifRoot: string, ?entities: string[], ?solutions: string[], ?logLevel: LogLevel) =
    let usr, pwd, dmn = env.getCreds()
    let logLevel = logLevel ?| LogLevel.Verbose
    Main.generate env.url env.apToUse usr pwd dmn daxifRoot entities solutions logLevel
  

  static member UpdateView (env: Environment) (view: TypeDeclarations.View) =
    let usr, pwd, dmn = env.getCreds()
    Main.updateView env.url env.apToUse usr pwd dmn view

  static member UpdateViewList (env: Environment) (views: TypeDeclarations.View list) = 
    let usr, pwd, dmn = env.getCreds()
    Main.updateViewList env.url env.apToUse usr pwd dmn views

  static member Parse (env: Environment) (guid: System.Guid) =
   let usr, pwd, dmn = env.getCreds()
   Main.parse env.url env.apToUse usr pwd dmn guid

  static member AddColumn (column : EntityAttribute<_,_>, width) index (view : TypeDeclarations.View) = 
    Main.addColumn column width index view
  static member AddColumnFirst (column : EntityAttribute<_,_>) width (view : TypeDeclarations.View) =
    Main.addColumnFirst column width view
  static member AddColumnLast (column : EntityAttribute<_,_>) width (view : TypeDeclarations.View) =
    Main.addColumnLast column width view
  static member RemoveColumn (column : EntityAttribute<_,_>) (view : TypeDeclarations.View) =
    Main.removeColumn column view
  static member AddOrdering (column : EntityAttribute<_,_>) (ordering : OrderType) (view : TypeDeclarations.View) = 
    Main.addOrdering column ordering view
  static member RemoveOrdering (column : EntityAttribute<_,_>) (view : TypeDeclarations.View) = 
    Main.removeOrdering column view
  static member ChangeWidth (column : EntityAttribute<_,_>) width (view : TypeDeclarations.View) = 
    Main.changeWidth column width view
  static member SetFilter (filter : FilterExpression) (view : TypeDeclarations.View) = 
    Main.setFilter filter view
  static member AndFilters (filter : FilterExpression) (view : TypeDeclarations.View) = 
    Main.andFilters filter view
  static member OrFilters (filter : FilterExpression) (view : TypeDeclarations.View) = 
    Main.orFilters filter view
  static member RemoveFilter (view : TypeDeclarations.View) = 
    Main.removeFilter view
  static member AddLink (rel : EntityRelationship) (columns : IEntityAttribute list) (columnWidths : int list) (indexes : int list) (view : TypeDeclarations.View) =
    Main.addLink rel columns columnWidths indexes view
  static member AddLinkFirst (rel : EntityRelationship) (columns : IEntityAttribute list) (columnWidths : int list) (view : TypeDeclarations.View) =
    Main.addLinkFirst rel columns columnWidths view
  static member AddLinkLast (rel : EntityRelationship) (columns : IEntityAttribute list) (columnWidths : int list) (view : TypeDeclarations.View) =
    Main.addLinkLast rel columns columnWidths view
  static member RemoveLink (rel: EntityRelationship) (view : TypeDeclarations.View) =
    Main.removeLink rel view
  static member Extend guid (view : TypeDeclarations.View) =
    Main.extend guid view
  static member initFilter (operator: LogicalOperator) = 
    Main.initFilter operator
  static member AddCondition (attributeEntity : EntityAttribute<'a,'b>) (operator : 'b) (arg : 'a) (filter : FilterExpression) =
    Main.addCondition attributeEntity operator arg filter
  static member AddCondition2 (attributeEntity : EntityAttribute<'a,'b>) (operator : 'b) (arg1 : 'a) (arg2 : 'a) (filter : FilterExpression) =
    Main.addCondition2 attributeEntity operator arg1 arg2 filter
  static member AddConditionMany (attributeEntity : EntityAttribute<'a,'b>) (operator : 'b) (arg : 'a list) (filter : FilterExpression) =
    Main.addConditionMany attributeEntity operator arg 
  static member AddFilter (toAdd : FilterExpression) (filter : FilterExpression) = 
    Main.addFilter toAdd filter
