# Realistic Vehicle Colors

A [Cities: Skylines II](https://www.paradoxinteractive.com/games/cities-skylines-ii) mod that redistributes civilian vehicle colors in traffic to match real-world car-color statistics — mostly white, black, grey and silver — instead of the near-uniform palette the base game uses. It also adds rare colors (e.g. orange) beyond the stock palette and exposes per-color sliders in the in-game Options menu.

**Published on PDX Mods:** [Realistic Vehicle Colors](https://mods.paradoxplaza.com/mods/143394) (ModID 143394)
**Discussion:** [Paradox forum thread](https://forum.paradoxplaza.com/forum/threads/realistic-vehicle-colors.1921167)

This repository exists so the mod's source is fully public and auditable — nothing hidden, no obfuscation.

## Features

- **Real-world defaults** for ten color buckets, on by default.
- **Per-color sliders** to tune the mix yourself: White, Black, Grey, Silver, Red, Blue, Brown / Beige, Green, Yellow and Other.
- **Three custom color slots** — pick any hex code and a probability. Great for adding rare colors (orange, teal, magenta).
- **Apply settings live** — all cars change color instantly, no restart.
- **Camper trailers** restricted to white and brown to match how they actually look on the road.
- **Clean removal** — disabling or uninstalling the mod restores every car to the game's original colors. The mod only mutates runtime prefab buffers, never save data.

## Scope

Affects civilian road vehicles: personal cars, food-delivery scooters, campers and bicycles.

Service vehicles (police, fire, ambulance, garbage, post, taxi, transit), farm equipment, semi-trailers, trains, ships and airplanes are left at their stock paintwork.

## How it works

On load the mod walks each civilian-vehicle prefab (parent → `SubMesh` → child render prefab → `ColorVariation` buffer) and rewrites the per-entry `m_Probability` weights according to the slider values. The game's `MeshColorSystem` then does weighted sampling at vehicle spawn, so newly spawned vehicles follow the new distribution. See the source and inline comments for details.

## Building

- IDE: Visual Studio 2022, or `dotnet build`.
- Requires the Cities: Skylines II modding toolchain installed (sets the `CSII_*` environment variables the build references).
- `dotnet build` auto-deploys the DLL to the local Mods folder.

## License

[MIT](LICENSE)
