
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

    let makeGame () =
        {
            continueCalled = false
            prefix = ""
        }

    type TurnResult =
    | NotAWord
    | Bombed
    | Challenge
    | Proceed of GameState

    let takeTurn numPlayers trie (gameState: GameState) =
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
                let chosenSuffix =
                    matchingSuffixes
                    |> List.filter (
                        fun (s:string) ->
                            (s.Length = 1) || ((s.Length - 1) % numPlayers <> 0)
                    )
                    |> List.sortByDescending String.length
                    |> List.head
                let sayContinue =
                    matchingSuffixes
                    |> List.exists (
                        fun (s:string) ->
                            s.Length = 1
                            &&
                            chosenSuffix.StartsWith s
                            &&
                            chosenSuffix <> s
                    )
                Proceed {
                    continueCalled = sayContinue
                    prefix = (gameState.prefix + (string chosenSuffix.[0]))
                    }
        else
            Challenge

//------------------------------------------------------------------------------

open System
open Trie
open WordBomb

let wordListFile = fsi.CommandLineArgs.[1]
let numPlayers = 2

let trie =
    System.IO.File.ReadLines wordListFile 
    |> Seq.filter (fun w -> not (String.IsNullOrWhiteSpace w))
    |> Seq.fold (fun trie w -> trie |> Trie.addWord w) (makeTrie())

printfn "Enter an English letter, followed by a '.' character if you believe "
printfn "this letter results in a valid word, but can be extended further to "
printfn "a different valid word, otherwise enter a space ' ' character."
printfn "\n"

let oneRound gameState =

    match Console.ReadKey().KeyChar with

    | '?' ->

        let turnResult =
            WordBomb.takeTurn numPlayers trie gameState

        match turnResult with
        | NotAWord ->
            printfn "\nMy bad! That was not a real word, I lose."
        | Bombed ->
            printfn "\nBombed! I lose."
        | Challenge ->
            printfn "\nMy bad! There is no such word, I lose."
        | Proceed(gameState') ->
            printfn "\nHey! Below are the possible words, you lose."

            trie
            |> wordsWithPrefix gameState.prefix
            |> List.iter (printfn "%s")

        Some (true, gameState)

    | l ->

        let continueCalled =
            match Console.ReadKey().KeyChar with
            | '.' -> true
            | ' ' -> false
            | c -> failwith (sprintf "Invalid character '%c' entered" c)
        
        let prefix' = gameState.prefix + (l.ToString())

        let turnResult =
            WordBomb.takeTurn numPlayers trie { continueCalled = continueCalled ; prefix = prefix' }

        match turnResult with
        | NotAWord ->
            printfn "\nYou called continue, but %s is not a valid word." prefix'
            Some (true, gameState)
        | Bombed ->
            printfn "\nBombed! You lose."
            Some (true, gameState)
        | Challenge ->
            printfn "\nChallenge! There is no such word."
            Some (true, gameState)
        | Proceed(gameState') ->
            Console.Write (gameState'.prefix.Substring(gameState'.prefix.Length-1))
            if gameState'.continueCalled then
                Console.Write "."
            else
                Console.Write " "
            Some (false, gameState')

Seq.unfold oneRound (makeGame()) |> Seq.find id