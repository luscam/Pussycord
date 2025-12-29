# Pussycord 🐱

![Build Status](https://img.shields.io/badge/build-beta-orange) ![Platform](https://img.shields.io/badge/platform-windows-blue) ![Privacy](https://img.shields.io/badge/privacy-paranoid-red) ![C#](https://img.shields.io/badge/language-C%23-purple)

Alright, welcome to **Pussycord**. Yeah, the name stays.

Here’s the deal: this is a modified version of **Discord Canary**, completely rewritten in C#. I’ve been coding for about three years now—not a lifetime, sure, but long enough to realize that standard chat clients leak data like a sieve. I built this because I wanted to sleep better at night.

This is an **initial release**. It’s raw.

### What is this thing?

It's Discord, but locked down. I haven't touched the UI—it looks and feels exactly like the vanilla app you're used to. I didn't want to mess with the visuals yet; I wanted to mess with the security.

The goal right now is pure user safety. Making it pretty or adding bells and whistles comes later.

### Features (The Safety Net)

*   **The Sandbox:** The app creates a sandbox around the Discord process. It’s like a quarantine zone. If something nasty tries to execute, it stays trapped inside the Pussycord process and shouldn't touch your actual system files.
*   **Anti-Token-Grabber:** Blocks scripts trying to snag your auth token.
*   **Anti-IP-Logger:** Filters out the sketchy connections before they can dox you.
*   **The Browser:**
    *   There's an **integrated browser** for opening links safely within the sandbox.
    *   Prefer your own setup? You can still toggle it to use your default browser. Up to you.

### What's Missing? (For Now)

I'm playing the long game here.

*   **No Plugins:** We don't support plugins yet. I know, I know. But adding third-party code defeats the purpose of a secure client until I can build a stable framework for it. It's planned for the future.
*   **Updates:** It’s simple. The app checks a local `VERSION` file to see where it's at. No complex auto-updaters breaking things in the background just yet.

### The Plan

Right now? Stability and privacy.
Later? A cleaner codebase, maybe some UI tweaks, and eventually a safe way to handle plugins.

I'm just one dev doing this to keep my own data safe. If it helps you too, awesome. If you find a bug (and you probably will), let me know.

---
*Disclaimer: Use at your own risk. I'm doing my best to make this secure, but I'm not a corporation with a thousand engineers. Just a guy who likes C# and hates getting IP logged.*
