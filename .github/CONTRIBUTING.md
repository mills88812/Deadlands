![A depiction of the 'RAIN WORLD' title screen, featuring Nomad.](../mod/decals/Notocorda_TitleScreen.png)

## Introduction
The following is a general guide on how to help out for an open-source C# project like this, for those unfamiliar with typical etiquette and expectations. **These are not rules**, just some common wisdom that will make it easier for you to get your code merged and this mod further on its way to being complete! ❤️

## Basics
- **Don't merge your own PR.** Unless it's important or beyond all reasonable doubt that you did it perfectly, wait for someone else to merge it.
- **Try to ask for help when you don't understand something.** A lot of the info about the Rain World engine is not easily available online, but you may get a good answer if you just ask. Nobody is expected to know everything and nobody deserves wasting 10 hours on figuring something out all over again by themselves. *Don't be a Pebbles.*

## Code Style
There's no set style, but here's some recommendations:

- Use file-wide namespaces instead of namespace blocks. This means writing `namespace Deadlands;` instead of `namespace Deadlands { /* ... all the code ... */ }`.
- Never use `unsafe`.
- Be communicative! Code you write is, in part, a conversation you are having with the rest of us. Explain yourself, use clear variable names, and keep it simple.
