# Zork Plugin

## Introduction
This allows to play the game "Zork" - a (very old) text based adventure.

The user navigates through an area using commands like "north" etc. and has some options to perform actions.

There are many video available that show the game.

Idea is that a blind person could play this game just using natural language.

## Implementation
I used the port described here https://www.dshifflet.com/blog/20180325.html, code available here: https://github.com/dshifflet/WasmZork
to get the so called zMachine running.
It is no problem to port the Zork and ZMachine projects to .Net 8.0
and to reference the dlls in this project.

Since the ported project is outside this solution the solution does not compile without doing the port and correcting the references.

## Limitations
It does seem there are bugs in the port.
E.g. when I come to the "canyon" there is an Exception and the game stops.

And it is currently unclear to me which commands are actually available.

Together with the not flawlessly working speach recognition it is not that much fun to play it yet. 