# Interview Problem: Keyboard Key Remapping Service

## Background

Modern gaming keyboards allow users to reassign what a physical key does when pressed. For example, many users don't often use their CapLock key, so they instead choose to remap CapsLock to Control when pressed. Each key on a keyboard is identified by a **HID (Human Interface Device) Usage Code**, a standardized numeric identifier defined by the USB HID specification.

A reference table of common keys and their HID Usage Codes is provided in [HidKeyCodes.md](HidKeyCodes.md).

---

## Problem Statement

Your task is to build a small web service that supports key remappings for the Apex Pro Gen 3 keyboard. You can assume the list of keys found in HidKeyCodes.md are on the keyboard and available for remapping. The service should support:

### 1. Assign key mappings

Validate and save a list of key mappings for a given keyboard

These are the general expected inputs, but the exact shape of them is up to you

Expected input
* The keyboard name (Apex Pro Gen 3)
* a set of key remappings.  For example, "A (0x04)" should remap to "Z (0x1D)", and "4 (0x21)" should map to "2 (0x1F)"

Expected output
* None, other than indications of success or failure

### 2. Get All Current Key Mappings

Return a representation of all current key mappings for a given keyboard.  This should include all keys on the keyboard, whether they have been remapped or not

Expected input
* The keyboard name (Apex Pro Gen 3)

Expected output
* The list of all key mappings for the keyboard

---

## Expectations

* The service should support a web API that can be accessed via HTTP calls
* The solution should be written in an object-oriented language with a strong preference for either C# or Golang
* The key mappings should be stored in a SQLite database that persists between runs
* We need to be able to run your project locally, so avoid using licensed 3rd party packages
* Provide your solution to us in the form of a github repo
