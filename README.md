<p align="center">
<img src="./banner.png" width="700">
</p>

# Celer [BETA]

> The open source, advanced, friendly and cutest toolbox for Windows 10 & 11.

> [!IMPORTANT]
> Celer is still a work in progress and thus should only be used for testing. Please be patient for a stable release.

[![License: GPL 3.0](https://img.shields.io/badge/License-GPLv3.0-green.svg)](https://www.gnu.org/licenses/gpl-3.0.en.html) ![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/surfscape/celer/total)

## Table of Contents

- [1. Introduction](#1-introduction)
- [2. Features](#3-features)
- [3. Installation](#4-installation)
- [4. Requirements](#5-requirements)
- [5. Roadmap](#6-roadmap)
- [7. Contributing](#8-contributing)
- [8. License](#9-license)

## Introduction

Celer is an app for Windows 10 & 11 with the purpose of giving you back control of your system. The app is structured into modules that contain various features that help you maintain your machine optimized.

## Features

- Dashboard - real time information of the hardware
- Cleaning - clean up unnecessary files from the system (+ limited support for third party software)
- Optimization - general battery & energy data, sensors, memory, GPU, and more
- Maintenance - easy access to Windows internal repair tools and network testing (with option to change DNS system-wide)
- Privacy & Security - general information of the system's privacy and security ratings

## Installation

We currently only provide x64 binaries and setup files for Celer but x86 and portable packages are planned when we reach a stable release.

The setup is hosted on GitHub and can be downloaded either through [GitHub Releases](https://github.com/surfscape/celer/releases) or through [Celer's page on SurfScape](http://surfscape.eu/projects/celer/#downloads).

### Requirements

- Windows 10 or 11 (64-bit only)
- Minimum of 1 GB of available RAM \*
- Minimum of 150 MB of free disk space
- [.NET Runtime 10 (x64)](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-10.0.0-windows-x64-installer) must be installed

<small>\* This metric depends on the RAM available to .NET which might reduce/increase Celer memory usage</small>

## Roadmap

We are currently planning new features some of these include:

- Support for third-party tools (ex: AdWare Cleaner, TRON Script, Snappy Driver Installer Origin)
- A frontend for Winget with recommended software
- Light theme (already available on beta 2 but without a setting to change it)
- Multi-language support
- Run in the background (planned on beta 2)
- Tray icon support with a small dashboard for quick actions (ex: cleaning temp files, restarting services and checking system status) (planned on beta 2)

## Contributing

### Commit Convention

- **feat:** commit that adds a new feature
- **fix:** commit that fixes an existing feature
- **refactor:** commit that rewrites code without adding or fixing a behaviour
  - **refactor(perf):** commit that rewrites code with the objective of improving performance
- **docs:** commit related to documentation
- **chore:** commit that doesn't fit the types above

## License

Celer is licensed under [GPL v3.0](https://www.gnu.org/licenses/gpl-3.0.en.html).
