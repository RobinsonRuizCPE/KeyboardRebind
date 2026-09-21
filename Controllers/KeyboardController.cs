using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace KeyboardRebind.Controllers;

[ApiController]
[Route("api/keyboards")]
public class KeyboardController : ControllerBase
{
    public KeyboardController(List<Keyboard> keyboard_list) {
        this.KeyboardsList = keyboard_list;
    }

    private readonly List<Keyboard> KeyboardsList;

    [HttpGet]
    public IActionResult GetKeyboardNames()
    {
        List<string> keyboard_names = new List<string>();
        foreach (Keyboard keyboard in this.KeyboardsList) {
            keyboard_names.Add(keyboard.Name);
        }

        return Ok(keyboard_names);
    }

    [HttpGet("{keyboard_name}")]
    public IActionResult GetKeyboard(string keyboard_name)
    {
        var found_keyboard = FindKeyboard(keyboard_name);
        if (found_keyboard is null) {
            return NotFound($"Keyboard '{keyboard_name}' was not found.");
        }

        return Ok(found_keyboard);
    }

    [HttpGet("{keyboard_name}/profiles")]
    public IActionResult GetProfiles(string keyboard_name)
    {
        var found_keyboard = FindKeyboard(keyboard_name);
        if (found_keyboard is null) {
            return NotFound($"Keyboard '{keyboard_name}' was not found.");
        }

        var profiles = KeyboardBindingsDatabaseHelper.GetProfiles(found_keyboard);
        return Ok(profiles);
    }

    [HttpGet("{keyboard_name}/profiles/bindings")]
    public IActionResult GetProfilesWithBindings(string keyboard_name)
    {
        var found_keyboard = FindKeyboard(keyboard_name);
        if (found_keyboard is null) {
            return NotFound($"Keyboard '{keyboard_name}' was not found.");
        }

        var profiles = KeyboardBindingsDatabaseHelper.GetProfilesWithBindings(found_keyboard);
        return Ok(profiles);
    }

    [HttpGet("{keyboard_name}/profiles/{profile_id:int}")]
    public IActionResult LoadProfile(string keyboard_name, int profile_id)
    {
        var found_keyboard = FindKeyboard(keyboard_name);
        if (found_keyboard is null) {
            return NotFound($"Keyboard '{keyboard_name}' was not found.");
        }

        KeyboardBindingsDatabaseHelper.LoadModifiedBindings(found_keyboard, profile_id);
        return Ok(found_keyboard);
    }

    [HttpPost("{keyboard_name}/profiles")]
    public IActionResult CreateProfile(string keyboard_name, [FromBody] CreateProfileRequest request)
    {
        var found_keyboard = FindKeyboard(keyboard_name);
        if (found_keyboard is null) {
            return NotFound($"Keyboard '{keyboard_name}' was not found.");
        }

        if (string.IsNullOrWhiteSpace(request.ProfileName)) {
            return BadRequest("A binding name is required.");
        }

        int profile_id = KeyboardBindingsDatabaseHelper.CreateProfile(found_keyboard, request.ProfileName);
        return Ok(new {
            Id = profile_id,
            Name = request.ProfileName
        });
    }

    [HttpPut("{keyboard_name}/profiles/{profile_id:int}/bindings")]
    public IActionResult SaveProfile(string keyboard_name, int profile_id, [FromBody] SaveBindingsRequest request)
    {
        var found_keyboard = FindKeyboard(keyboard_name);
        if (found_keyboard is null) {
            return NotFound($"Keyboard '{keyboard_name}' was not found.");
        }

        if (request.ModifiedBindings is null) {
            return BadRequest("Modified bindings are required.");
        }

        // Resolve the key_name of the request to the HID code
        var resolved_bindings = new Dictionary<string, byte>();
        foreach (var binding in request.ModifiedBindings)
        {
            var source_key_name = binding.Key;
            var target_key_name = binding.Value;

            if (!found_keyboard.BaseBindings.ContainsKey(source_key_name))
                return BadRequest($"'{source_key_name}' is not a valid source key.");

            if (string.IsNullOrWhiteSpace(target_key_name) || !found_keyboard.BaseBindings.TryGetValue(target_key_name, out var targetHidCode))
                return BadRequest($"'{target_key_name}' is not a valid target key.");

            resolved_bindings[source_key_name] = targetHidCode;
        }

        found_keyboard.ModifiedBindings = resolved_bindings;
        KeyboardBindingsDatabaseHelper.SaveModifiedBindings(found_keyboard, profile_id);
        return NoContent();
    }

    [HttpDelete("{keyboard_name}/profiles/{profile_id:int}")]
    public IActionResult DeleteProfile(string keyboard_name, int profile_id)
    {
        var found_keyboard = FindKeyboard(keyboard_name);
        if (found_keyboard is null) {
            return NotFound($"Keyboard '{keyboard_name}' was not found.");
        }

        KeyboardBindingsDatabaseHelper.DeleteProfile(found_keyboard, profile_id);
        found_keyboard.ModifiedBindings = new Dictionary<string, byte>(found_keyboard.BaseBindings);
        return NoContent();
    }

    [HttpPost("{keyboard_name}/reset")]
    public IActionResult ResetBindings(string keyboard_name)
    {
        var found_keyboard = FindKeyboard(keyboard_name);
        if (found_keyboard is null) {
            return NotFound($"Keyboard '{keyboard_name}' was not found.");
        }

        found_keyboard.ResetModifiedBindings();
        return Ok(found_keyboard);
    }

    private Keyboard? FindKeyboard(string keyboard_name)
    {
        return KeyboardsList.Find(
            keyboard => string.Equals(
                keyboard.Name,
                keyboard_name,
                StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class CreateProfileRequest
{
    public string ProfileName { get; set; } = "";
}

public sealed class SaveBindingsRequest
{
    public Dictionary<string, string>? ModifiedBindings { get; set; }
}
