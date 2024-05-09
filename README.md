# ARchivist

## Overview

The assets I've created for this project are stored in the "Assets/CustomAssets", and are as follows:
- Hyperlinks (the name of the scene I created)
- SpawnHyperlinks (a C# script for spawning hyperlinks when an AR marker is seen on camera
- QRCodeDector (a C# script for continually monitoring for QR codes)
- CoordinateConverter (a C# script for converting between PyMuPDF coordinates and Unity AR coordinates)
- OverlayData (a C# script that defines an "Overlay" object; note that this isn't the same exact thing as the hyperlink-overlay prefab defined below, although they are somewhat similar).
- hyperlink-overlay (a prefab that I'm currently using as an AR hyperlink overlay)
- blue-semi-opaque (a semi-transparent blue material for our hyperlink overlay)
- horse_article_prototype.pdf (a pdf containing a prototype that can be used for testing)

Furthermore, the scripts for monitoring for AR markers and spawning hyperlinks are attached as components to the "AR Session Origin" object in the "Hyperlinks" scene.

If you'd like to change which AR marker to use (e.g. Kanji vs. something else), you can update the ReferenceImageLibrary that's listed as an argument to the AR Tracked Image Manager script on the "AR Session Origin" object.

**A few other notes**:
- To scan the qr code, make sure to get the camera close to the code while keeping it in focus
- This currently seems to work way better with my iphone than with my android. With the android, you really have to keep the camera relatively close to the AR marker

