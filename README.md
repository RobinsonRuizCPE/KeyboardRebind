#### What is this ####
This is a web service that allows you to rebind keyboard keys and save them into a batabase
This DOES NOT rebind your actual keyboard keys, this is just a POC for this kind of service.

#### How to run the project ####
- From the repository root, use the command prompt:
"dotnet run"

- This will print the address :
"Now listening on: http://localhost:<port>"

- Copy this adress in a web browser

Note : If you want to build the .exe and use it instead, it must be placed at the root of the repo with the necessary DLL/Json/etc...


#### About the Web UI ####

The browser UI lets you select a keyboard, load, save, update, or delete a profile, click a target key and press a supported physical key to change its target.
It's possible that, depending on the keyboard used, some of the key pressed aren't supported.

#### Add a new keyboard ####

To add a new keybord to the service, simply add a folder inside KeyboardRebind\KeyboardsData\{KeyboardName}
Inside this folder, place the HidKeyCodes.md file
The keyboard is now added to the service, the new database will be created automatically at runtime

#### API Documentation ####

## GET Methods ##
--- List available keyboards ---

GET "/api/keyboards"

This returns a list of keyboards names.

--- Get keyboard base key data ---

GET "/api/keyboards/{keyboardName}"

This returns the following keyboard attributes :
{
	Name;
	Database path;
	Base bindings;
	Currently loaded modified bindings (if loaded in the GUI, otherwise it's just a copy of the base one)
}

Note : Use `Apex%20Pro%20Gen%203` as the {keyboardName}  in the URL for the "Apex Pro Gen 3". 

--- List profiles for a keyboard ---

GET "/api/keyboards/{keyboardName}/profiles"

This list all the profiles saved for this keyboard. A profile is composed of : 
{
	Name;
	Id;
}

--- Load modified bindings for a profile ---

GET "/api/keyboards/{keyboardName}/profiles/{profileId}"

This returns the following keyboard attributes :
 {
	Name;
	Database path;
	Base bindings;
	Modified bindings for this profile ID
}


## POST Methods ##
The following POST Methods use JSON

--- Create a mapping profile ---

POST "/api/keyboards/{keyboardName}/profiles"

{
  profileName: {profileName}
}

Response:

{
  "id": {profileId},
  "name": {profileName}
}

Test with a command prompt :
curl.exe -X POST "http://localhost:5098/api/keyboards/Apex%20Pro%20Gen%203/profiles" -H "Content-Type: application/json" -d "{\"profileName\":\"GUILess test\"}"


--- Reset the in-memory mappings to defaults ---

POST "/api/keyboards/{keyboardName}/reset"

This reset the currently selected profile to the default one. This is used for GUI to handle the combobox.
This DOES NOT remove the any data from the database !

Test with a command prompt :
curl.exe "http://localhost:5098/api/keyboards/Apex%20Pro%20Gen%203/profiles/{profileId}"

## PUT methods ##
--- Save profile mappings ---

PUT "/api/keyboards/{keyboardName}/profiles/{profileId}/bindings"

The service validates source-key names and target HID codes against the keyboard's HID reference data. 

Example request mapping `A` and `4`:

{
  "modifiedBindings": {
    "A": {NewHIDCode},
    "4": {NewHIDCode}
  }
}

Test with a command prompt :
curl.exe -i -X PUT "http://localhost:5098/api/keyboards/Apex%20Pro%20Gen%203/profiles/1/bindings" -H "Content-Type: application/json" -d "{\"modifiedBindings\":{\"A\":29,\"4\":31}}"

Note : Entries whose target is the source key's default HID code are not stored as bindings.

## DELETE methods ##

--- Delete a profile

DELETE "/api/keyboards/{keyboardName}/profiles/{profileId}"

This deletes a profile and it's modified bindings in the database

#### TODO ####

- By adding a "profile" feature, some of the demands of the exercise are met in a different way. Since the exercice says that ""These are the general expected inputs, but the exact shape of them is up to you"", I allowed myself to change a bit the design of the service.
- When a profile is deleted, the ID of the next profile is still an increment of the previous ones. If the user creates MAX(int) profiles, issues may appear.
- Updating bindings format is not user-friendly in command line, since we ask for the {NewHIDCode}, it should ask for a key maybe, and resolve the HID code internaly.
- The granualarity(?) of the methods feels a bit off. The service method were conceptualized, in parts, with the GUI in mind. This means some of the methods input/output are not really well designed for the service itself.
