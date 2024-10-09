---
slug: networking
title: Networking
authors: [admer456]
tags: [idea]
---

# Multiplayer

I've always envisioned multiplayer to be part of this story. Ever since I wrote the first design document for BurekTech X, and later for Elegy, I've viewed basic "multiplayer with friends" as something that should be an out-of-the-box feature. 

By this, I don't mean an MMO-scale thing or even support for an official-ish system involving a master server and whatnot, no no, I just want to provide very basic "join this IP address" multiplayer which could later be extended to support Steam's P2P thingy. Ultimately, those are the kinds of games I wanna make on this, a singleplayer + multiplayer experience, whether the latter is co-op or deathmatch or whatever gamemode.

<!-- truncate -->

# Architecture

Quite simply there are a few layers:
* Client - handles input, rendering, UI and such
* Session - handles joining a server, clientside prediction and generally simulating the clientside aspects of the game
* Server - handles game simulation and manages clients

It's comparable to Quake's model, except here I have Session as a separate thing from Client, since it has a shorter lifespan than Client. Basically, when the Client fires a UI event to join a server, that's when a new Session instance is made, and so on.

The update loop essentially goes:
```cs
// Update input, main menu etc.
mClient?.Update( delta );

// Send input snapshot to server
mSession?.SendClientState();

// Entity simulation on the server, sending data to clients etc.
mServer?.Update( delta );

// Receive entity state from server, predict & simulate clientside entities
mSession?.Update( delta );
```
Notice how they're all nullable. There are a few possible configurations where this can run:
* Client-only - connected to a server
* Server-only - headless server, no rendering going on
* Full - both client and server run on the same machine

The special sauce is in how singleplayer is handled. Quake basically sends entity packets to the local machine, treating everything strictly as a remote client-server kind of setup. In Elegy, I wanted to take a bit of a different approach, where in multiplayer, it would communicate to clients via packets, and in singleplayer, it'd directly pass everything to the local client since it's already in memory.

So that's when I introduced *the bridges.*

## Bridges

There are two kinds of bridges: server bridges (a bridge to the server, used by Session) and client bridges (a bridge to a client/session, used by Server).

There are two kinds of each bridge too: local and remote. The local server bridge would basically fetch the game state directly from the server in memory, while a remote server bridge would anticipate packets and so on.

Similarly, a local client bridge would directly send a message to the local client, whereas a remote one would send that message as a packet. Very simple interfaces they are.

# Protocol

There are a few parts to this.

## Connection protocol

When a client wants to join a server, the following chain of events may happen (prefixed with C and S for clarity of roles):
1. C: **JoinRequest**
    * "Hey server, I want to join"
2. S: **AuthRequest**
    * "Okay client, tell me who you are"
    * Optionally with a password, if the server is password-protected
3. C: **AuthResponse**
    * "This is who I am"
    * Client responds with authentication info and optionally a password
    * If this client is blocked, this will cancel the connection
4. S: **AssetInfoRequest**
    * "Okay, are all your files in order?"
    * Sends information about all used assets by the server
    * Meaning maps, models, textures etc.
5. C: **AssetInfoResponse**
    * "Yeah, they are" or "I'm missing a few"
    * Either has everything, or doesn't
    * Sends hashes of assets too, for asset purity checking
6. S: **AssetPayload** (Optional)
    * "Looks like you're missing 'models/barrel.gltf', you can download it from here"
    * or "One of your assets is not the same as mine, you are not permitted to join"
    * Can be URLs to Workshop items, or actual data for some of the assets
7. C: **SpawnRequest**
    * "Alright I've loaded everything, can you reserve a seat for me?"
    * Requests the server to concretely spawn a player entity, so that it can finish joining
8. S: **SpawnResponse**
    * "Here you go: you are entity ID 5, and here's a snapshot of the game"
    * Sends a full frame of the game state (can come in chunks) and an entity ID
    * The entity at the ID will become "possessed" by the client, and its player controller will receive input from it
9. C: **SpawnComplete**
    * "I am ready to send you input snapshots and receive gamestate data"
    * This one seals the deal. The client is fully connected and ready to start interacting

It is important to note that during any of these steps, the server can also send this packet:
* **DisconnectRequest**
    * "The administrator decided to kick you, so you're out"
    * Sends a short string to the client with a reason for the disconnection. Can be anything
    * Typically happens whenever the server shuts down, or when a client is kicked/banned etc.

## Game protocol

This one can consist of a few serverside packets:
* **GameStatePayload**
    * Contains entity state data. Not every entity is included (e.g. server-only or invisible logic entities).
    * Contains other info like "time of sending" etc.
    * Can be partial, e.g. only the first 100 entities, or the next 100, or any arbitrary amount. This mode tends to happen if there are LOTS of entities in the map, so they are gradually loaded.
* **GameEventPayload**
    * Contains game event data. This is essentially an RPC.
    * It can be utilised to spawn particles on entities and so on.

There are also a few clientside packets:
* **ClientInputPayload**
    * Contains client input, e.g. a bitmask of actions and a few vectors (mouse position and so on).
* **ClientEventPayload**
    * A client-authoritative RPC to the server.
    * It can be utilised to control certain entities that are directly controlled by the client, to ensure maximum smoothness on their end. This may be used very rarely.

Here's how the game protocol may concretely look in code, from the end user's POV:
```cs
[Component, Requires<Transform>]
public partial struct Vehicle
{
    // Called on the clientside
    [Event]
    public void ClientUpdate( Entity.ClientUpdateData data )
    {
        var rpc = data.Session.Rpc;
        int entityId = data.Self.Id;
        Transform vehicleTransform = data.Self.Get<Transform>();

        rpc.Send( entityId, Vehicle.UpdateFromClient, vehicleTransform );
    }

    // Called by the client, server listens
    [ServerRemote]
    public void UpdateFromClient( ServerRemoteData data, Transform newVehicleTransform )
    {
        data.Self.Get<Transform>().Update( newVehicleTransform );
    }
}
```

Meanwhile, a server-authoritative example:
```cs
[Component, Requires<Transform>]
public partial struct Explosive
{
    // Serverside combat-related event
    // When this entity is damaged, it immediately explodes
    [Event]
    public void OnDamage( Entity.DamageData data )
    {
        var rpc = data.Server.Rpc;
        var entityId = data.Self.Id;

        // There would be a few of these:
        // Send -> sends to an individual client
        // SendAll -> sends to all clients
        // SendUnreliable -> same as Send except it's not guaranteed to arrive
        // SendAllUnreliable -> same as SendAll except it's not guaranteed to arrive
        rpc.SendAllUnreliable( entityId, Explosive.EmitEffect );
    }

    [ClientRemote]
    public void EmitEffect( ClientRemoteData data )
    {
        var particleSystem = data.Client.ParticleWorld;
        var transform = data.Self.Get<Transform>();

        particleSystem.Emit( "Fx.Explosion1", transform.Position );
    }
}
```
