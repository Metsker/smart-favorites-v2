# Smart Favorites v2 for Unity

A favorites panel for Unity for better asset management in your project

![Favorites Panel for Unity](https://github.com/user-attachments/assets/698b82d5-dcdb-4b2d-89d2-0decb2e1286d)

## Features

- Custom categories
- Works with both project and scene assets
- Asset previews
- Items reordering
- Multiselect (for removing items from the list)

## Installation

Import the [last package](https://github.com/Metsker/smart-favorites-v2/releases) to your project

## Usage

- Open the window with Window >> Favorites
- "FavoritesData" asset will be created at the project root, it contains plugin's settings and save data, you can change its location freely
- Add a favorites list with the +
- Remove a favorites list with the -
- Rename a favorites list with the pen icon at the right of the name
- Switch to an other list by clicking on the current list name, or with arrow buttons
- Drag-and-drop any asset to the window to add it to the current favorites list
- Single click the list item to ping it, Double click to focus it, or Drag-and-drop it away from the window to add it to the current context, if it's valid

## Known issues

- Rearrange of the categories is currently possible only through the "FavoritesData" asset 
