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

        foreach (var binding in request.ModifiedBindings) {
            string source_key_name = binding.Key;
            byte target_hid_code = binding.Value;

            if (!found_keyboard.BaseBindings.ContainsKey(source_key_name))
            {
                return BadRequest(
                    $"'{source_key_name}' is not a valid source key.");
            }

            if (!found_keyboard.BaseBindings.ContainsValue(target_hid_code))
            {
                return BadRequest(
                    $"0x{target_hid_code:X2} is not a valid target HID code.");
            }
        }

        found_keyboard.ModifiedBindings = new Dictionary<string, byte>(request.ModifiedBindings);
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
    public Dictionary<string, byte>? ModifiedBindings { get; set; }
}
