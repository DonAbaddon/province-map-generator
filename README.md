# province-map-generator

This small handmade app helps you to draw province map (provinces.png) for your CK3 mod or for any other purpose.

## Releases
https://github.com/DonAbaddon/province-map-generator/releases

## How to use
1. Download Release.zip from releases and unpack archive anywhere.
2. Prepare map to be filled by provinces. You need to draw by hand 24bit .png image with only black (#000000) and white (#FFFFFF) colours. Black colour represents ocean, navigable rivers, mountains and any other places that won't be filled by provinces. White colour represents lands thar will bi filled.
3. Launch .exe file from archive.
4. Click "Load" button and select your .png province map template from step 2. The template will be drawn on canvas so you can see the process. Don't worry about resizing, the copy of image resized to fill canvas, original template and output will have correct size and resolution.
5. Click "Init" button to draw province basis. At this stage your template will be filled by straight coloured lines from wich provinces will grow. Check folder where your .exe app file is located, here you can already see your definition.csv file. It is guaranted that no colours are repeating and all colours at province map will have its own line in definition.csv.
6. Click "Draw" button and wait until all white space will be filled by provinces.
7. When all map is filled click "Stop" button to stop drawing. If you don't like the pattern, you can click "Init" again and repeat the process. If you spot some black land not filled by provinces then simply click "Draw". Provinces will continue to grow from the point you stopped the process.
8. Click "Save" button to save province map. 32bit "provinces.png" file will appear in .exe file folder. Change colour depth to 24bit and you are ready to go!
