# Pussycord

![Build Status](https://img.shields.io/badge/build-initial_release-orange) ![Platform](https://img.shields.io/badge/platform-windows-blue) ![Security](https://img.shields.io/badge/security-hardened-red) ![Language](https://img.shields.io/badge/language-C%23-purple)

Pussycord is a modified implementation of Discord Canary entirely rewritten in C#. I have been programming for three years and developed this project to address specific privacy concerns found in the standard client.

This is an initial release. The primary focus is security rather than visual customization. I have not altered the standard Discord user interface.

### Core Functionality

 The application prioritizes data protection through several mechanisms.

*   **Process Sandboxing**
    The client creates a sandbox environment around the Discord process. This isolation ensures that any malicious execution within the application remains contained and cannot access your main system files.

*   **Security Filters**
    I implemented active protections against common threats. The client blocks token grabbers and filters connections to prevent IP logging attempts.

*   **Link Handling**
    There is an integrated web browser that runs inside the sandbox. This allows you to open links without exposing your main browser data. You can disable this feature and use your default system browser if you prefer.

### Development Status

The project is currently in a beta state.

*   **Updates**
    The application checks a local `VERSION` file to determine the current build status.
    
*   **Plugins**
    There is no support for plugins at this time. I plan to implement a secure plugin framework in future versions once the core stability is confirmed.

*   **Interface**
    The visual interface is identical to the vanilla Discord client.

This is a personal project aimed at improving user security.

***

*Disclaimer: This software is provided as is. I am not responsible for account limitations resulting from the use of modified clients.*
