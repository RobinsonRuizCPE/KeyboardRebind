using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace KeyboardRebind.Controllers;

[ApiController]
[Route("api/keyboards")]
public class KeyboardController : ControllerBase
{
    public KeyboardController(List<Keyboard> keyboard_list)
    {
        this.KeyboardsList = keyboard_list;
    }

    private readonly List<Keyboard> KeyboardsList;

    [HttpGet]
    public IActionResult GetKeyboardNames()
    {
        List<string> keyboard_names = new List<string>();
        foreach (Keyboard keyboard in this.KeyboardsList)
        {
            keyboard_names.Add(keyboard.Name);
        }

        return Ok(keyboard_names);
    }

    [HttpGet("{keyboard_name}")]
    public IActionResult GetKeyboard(string keyboard_name)
    {
        var found_keyboard = KeyboardsList.Find(
            keyboard => {
                return keyboard.Name == keyboard_name;
            });

        if (found_keyboard is null)
        {
            return NotFound($"Keyboard '{keyboard_name}' was not found.");
        }

        return Ok(found_keyboard);
    }
}