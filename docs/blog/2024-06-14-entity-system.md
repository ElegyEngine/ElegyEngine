---
slug: entity-system
title: Hybrid entity system
authors: [admer456]
tags: [idea]
---

# Hybrid entity system

Elegy's entity system could've directly copied Half-Life's, having a class hierarchy and a base entity class that supports a lot of stuff out of the box. However, this path has a few issues:
* cannot mix'n'match traits (e.g. door + breakable)
* is cache-unfriendly
* potential of bloat by dozens of unused member variables

So, I've decided to research a bit and it seems with some clever design and an ECS library, we can reap the benefits of both approaches.

## Entity system requirements

Fundamentally, entities can undergo the following interactions:
* Spawned:
	* From level data
	* Procedurally
* Saved/loaded
* Sent/received across the network
* Events:
	* Used by the player
	* Interacted with by other entities
* Updates:
	* Simulated on the server
	* Interpolated/predicted on the client

Entities can have various relationships:
* Trigger-target
* Parent
* Custom relationships defined by different kinds of entities

So let's see how to satisfy these.

## Overview - concepts

The concepts we're working with are:
* Entities - hosts to components, they generally do nothing
* Components - composable entity traits 
* Systems - events and functionality for components
	* In code, these can be part of component structs as static methods quite simply

### Spawning

The level data would look like this:
```json
{
	"classname" "func_door_breakable"
	"origin" "250 0 64"
	"Door.StartAngle" "0"
	"Door.Attrib1" "25"
	"Door.Attrib2" "40"
	"Door.Attrib3" "50"
	"Breakable.Health" "100"
	"Breakable.Material" "glass"
	"Breakable.Attrib1" "2"
	"Breakable.Attrib2" "100"
}
```

One possible approach is to use code generation to generate a parsing method:
```cs
switch ( key )
{
	case "Door.StartAngle": entity.GetComponent<Door>().StartAngle = ParseFloat( value );
	...
	case "Breakable.Health": entity.GetComponent<Breakable>().Health = ParseFloat( value );
	...
}
```

It beats runtime reflection-based loading for sure.

Components may sometimes need to initialise some stuff once they spawn, or perform checks or other things. For this, a `Spawn` method can be introduced:
```cs
[MapComponent]
public struct Door
{
	public static void OnSpawn()
	{
		// Query all Door components, perform angle corrections/adjustments
	}
}
```

*TODO: write about procedural entity creation and components requiring other components*

### Saving/loading, sending/receiving, transitioning

The end user would simply mark fields with attributes:
```cs
public struct Door
{
	[Saved]
	[Networked]
	public float DoorAngleMin { get; set; } = 0.0f;

	[Saved]
	[Networked]
	public float DoorAngleMax { get; set; } = 0.0f;

	[Saved]
	[Networked]
	public float DoorAngle { get; set; } = 0.0f;
}
```

Similarly to the idea behind spawning, code could be generated to network & save this.

If manual de/serialisation is needed, it is possible to expose it to the system:
```cs
public struct Door
{
	public static void OnGameSave( ByteBuffer buffer, in Door self )
	{
		buffer.WriteFloat( self.DoorAngle );
		...
	}

	public static Door OnGameLoad( ByteBuffer buffer )
	{
		Door result = new();
		result.DoorAngle = buffer.ReadFloat();
		...
		return result;
	}
}
```

### Events

Events are probably the most complex here. In the map editor, you can give entities names and set up links between entities, e.g. a button could open a door:
```json
"!Button.OnPress" "room1_dr.Door.Open"
...
"!Trigger.OnTouch" "room2_dr.Door.Open 2.0"
```
(`!` is an event hint, `2.0` denotes a time delay)

In essence, you'd need to find an entity with the name `room1_dr` or `room2_dr` for example, get its `Door` component and call the `Open` event. This is an example of the trigger-target relationship.

Entity names and events could be encapsulated by a component in and of itself: `MapEntity` for example.

```cs
// There are plenty of opportunities to optimise this, however, it's 
// worth nothing that these events don't get called every frame.
// They are occasional at best, if we can trust level designers...
public struct EntityEvent
{
	// Button.OnPress
	public string Input { get; set; } = string.Empty;
	// room1_dr
	public string TargetName { get; set; } = string.Empty;
	// Door
	public string ComponentName { get; set; } = string.Empty;
	// Open
	public string ComponentEvent { get; set; } = string.Empty;

	public string Delay { get; set; } = 0.0f;
}

public struct MapEntity
{
	public string Name { get; set; } = string.Empty;

	public List<EntityEvent> Events { get; set; } = new();
}
```

Each entity spawned from level data will have a `Name` and potentially populated `Events`. So it makes sense to perform queries on them.

Populating `Events` should be automatic:
```cs
[MapComponent]
public struct Door
{
	[MapEvent]
	public static void OnOpen( Entity self, Entity activator, Entity caller )
	{
		self.GetComponent<Door>().State = DoorState.Opening;
		self.GetComponent<Audio>().PlayNew( "sounds/dooropen.wav" );
	}
}
```

`caller` is the entity which called the event, e.g. a button calling the door.  
`activator` is the entity which initially started the chain, e.g. a player using the button.

### Updates

This one is pretty simple. Systems can have an `OnUpdate` method for example:
```cs
public struct Door
{
	public static void Update( float deltaTime )
	{
		// Query all Door components and update their angles and transforms
	}
}
```

*TODO: write about entity parent relationship*
