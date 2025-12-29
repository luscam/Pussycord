# Pussycord

![Version](https://img.shields.io/badge/dynamic/json?url=https://raw.githubusercontent.com/luscam/Pussycord/refs/heads/main/VERSION&query=$.version&label=version&color=blue) ![Build](https://img.shields.io/badge/build-passing-success) ![Security](https://img.shields.io/badge/security-hardened-red) ![Platform](https://img.shields.io/badge/platform-windows-blue)

Pussycord is a privacy-focused modification of Discord Canary written in C#. I have three years of programming experience and developed this sandbox environment to address security vulnerabilities present in the standard client.

This is an initial release focusing strictly on data protection. I have not modified the user interface. The visual experience remains identical to the stock Discord application.

### Security Implementation

The core function of this client is process isolation.

*   **Sandbox Environment**
    The application initiates a sandbox around the Discord process. This prevents malicious code execution from escaping the client scope and accessing system files.

*   **Active Filtering**
    The client includes logic to block token grabbers. It also filters connection attempts to prevent IP logging.

*   **Browser Integration**
    An integrated web browser handles links within the sandbox to contain cookies and tracking data. You can disable this setting to use your default operating system browser.

### Project Status

The current build prioritizes stability and security over feature expansion.

*   **Plugin Support**
    There is no support for plugins in this version. I intend to implement a secure plugin system in future updates.

*   **Updates**
    The version badge above reads directly from the local `VERSION` file in the repository.

This project is open for inspection.

***

*Disclaimer: This software is provided as is. Usage of modified clients may violate Terms of Service.*
