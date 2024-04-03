I haven't yet tested this project an Android, but it's working on an iPhone.

The assets I've created for this project are stored in the "Assets/CustomAssets", and are as follows:
- Hyperlinks (the name of the scene I created)
- SpawnHyperlinks (a C# script for spawning hyperlinks when a Kanji AR marker is seen on camera
- rectangle (a prefab that I'm currently using as a hyperlink overlay)
- blue-semi-opaque (a semi-transparent blue material for our hyperlink overlay)
- archivist-protoypes.pdf (a pdf containing some of our prototypes with AR markers; for testing)

Furthermore, the scripts for monitoring for AR markers and spawning hyperlinks are attached as components to the "AR Session Origin" object in the "Hyperlinks" scene.

Lastly, if you'd like to change which AR marker to use (e.g. Kanji vs. something else), you can update the ReferenceImageLibrary that's listed as an argument to the AR Tracked Image Manager script on the "AR Session Origin" object.
