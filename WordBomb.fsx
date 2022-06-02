
module Trie =

    type Trie =
        {
            endOfWord : bool
            children : Map<char, Trie>
        }

    let makeTrie () =
        {
            endOfWord = false
            children = Map([])
        }

    let trieChild c trie =
        trie.children.TryFind c
        |> Option.defaultValue (makeTrie())

    let addWord (word: string) trie =

        let rec inner word trie =
            match word with
            | [] ->
                makeTrie()
            | [c] ->
                {
                    trie with children =
                                    trie.children.Add(c, 
                                                      { makeTrie() with endOfWord = true })
                }
            | h::t ->
                { trie with children = trie.children.Add(h, inner t (trieChild h trie)) }

        {
            endOfWord = false
            children = trie.children.Add(
                word.[0],
                (inner (word |> List.ofSeq |> List.tail) (trieChild word.[0] trie)))
        }
    
    let findPrefix (prefix: string) (trie: Trie) =

        let rec inner prefix trie =
            match prefix with
            | [] ->
                Some trie
            | [c] ->
                trie.children.TryFind c
            | h::t ->
                trie.children.TryFind h 
                |> Option.bind (fun trie' -> inner t trie')

        inner (prefix |> List.ofSeq) trie

    let hasPrefix prefix trie =
        match findPrefix prefix trie with
        | Some trie ->
            true
        | None ->
            false

    let hasWord word trie =
        match findPrefix word trie with
        | Some trie ->
            trie.endOfWord
        | None ->
            false

    let rec allStrings (prefix: string) (trie: Trie) =
        [
            if trie.endOfWord then
                prefix
            for c in trie.children.Keys do
                for s in (allStrings (prefix + (string c)) trie.children.[c]) do
                    s
        ]

    let wordsWithPrefix (prefix: string) (trie: Trie) =
        match findPrefix prefix trie with
        | Some trie' ->
            allStrings prefix trie'
        | None ->
            []

module WordBomb =

    open Trie

    type GameState =
        {
            continueCalled : bool
            prefix : string
        }

    type TurnResult =
    | NotAWord
    | Bombed
    | Challenge
    | Proceed of GameState

    let takeTurn numPlayers (gameState: GameState) trie =
        if hasPrefix gameState.prefix trie then
            if (hasWord gameState.prefix trie) && (not gameState.continueCalled) then
                Bombed
            elif (not (hasWord gameState.prefix trie)) && gameState.continueCalled then
                NotAWord
            else
                let matchingSuffixes =
                    trie
                    |> wordsWithPrefix gameState.prefix
                    |> List.map (fun (s:string) -> s.Substring(String.length gameState.prefix))
                let sayContinue =
                    matchingSuffixes
                    |> List.exists (fun (s:string) -> s.Length = 1)
                let chosenSuffix =
                    matchingSuffixes
                    |> List.filter (
                        fun (s:string) ->
                            (s.Length = 1) || ((s.Length - 1) % numPlayers <> 0)
                        )
                    |> List.sortByDescending String.length
                    |> List.head
                Proceed {
                    continueCalled = sayContinue
                    prefix = (gameState.prefix + (string chosenSuffix.[0]))
                    }
        else
            Challenge

module TrieTest =

    open Trie

    let wordList =
        [
            "aardvark"
            "aardwolf"
            "aaron"
            "aback"
            "abacus"
            "abaft"
            "abashed"
            "woo"
            "wood"
            "woodbine"
            "woodcock"
            "woodcocks"
            "woodcut"
            "woodcuts"
            "woodcutter"
            "woodcutters"
            "wooded"
            "wooden"
            "abate"
            "abbeys"
            "abbot"
            "abbots"
            "abbreviate"
        ]

    System.IO.File.ReadLines(".\wordlist.txt")
    |> Seq.fold (fun trie w -> trie |> Trie.addWord w) (makeTrie())
    |> Trie.wordsWithPrefix "cad"

//------------------------------------------------------------------------------

open System
open Trie
open WordBomb

let wordListFile = fsi.CommandLineArgs.[1]
let numPlayers = int fsi.CommandLineArgs.[2]
let prefix = fsi.CommandLineArgs.[3]
let continueCalled = Convert.ToBoolean fsi.CommandLineArgs.[4]

let wordsToTrie trie =
    trie
    |> Seq.filter (fun w -> not (String.IsNullOrWhiteSpace w))
    |> Seq.fold (fun trie w -> trie |> Trie.addWord w) (makeTrie())

System.IO.File.ReadLines wordListFile 
|> wordsToTrie
|> WordBomb.takeTurn numPlayers { continueCalled = continueCalled; prefix = prefix }
|> printfn "%A"