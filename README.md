# Pussycord

![Version](https://img.shields.io/badge/dynamic/yaml?url=https://raw.githubusercontent.com/luscam/Pussycord/main/VERSION&query=$&label=version&color=blue) ![Build](https://img.shields.io/badge/build-passing-success) ![Security](https://img.shields.io/badge/security-hardened-red) ![Platform](https://img.shields.io/badge/platform-windows-blue)

Pussycord is a modified implementation of Discord Canary entirely rewritten in C#. I have three years of programming experience and developed this project to address specific privacy concerns found in the standard client.

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
    
    The version badge above reads directly from the local `VERSION` file in the repository.

*   **Plugins**
    
    There is no support for plugins at this time. I plan to implement a secure plugin framework in future versions once the core stability is confirmed.

*   **Interface**
    
    The visual interface is identical to the vanilla Discord client.

This is a personal project aimed at improving user security.

***

*Disclaimer: This software is provided as is. I am not responsible for account limitations resulting from the use of modified clients.*
