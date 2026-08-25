@"
# Woordenschat in VR — Virtual Harbour

A VR vocabulary-learning game for young children (kleuters, ages ~4–6), built as part of the **NOLAI** research project in collaboration with Radboud University. Players explore a small island harbour town across multiple levels, meeting AI-driven characters who help them learn new words in rich, contextual situations.

Read more about the project and its research background in this article by Radboud University: [Virtuele havenstad helpt kleuters hun woordenschat te vergroten](https://www.ru.nl/over-ons/nieuws/virtuele-havenstad-helpt-kleuters-hun-woordenschat-te-vergroten).

In development since 2023.

## How it works

This Unity project is the VR client. It connects live to a companion dashboard ([`nolai-dashboard`](https://github.com/Stijn-van-Sprang/nolai-dashboard)), which streams commands into the game and records session data as children play. Non-player characters in the game have distinct personalities and hold real conversations with players, powered by a locally hosted language model on the dashboard side. Some lines (e.g. introductions, hints) are fixed voice lines; open conversation is generated live.

## Requirements

- [Unity Hub](https://unity.com/download) + a Unity account (a free personal license is sufficient)
- A running instance of the [``nolai-dashboard``](https://github.com/Stijn-van-Sprang/nolai-dashboard) project (see that repo's README) — the game will not function fully without it
- A VR headset supported by Unity's XR toolchain. THis project has been build and tested for research purpouses on the Meta Quest 3 with an extra small headstrap for the small heads of children.

## Getting started

1. **Clone this repository.** The latest version lives on the ``Main`` branch. Remember where you clone it to — you'll point Unity Hub at this folder.
2. **Install Unity Hub** from [unity.com/download](https://unity.com/download) and sign in (create a free account if needed). When prompted, add a free license to your account — an education license also works if you have one.
3. **Add the project** in Unity Hub: *Add* → select the folder you cloned in step 1.
4. Unity Hub will flag that the required editor version isn't installed. Install the recommended version through Unity Hub. During installation, it's recommended to also install **VS Code** with the *"Game development with C# in Unity"* module (offered automatically by the installer).
5. **Open the project** and go to the ``Scenes`` folder — all game scenes are located there.
6. In the scene view, find the **``Dashboard Connector``** component and set its IP address to the IP of the machine running the dashboard (``localhost`` if running both on the same machine).
7. Press Play. If the dashboard is already running, you should see a live preview/connection appear on the dashboard side.

## Third-party assets

Some assets used in this project require their own (in some cases paid) license and are **not included** in this repository. These are kept out of version control (see ``.gitignore``) and referenced under each asset's folder in ``Assets/3rd-party/``, which contains license files and links to the original sources. Before opening the project, check that folder and acquire/import any listed assets under your own license.

## Project structure

- ``Assets/Scenes/`` — all game levels/scenes
- ``Assets/3rd-party/`` — license files/links for third-party assets (see above); the actual packaged content is gitignored
- ``Dashboard Connector`` (scene component) — handles the live connection to the dashboard's API

## Status

Actively developed research prototype. Expect ongoing changes as the research and testing phases (with early childhood educators) continue.

## License

_TBD — add license terms here before making the repository public._
"@ | Out-File -FilePath "README.md" -Encoding utf8