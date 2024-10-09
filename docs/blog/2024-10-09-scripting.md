---
slug: scripting
title: C# Scripting
authors: [admer456]
tags: [idea]
---

I have been messing around with Roslyn's C# API lately, and I've found it to be quite useful for my vision for Elegy's scripting system.

For years I've been observing a few C# scripting solutions like *CS-Script* and recently *Westwind.Scripting*, however those did not quite fit the bill.

<!-- truncate -->

## Introduction

In essence, from a gameplay programmer's POV, Elegy is structured like so:
* Framework & launcher - the core/platform stuff, you would never touch this unless you're adding console support.
* Engine - the engine modules, asset system, console system etc. You would also likely never touch this.
* Game SDK - this hosts all the game systems, the client-server model, and other game things.

Here I propose a new layer at the end of this all, one about scripting. You can view it as an extension of the game SDK. Effectively, the game SDK would host systems and define a few core entity components, and scripts would provide data, behaviours and potentially extra entity components too.

Modern engines view scripts as more of a development-time thing. While working in the engine editor, you have access to the code files of your project, and once you're done, you bake it all into an executable. This, to me, isn't true scripting.

Real, true scripting is when you have these "naked" code files distributed with the game, and modders can change it too. And that's precisely what I'm looking to achieve here. Power to the players & modders!

In essence, the game's systems could utilise several categories of scripts:
* UI scripts - the entire main menu and in-game HUD may be implemented here
* Level a.k.a. mission scripts - in certain scenarios, it may be too complicated to implement a feature via logic & trigger entities, so it's best to do it with scripts instead
* NPC behaviour scripts
* Weapon behaviour scripts

This is just the surface of it. This can be extended way beyond just these four. Think: gamemodes, player controllers, custom entity components etc. This exact stuff is up to the game SDK to design a proper API for all this, and use the scripting system to implement it. It is not the responsibility of the scripting system itself, but it *is* its responsibility to provide a flexible API to be able to perform all those things.

### Wait, C# scripting?

Yes! If you're building a C# engine, I see literally *zero* need to use another language for scripting. Luau, Python, TypeScript (God forbid) or whatever, believe me, I do not see a reason for it. At least, not for Elegy.

I found a way to get the best of both worlds. The openness, or well, "nakedness" of text file scripts, and their ease of iteration, can be combined with the powerful features & level of performance C# offers. And it's all doable with `Microsoft.CodeAnalysis` i.e. the .NET Roslyn APIs.

And oh, let's not forget: proper IDE support.

In essence, you would have, for example, a `scripts` folder with a `.csproj` file inside, as well as one or more `.cs` files. This gets dynamically compiled by the scripting system and reloaded by the game. Now, I know at this point it sounds like a traditional module setup, but hear me out: these would be dynamically compiled and the code would still be distributed with the game.

Read more below.

## Looking for the Right(tm) thing

As far as 4 years ago, if I remember correctly, I saw the [CS-Script library](https://github.com/oleg-shilo/cs-script). At the time, I didn't know nearly as much C# as I do now, I was still in that phase when I was looking to build a C++ engine with rich C# scripting. Over the years though, I've come to realise that this library doesn't really do what I want!

CS-Script is designed to parse some C# code, and let you call methods from it or instantiate classes. This is fine and all, but it's not what I'm looking for. Additionally, its Roslyn backend doesn't support namespace declarations. What?

So yeah, this library ain't for me. Plus, it *did* fool me for a little bit. I thought Roslyn *itself* was limited in that regard! It sure confused me, but a few years ago when I looked at it, not knowing better, I thought this was not the way to go.

Eventually, I found [this awesome article](https://weblog.west-wind.com/posts/2022/Jun/07/Runtime-CSharp-Code-Compilation-Revisited-for-Roslyn), half of which is about utilising Roslyn APIs, the other half about the design of the [Westwind.Scripting](https://github.com/RickStrahl/Westwind.Scripting) library. Rick Strahl, thank you so much for this absolute, priceless gem of a read.

Westwind.Scripting itself is still not the library for my needs. Fundamentally, it's designed to parse and execute individual code files, or code snippets. I need a full-on project compiler. Since such a thing apparently doesn't exist, I decided to create one myself.

## Elegy.Scripting

So, I propose a new engine module, to accompany the ECS and RenderBackend modules. It is not an engine subsystem, so it does not have an init-shutdown design.

Fundamentally it would have 3 concepts:
* A little Roslyn manager, simply to warm it up and shut it down.
* C# project document model, with support for hot-reloading.
	* A.t.m. it appears that `Microsoft.CodeAnalysis.CSharp` does not provide any parsing for C# projects. Diddly darn!
	* Can compile .NET 8 - C# 12 code.
	* Can link easily against the game DLL to gain access to components, systems etc.
* Security-related components, e.g. whitelists and blacklists for references, namespace includes and so on.

Should research further:
* Debugging support - seems pretty likely.
* Codegen/analyser support - seems pretty likely, and would be valuable for custom components.
* Incremental compilation - would save a little bit of time.

Limitations:
* No NuGet packages, no project references, only assembly references.
* No super duper fancy stuff.

This subproject is super early in development at the moment, so I don't know if improvements like incremental compilation are doable at this moment. Hot-reload for sure is. I'll make sure to follow up in a part 2 of this blog.

I'm cutting this one a bit short today, but stay tuned!
