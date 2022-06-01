
# **WordBomb**

## **How the game works**

Players take turns to build towards a word by adding characters. Player must select a character that helps extend the sequence of characters closer to a valid word. However, if a player's turn ends in a valid word, a bomb blows up and the player loses.

### **What actions can a player take in a turn?**

Given a sequence of characters that have been proposed so far, a player can either
- Challenge the previous player's proposed character, if the player does not think it helps build a valid word.
- Call out that the previous player lost because the proposed character has resulted in a valid word, which blows up a bomb.
- Propose a character that extends the sequence of characters towards a valid word. If the proposed character results in a word (eg. brace), but it can also be extended further to a different word (eg. bracelet), the player must also say "continue", otherwise they will lose if the next player points out that "brace" is a valid word.

### **What does this program do?**

This program implements one turn of a player, given
- a wordlist.
- number of players.
- the sequence of characters proposed so far.

### **How to use this program?**

dotnet fsi .\WordBomb.fsx <wordlistfile> <numplayers> <wordprefix>

Example 1

    dotnet fsi .\WordBomb.fsx .\wordlist.txt 2 bracket

will output

    Bombed

Example 2

    dotnet fsi .\WordBomb.fsx .\wordlist.txt 2 brac

will output

    Proceed (true, "brack")

Example 3

    dotnet fsi .\WordBomb.fsx .\wordlist.txt 2 foobar

will output

    Challenge

