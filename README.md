# Switch-Toolbox
A tool to edit many file formats between the Wii U and the Nintendo Switch. 

## Information
If the tool does not open, right click the zip and hit Unblock!
Ensure that you have .NET 4.6 or newer installed, too.

## Changelog

v0.8.5.2 - v0.8.63

- Fixed exporting models (static for now)
- Fixed some WiI U bfres not culling properly.
- Fix importing DDS files
- Fix Wii U textures saving with wrong channel type.
- Show properties for wii u textures.
- Fix texture importer not importing all textures at once.
- Add proper point filtering for Wii U.
- Add toggle for mesh visibility in treeview.
- Fix some crashes resaving a bfres that was already saved.
- Play animations properly on wii u.
- Update importing textures properly.
- Fix shader colors being incorrectly used.
- Add proper placeholder textures for wii u if missing to prevent crashes.
- Add basic textures if unused for the imported model (if materials are used)
- Fix wii u material params saving some invalid data
- Fix texture refs being saved for wii u.
- Added option to disable importing new materials when replacing an fmdl. Note this will make
- all objects mapped to the first material and texture mapping will not be automated.
- Batch export and replacing of bfmats.
- Fixed wii u importing with wrong face primitive type (caused wire frame visual effect).
- Basic transform tool (right click fmdl, transform)
- Fixed rotation import setting where normals would break.
- Fixed DDS being read by program after saved
- Add support for .dds2 extension
- Quick bug fixes for importing textures.
- Auto updater.
- Fix DDS exporting for DX10 formats.
- More Misc fixes.


v0.8.5.1
 - Basic .nutexb dds injection. Must be same width/height and format due to .arc size limitations atm!

v0.8.4 - v0.8.5
- Textures for Wii U deswizzle properly thanks to AboodXD. 
- Zstb compressed files will be detected and decompressed automatically on open. 
- Textures BC1-BC5 will show properly if DirectXTex is not working on older windows versions. Small work around atm.
- Improper transformations (like bad rotations) should be fixed now. 
- Basic GTX file viewing
- Fixed BNTX texture alignment if there is only one texture present in the file. 
- Many more bug fixes to enhance the user experience. 

v0.8.3
- Support all nutexb formats from .arc.
- Fix brightness issues for nutexb srgb (using unorm for decoding atm).

v0.8.2 Changelog
 - Fixed nutexb errors (incorrect sizes)
 - Batch exporting nutexb works properly now. Will export with offset for unique name.
 - DDS save properly to open in photoshop and other programs. 

v0.8.1 Changelog
 - Fixed error messages and freezes from nutexb.

v0.8 Changelog
- DDS compression reworked allowing png importing, and previewing of all DDS formats from [DirectXTex](https://github.com/Microsoft/DirectXTex).
- Exporting/importing of all animation types as binary.
- .Nutexb previewing/exporting. (and batch exporting)
- DAE/FBX rigging supported. Models will no longer show invisible. 
- Improvements to BFRES library. Some games may work better now. 
- Channel types are now set on import. May fix normal map issues.
- Archives use double click for opening files
- DDS cubemaps can be imported
- GFPAK repacking (untested)
- Animations for skeleton animations can be played (thanks to smash forge). 
- Improvements to UI. May load quicker. 
- Update bake map placeholder textures to look proper.
- Many improvements and big fixes!

v0.72 Changelog
- Fixed crashes from RGBA DDS.
- Added an option to disable viewport. (May be quicker, and for those who use old GL versions).

v0.71 Changelog
- Stability improvements. 
- Add byml editor back in.
- Some bug fixes and exception issues

This tool currently features:
- BFRES
   - Model importing (dae, fbx, obj, and csv)
   - Material editing (Render info, texture mapping, parameters, etc)
   - Material  copying
   - Animation and model sub section can be exported/imported.
   - Can delete, add, replace individual objects from an fmdl.
- BNTX
   - Can add/remove textures.
   - Can import textures as DDS. (Thanks to AboodXD! png/jpeg, etc planned later)
   - Can export as binary, dds, png, tga, etc.
   - Can preview mipmap and surface(array) levels.
- NUTEXB
     - Can export as binary, dds, png, tga, etc.
     - Can preview.
- SARC
   - Supported editing/saving data opened. (Automatically saves data opened in objectlist if supported)
   - Supports padding (Thanks to Exelix and AboodXD)
   - Can save sarcs in sarcs in sarcs.
- BARS
   - Can extract and replace audio files.. (rebuilds the file)
- KCL
   - Preview collision models.
   - Replace/Export as obj (Thanks to Exelix)
- BFFNT
   - Can extract font images and preview their image arrays (BNTX)
- GFPAK
   - Can extract, edit, and rebuild (untested).

## Building
To build make sure you have Visual Studio installed (I use 2017, older versions may not work) and open the .sln. Then build the solution as release. It should compile properly on the latest.

In the event that the tool cannot compile, check references. All the libraries are stored in Switch-Toolbox/Lib folder. 

## This tool is in BETA and not final! Code also needs some major clean up!
## Credits

- Smash Forge Devs (SMG, Ploaj,  jam1garner, smb123w64gb, etc) for some code ported over. Specifically animation stuff, GTX c# implementation, and some rendering.
- Assimp devs for their massive asset library!
- Wexos (helped figure out a few things, ie format list to assign each attribute)
- JuPaHe64 for the base 3D renderer.
- Every File Explorer devs (Gericom) for Yaz0 stuff
- Exelix for Byaml, Sarc and KCL library
- Syroot for helpful IO extensions and libraries
- GDK Chan for some DDS decode methods
- AboodXD for some foundation stuff with exelix's SARC library, GTX and BNTX texture swizzling and documentation
- MelonSpeedruns for logo.

Resources
- [Treeview Icons by icons8](https://icons8.com/)
- Smash Forge (Currently placeholders)

Libraries
- [Exelix (Sarc, kcl, and byml libraries)](https://github.com/exelix11/EditorCore/tree/master/FileFormatPlugins)
- [ZstdNet (Compression)](https://github.com/skbkontur/ZstdNet)
- [Be.HexEditor by Bernhard Elbl](https://sourceforge.net/projects/hexbox/)
- GL EditorFramwork by jupahe64
- [WeifenLuo for docking suite](http://dockpanelsuite.com/)
- [SF Graphics by SMG (Experimental](https://github.com/ScanMountGoat/SFGraphics) (currently just a placeholder for shader workflow and some useful things)
- [Audio & MIDI library](https://github.com/naudio/NAudio)
- [VGAudio](https://github.com/Thealexbarney/VGAudio)
- [Assimp](https://bitbucket.org/Starnick/assimpnet/src/master/)
- [OpenTK](https://github.com/opentk/opentk)
- [BezelEngineArchive Library](https://github.com/KillzXGaming/BEA-Library-Editor)
- [Syroot BinaryData](https://gitlab.com/Syroot/BinaryData)
- [Syroot Maths](https://gitlab.com/Syroot/Maths)
- [Syroot Bfres Library (Wii U)](https://gitlab.com/Syroot/NintenTools.Bfres)
- [Costura for embedding data for plugins](https://github.com/Fody/Costura) 
- [CsvHelper (unused atm but planned to be used](https://joshclose.github.io/CsvHelper/)

License
 in Switch_Toolbox\Lib\Licenses
 
 Please note if you do not want your library used or if i'm missing credits! 
