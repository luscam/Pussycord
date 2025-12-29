# Pussycord 🐱

![Build Status](https://img.shields.io/badge/build-passing-brightgreen) ![Platform](https://img.shields.io/badge/platform-windows-blue) ![Paranoia Level](https://img.shields.io/badge/paranoia-maximum-red) ![C#](https://img.shields.io/badge/language-C%23-purple)

So, here we are. You've stumbled upon **Pussycord**. Yeah, I know the name's a bit... out there. Get over it.

This isn't your standard, bloated chat app. It's a modified version of **Discord Canary**, rewritten with C# because, frankly, I needed more control over what happens under the hood. I’ve been coding for two decades, and if there's one thing I’ve learned, it’s that trusting default clients with your data is a rookie mistake.

### Why does this exist?

Look, the internet is a mess. A literal minefield. You click one wrong link in a "general" channel and suddenly some script kiddie has your token or your IP. I got tired of worrying about it.

Pussycord is my answer to that anxiety. It’s built for privacy first. Everything else is secondary.

### The Good Stuff (Features)

I didn't bloat this with useless plugins or themes. It does what it needs to do to keep you safe.

*   **The Sandbox:** This is the cool part. The application creates a sandbox around the Discord process. Think of it like a hazmat suit for your PC. If something goes wrong inside the app, it stays inside the app. It doesn't leak out to your main system processes.
*   **Paranoid Security:**
    *   **Anti-Token-Grabber:** Stops those nasty scripts dead in their tracks. Your token stays yours.
    *   **Anti-IP-Logger:** We filter out the sketchy stuff before it even has a chance to ping back home.
*   **The Browser Situation:**
    *   I threw in an **integrated browser**. It’s sandboxed too. You can open links right there without exposing your main Chrome or Firefox data.
    *   Don't like it? Fine. You can still set it to open links in your normal default browser if you really want to live dangerously (or just trust your adblocker). Choice is yours.

### Getting Started

Honestly, it's pretty plug-and-play right now.

1.  Grab the latest release.
2.  Run the executable.
3.  Log in (safely, for once).

### A Note on Development

I'm just one person working on this when I have time (and enough coffee). It's an initial release. It might act weird sometimes. The UI isn't winning any beauty pageants yet. But it’s secure, and it works.

If you find a bug, open an issue. If you hate the name... well, I can't help you there.

Stay safe out there.

---
*Disclaimer: I'm not responsible if you get banned for using a modified client, though I've done my best to keep it under the radar. Use your head.*
