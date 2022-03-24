# Cardographer Tabletop

The unity tabletop for ideation cards.

The tabletop will eventually be linked with cardographer-platform

## Version Details
- Uses Unity 2019.4.36f1 LTS
- [Uses Mirror Networking for communication between clients and servers](https://mirror-networking.com)
- [Uses Davinci for loading of images](https://github.com/shamsdev/davinci)

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


## Design Notes
The Cardographer Tabletop is made up of several components which allows the user to interact with ideation cards created for the tabletop, or created using the cardographer platform.

### Interactables
1. Cards: The base for all other interactables. The card bse class contains the basic movement interactions and the rest of the interactables inherit from this class.

2. Decks
3. Boards
4. Annotations

### 

## Controls
