#r "../node_modules/fable-core/Fable.Core.dll"

open System
open Fable.Core
open Fable.Import

Node.require.Invoke("core-js") |> ignore

let possibleColors = 
    [
        "AntiqueWhite"
        "Aqua"
        "Aquamarine"
        "Azure"
        "Beige"
        "Bisque"
        "BlanchedAlmond"
        "Blue"
        "BlueViolet"
        "Brown"
        "BurlyWood"
        "CadetBlue"
        "Chartreuse"
        "Chocolate"
        "Coral"
        "CornflowerBlue"
        "Cornsilk"
        "Crimson"
        "Cyan"
        "DarkBlue"
        "DarkCyan"
        "DarkGoldenRod"
        "DarkGray"
        "DarkGrey"
        "DarkGreen"
        "DarkKhaki"
        "DarkMagenta"
        "DarkOliveGreen"
        "DarkOrange"    
        "DarkOrchid"
        "DarkRed"
        "DarkSalmon"
        "DarkSeaGreen"
        "DarkSlateBlue"
        "DarkSlateGray"
        "DarkTurquoise"    
        "DarkViolet"
        "DeepPink"    
        "DeepSkyBlue"
        "DimGray"
        "DimGrey"
        "DodgerBlue"
        "FireBrick"
        "FloralWhite"
        "ForestGreen"
        "Fuchsia"    
        "Gainsboro"
        "GhostWhite"    
        "Gold"
        "GoldenRod"
        "Gray"
        "Grey"
        "Green"
        "GreenYellow"
        "HoneyDew"
        "HotPink"    
        "IndianRed"
        "Indigo"
        "Ivory"
        "Khaki"
        "Lavender"
        "LavenderBlush"
        "LawnGreen"
        "LemonChiffon"
        "LightBlue"
        "LightCoral"
        "LightCyan"
        "LightGoldenRodYellow"    
        "LightGray"
        "LightGrey"
        "LightGreen"
        "LightPink"
        "LightSalmon"
        "LightSeaGreen"
        "LightSkyBlue"
        "LightSlateGray"
        "LightSlateGrey"
        "LightSteelBlue"
        "LightYellow"
        "Lime"
        "LimeGreen"
        "Linen"
        "Magenta"
        "Maroon"
        "MediumAquaMarine"
        "MediumBlue"
        "MediumOrchid"
        "MediumPurple"
        "MediumSeaGreen"
        "MediumSlateBlue"
        "MediumSpringGreen"
        "MediumTurquoise"
        "MediumVioletRed"
        "MidnightBlue"
        "MintCream"
        "MistyRose"
        "Moccasin"
        "NavajoWhite"
        "Navy"
        "OldLace"
        "Olive"
        "OliveDrab"
        "Orange"
        "OrangeRed"
        "Orchid"
        "PaleGoldenRod"
        "PaleGreen"
        "PaleTurquoise"
        "PaleVioletRed"
        "PapayaWhip"
        "PeachPuff"
        "Peru"
        "Pink"
        "Plum"
        "PowderBlue"
        "Purple"
        "RebeccaPurple"
        "Red"
        "RosyBrown"
        "RoyalBlue"
        "SaddleBrown"
        "Salmon"
        "SandyBrown"
        "SeaGreen"
        "SeaShell"
        "Sienna"
        "Silver"
        "SkyBlue"
        "SlateBlue"
        "SlateGray"
        "SlateGrey"
        "Snow"
        "SpringGreen"
        "SteelBlue"
        "Tan"
        "Teal"
        "Thistle" 
    ]

module Model = 

    open System

    type GameBoard = {
        Selection : Selection
        Board : Tile list list
    }
    and Tile = {
        Row : int
        Col : int
        HiddenColor : string
        CoverColor : string
        Status : TileStatus
    }
    and TileStatus = UnMatched | AttemptingMatch | Matched
    and Selection = NoneSelected | OneSelected of Tile | TwoSelected of Tile * Tile

    module GameBoard = 
        let updateTile r c t gb = 
            let updatedRow = gb.Board.[r] |> List.toArray |> (fun arr -> arr.[c] <- t; arr) |> List.ofArray
            {
                gb with
                    Board = gb.Board |> List.toArray |> (fun arr -> arr.[r] <- updatedRow; arr) |> List.ofArray
            }

        let updateSelected s gb = { gb with Selection = s}
        
    let generateRandomModel n =
        let totalTiles = n * n
        let rnd = Random() 
        let rec randomColorsWithIndexes (possIndexes1:Set<int>) (possIndexes2:Set<int>) (possColors:Set<string>) n =
            match n with
            | 0 -> []
            | _ ->  let index1 = possIndexes1 |> Set.toList |> (fun l -> l.[rnd.Next(l.Length)])
                    let index2 = possIndexes2 |> Set.toList |> (fun l -> l.[rnd.Next(l.Length)])
                    let color = possColors |> Set.toList |> (fun l -> l.[rnd.Next(l.Length)])
                    let remainingIndexes1 = possIndexes1 |> Set.remove index1
                    let remainingIndexes2 = possIndexes2 |> Set.remove index2
                    let remainingColors = possColors |> Set.remove color
                    (color, index1, index2) :: (randomColorsWithIndexes remainingIndexes1 remainingIndexes2 remainingColors (n - 1))
        
        let tiles = 
            randomColorsWithIndexes (Set [0 .. (totalTiles / 2) - 1]) ( Set [(totalTiles / 2) .. totalTiles - 1]) (Set possibleColors) (totalTiles / 2)
            |> List.collect (fun (c,i1,i2) -> [(c,i1); (c,i2)])
            |> List.sortBy (fun (c, i) -> i)
            |> List.mapi (fun flatIndex (c, i) -> 
                ({   
                    Row = flatIndex / n
                    Col = flatIndex % n
                    HiddenColor = c
                    CoverColor = "black"
                    Status = UnMatched
                }, flatIndex)
            )
            |> List.sortBy (fun (t, flatIndex) -> flatIndex)
            |> List.map fst
            |> List.rev
            |> List.fold (fun (rows: Tile list list) tile -> 
                    match rows with
                    | [] -> [[tile]]
                    | h :: t when h.Length < n -> (tile :: h) :: t
                    | _ -> [tile] :: rows
                ) []
        
        {
            Selection = NoneSelected
            Board = tiles
        }

