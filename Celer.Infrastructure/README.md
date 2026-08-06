# Celer.Infrastructure

This package contains abstractions to retrieve system and hardware information and change system parameters.

The main goal is to aggregate any type of low system interaction and information retrival that is currently scattered around viewmodels and services. This help with organisation, debugging, modularity, and portability.

## Implemented

- **Battery** - retrieves information about the current state of batteries in the system through WIM by using CimSession