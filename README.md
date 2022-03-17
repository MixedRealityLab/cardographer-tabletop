# Cardographer Tabletop

The unity tabletop for ideation cards.

The tabletop will eventually be linked with cardographer-platform

## Version Details
- Uses Unity 2019 LTS

## How to Build

1. Download repository
2. Open with associated Unity editor
3. Set up the "Server" prefab on the main scene
   - Change the IP address to the IP of the server the tabletop is connecting too
   - Change the Network Transport method if required
   - Update the connection port
4. Create a headless build, then have that build run on the server that was specified above
5. Create client build, either a applicaiton or a webgl build
   - The webgl build should be hosted on the same server as the headless build
