---
Title: Planet Generation
Sort_Priority: 85
---

# Planet Generation

This guide covers some aspects of generating your planet, a lot of stuff is already explained in [the celestial body schema]({{ "Celestial Body Schema"|route }}).

## Orbits

First thing you should specify about your planet is its orbit. `primaryBody` will specify what planet this body will orbit.  If you're in a new solar system and want this planet to be the center, set `centerOfSolarSystem` to `true` (keep in mind `centerOfSolarSystem` is in the `Base` module, not `Orbit`).  Next up you'll need to specify the [orbital parameters](https://en.wikipedia.org/wiki/Orbital_elements). <!-- TODO: Make External, Vim is dumb --> 

## Heightmaps

Heightmaps are a way to generate unique terrain on your planet. First you specify a maximum and minimum height, and then specify a [heightMap]({{ "Celestial Body Schema"|route }}#HeightMap_heightMap) image. The more white a section of that image is, the closer to `maxHeight` that part of the terrain will be. Finally, you specify a `textureMap` which is an image that gets applied to the terrain.

Here's an example heightmap of earth from the Real Solar System addon.

![Earth's Heightmap]({{ "images/planet_gen/earth_heightmap.webp"|static }})

```json
{
    "name": "My Cool Planet",
    "HeightMap": {
        "minHeight": 5,
        "maxHeight": 100,
        "heightMap": "planets/assets/my_cool_heightmap.png",
        "textureMap": "planets/assets/my_cool_texturemap.png"
    }
}
```

There are also tools to help generate these images for you such as [Textures For Planets](https://www.texturesforplanets.com/){ target="_blank" }.

## Variable Size Modules

The following modules support variable sizing, meaning they can change scale over the course of the loop.

- Water
- Lava
- Star
- Sand
- Funnel
- Ring

To do this, simply specify a `curve` property on the module

```json
{
    "name": "My Cool Planet",
    "Water": {
        "curve": [
            {
                "time": 0,
                "value": 100
            },
            {
                "time": 22,
                "value": 0
            }
        ]
    }
}
```

This makes the water on this planet shrink over the course of 22 minutes.

## Quantum Planets

In order to create a quantum planet, first create a normal planet. Then, create a second planet config with the same `name` as the first and `isQuantumState` set to `true`.
This makes the second planet a quantum state of the first, anything you specify here will only apply when the planet is in this state.

```json
{
    "name": "MyPlanet",
    "Orbit": {
        "semiMajorAxis": 5000,
        "primaryBody": "Sun"
    }
}
```

```json
{
    "name": "MyPlanet",
    "isQuantumState": true,
    "Orbit": {
        "semiMajorAxis": 1300,
        "primaryBody": "TIMBER_HEARTH"
    }
}
```

## Barycenters (Focal Points)

To create a binary system of planets (like ash twin and ember twin), first create a config with `FocalPoint` set

```json
{
    "name": "My Focal Point",
    "Orbit": {
        "semiMajorAxis": 22000,
        "primaryBody": "Sun"
    },
    "FocalPoint": {
        "primary": "Planet A",
        "secondary": "Planet B"
    }
}
```

Now in each config set the `primaryBody` to the focal point

```json
{
    "name": "Planet A",
    "Orbit": {
        "primaryBody": "My Focal Point",
        "semiMajorAxis": 0,
        "isTidallyLocked": true,
        "isMoon": true
    }
}
```

```json
{
    "name": "Planet B",
    "Orbit": {
        "primaryBody": "My Focal Point",
        "semiMajorAxis": 440,
        "isTidallyLocked": true,
        "isMoon": true
    }
}
```
