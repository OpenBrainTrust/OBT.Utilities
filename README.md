# OBT.Utilities
 Various Utilities in C#. As the repository grows, some of these may be filed away in other repositories. We have a backlog of scripts to post and will continue adding.
 
 For now, we have the following:
 
 1. [KeywordSearch]("https://github.com/OpenBrainTrust/OBT.Utilities#keywordsearch")
 
 2. [TextFormatter]("https://github.com/OpenBrainTrust/OBT.Utilities#textformatter")
 
 3. [TerrainFromTexture2D]("https://github.com/OpenBrainTrust/OBT.Utilities#terrainfromtexture2d")
 
 ## KeywordSearch
 Tool for mass keyword searching all strings in any class of object or type. KeywordSearch takes arrays of generic C# objects, searches for the desired input text, and returns any result records and their text as part of its outputs, ordered by the best match or incidence of partial matches to search words and phrases. Can run either as MonoBehaviour or vanilla C# class.
 
 The input object arrays you want to search through can be of mixed types. KeywordSearch can return the strings from both class and array contents, but does not recursively search on its own beyond the first layer - you can however use its object returns themselves to conduct deeper searches.
 
 Uses System.Reflection, so be mindful of its accessibility when using with sensitive embedded information. Regardless, it will not return values that are compiled as private or similar.
 
 ## TextFormatter
 Formats and unformats blocks of Text for Machine Language Learning & Output Purposes.
 
 ## TerrainFromTexture2D
 
 Creates Terrain GameObjects ad hoc, or applies Terrain heightmaps to existing Terrains, from a Texture2D. The input Texture2D must be in R16 Format. Typical pipeline is Elevation Data -> 16-Bit Grayscale Image (.TIFF for instance) -> Place In Unity Project -> New Texture2D
 
 
 [Top](https://github.com/OpenBrainTrust/OBT.Utilities#obtutilities)