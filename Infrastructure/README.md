# Celer.System

This new package contains abstractions to retrieve system data and set system parameters.

The main goal with this new package is to aggregate any type of low system interaction and information retrival that is currently scattered around viewmodels and services. This help with organisation, debugging, modularity, and portable.

## What is included in this package:

- **Battery** - provides methods to retrieve information of the system battery(ies)
- **Power** - provides methods to retrieve the current power plan and also change it
- **Memory** - provides methods to retrive memory information (includes per slot information)