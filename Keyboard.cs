using System.Globalization;
using System.Text.RegularExpressions;

namespace KeyboardRebind
{
    public class Keyboard
    {
        public Keyboard(string keyboard_data_path)
        {
            string keyboard_name = Path.GetFileName(keyboard_data_path);
            string hid_key_code_path = Path.Combine(keyboard_data_path, "HidKeyCodes.md");

            Name = keyboard_name;

            ParseBaseBinding(hid_key_code_path);
        }

        public string Name { get; }
        public Dictionary<string, byte> BaseBindings { get; } = [];
        public Dictionary<string, byte> ModifiedBindings { get; set; } = [];


        private void ParseBaseBinding(string base_binding_path)
        {
            if (!File.Exists(base_binding_path)) {
                throw new FileNotFoundException("Could not find the HID key-code reference file.", base_binding_path);
            }

            BaseBindings.Clear();

            // Specific regex to parse the .md file
            Regex binding_row_pattern = new(@"^\|\s*(?<name>.+?)\s*\|\s*(?<hex>0x[0-9A-Fa-f]{2})\s*\|\s*(?<decimal>\d+)\s*\|$", RegexOptions.Compiled);
            foreach (string line in File.ReadLines(base_binding_path))
            {
                Match match = binding_row_pattern.Match(line);
                if (!match.Success)
                {
                    continue;
                }

                string key_name = match.Groups["name"].Value.Trim();
                string hex_code = match.Groups["hex"].Value;
                byte hid_code = byte.Parse(hex_code.AsSpan(2),NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);

                BaseBindings.TryAdd(key_name, hid_code);
            }

            ModifiedBindings = BaseBindings;
        }
    }
}
