# AllJoyn Device Service Bridge
# for Philips Hue

This bridge acts as a Device Service Bridge between Philips Hue and AllJoyn Lighting Service Framework. 
The project contains a Headed Windows App for testing on local desktop, and a Headless startup task for running on IoT Core Framework.

The AllJoyn bridge can handle any number of Hue bridges, and will remember the registration for each of them across sessions, so you don't need to re-link the process to the bridge again.

To link a Hue bridge to the AllJoyn bridge, use AllJoyn Explorer to find and discover the Hue Bridges, press the "Link" button on your Hue bridge, and then execute the "Link" method on the AllJoyn bridge device. After this, any lightbulb associated with the Hue Bridge should show up in AllJoyn Explorer.
