Simple command line utility that converts the Opera bookmark file (a json format) into the Netscape Bookmark File Format (an html format) so that it can be transferred to other browsers. Conversion specifically targets the "bookmark bar", other bookmarks are currently ignored. Full folder structure (including nested) is retained.

Bookmark file is located in %appdata%\Opera Software\Opera Stable\Bookmarks

Written in C#, requires .NET 3.5
Made for Opera v38.0.2220.41, compatibility with other versions is untested