module View = 
    open Model
    
    let gameContainer = Browser.document.getElementById("memory-tiles-container")
    let render tileClick (gameBoard:GameBoard)  = 

        gameContainer.innerHTML <- ""
        
        let board = gameBoard.Board
        for rowIndex in 0.. board.Length - 1 do

            let rowDiv = Browser.document.createElement("div")
            rowDiv.className <- "row"
            rowDiv.style.minHeight <- "100px"

            for cIndex in 0..board.[rowIndex].Length - 1 do
                let tile = board.[rowIndex].[cIndex]
                let tileDiv = Browser.document.createElement("div")
                tileDiv.className <- "col-3"
                let bc = match tile.Status with
                         | UnMatched -> tile.CoverColor
                         | AttemptingMatch -> tile.HiddenColor
                         | Matched -> tile.HiddenColor
                tileDiv.style.backgroundColor <- bc
                tileDiv.style.border <- "white solid 4px"
                
                let eventHandler r c d = Func<Browser.MouseEvent, obj>(fun _ -> tileClick r c gameBoard :> obj)
                tileDiv.addEventListener_click(eventHandler rowIndex cIndex tileDiv)

                rowDiv.appendChild tileDiv |> ignore   
                gameContainer.appendChild rowDiv |> ignore 

module Controller = 
    open Model

    let rec tileClick tileRow tileCol (gameBoard: GameBoard) = 
        let board = gameBoard.Board
        let tile = board.[tileRow].[tileCol] 
        let lastSelection = gameBoard.Selection

        match lastSelection with
        | TwoSelected(t1, t2) when t1.Status = Matched && t2.Status = Matched -> 
            gameBoard
            |> GameBoard.updateTile tile.Row tile.Col { tile with Status = AttemptingMatch }
            |> GameBoard.updateSelected (OneSelected tile)
            |> View.render tileClick
        | TwoSelected(t1, t2) when t1.Status = Matched -> 
            gameBoard
            |> GameBoard.updateTile tile.Row tile.Col { tile with Status = AttemptingMatch }
            |> GameBoard.updateTile t2.Row t2.Col { t2 with Status = UnMatched }
            |> GameBoard.updateSelected (OneSelected tile)
            |> View.render tileClick  
        | TwoSelected(t1, t2) when t2.Status = Matched -> 
            gameBoard
            |> GameBoard.updateTile tile.Row tile.Col { tile with Status = AttemptingMatch }
            |> GameBoard.updateTile t1.Row t1.Col { t1 with Status = UnMatched }
            |> GameBoard.updateSelected (OneSelected ({ tile with Status = AttemptingMatch }))
            |> View.render tileClick  
        | TwoSelected(t1, t2) -> 
            gameBoard 
            |> GameBoard.updateTile t1.Row t1.Col { t1 with Status = UnMatched }
            |> GameBoard.updateTile t2.Row t2.Col { t2 with Status = UnMatched }
            |> GameBoard.updateTile tile.Row tile.Col { tile with Status = AttemptingMatch }
            |> GameBoard.updateSelected (OneSelected ({ tile with Status = AttemptingMatch }))
            |> View.render tileClick 
            
        | OneSelected t1 when t1.HiddenColor = tile.HiddenColor -> 
            gameBoard
            |> GameBoard.updateTile t1.Row t1.Col { t1 with Status = Matched }
            |> GameBoard.updateTile tile.Row tile.Col { tile with Status = Matched }              
            |> GameBoard.updateSelected (TwoSelected ({ t1 with Status = Matched },{ tile with Status = Matched }))
            |> View.render tileClick 
        | OneSelected t1 -> 
            gameBoard
            |> GameBoard.updateTile tile.Row tile.Col { tile with Status = AttemptingMatch }              
            |> GameBoard.updateSelected (TwoSelected (t1, { tile with Status = AttemptingMatch }))
            |> View.render tileClick 
        | NoneSelected -> 
            gameBoard
            |> GameBoard.updateTile tile.Row tile.Col { tile with Status = AttemptingMatch }              
            |> GameBoard.updateSelected (OneSelected ({ tile with Status = AttemptingMatch }))
            |> View.render tileClick 
            
    let startGame() = 
        View.render tileClick (Model.generateRandomModel 4)

Controller.startGame()