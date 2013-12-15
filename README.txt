Our Winwdows App for IC2014

- Please don't add WindowsApp/WindowsApp/Generated Files in Commits

ASSETS - All the Images would reside here
COMMON - Some of the dependencies would be here while others would be in EXTERNAL DEPENDENCIES.
DEPENDENCIES - Contains Library packages needed by the code.

STEPS to Add SQLite Support:- 

 - Follow steps given on this blogPost http://syncwinrt.codeplex.com/wikipage?title=w81temp
 - Now, go to References in Solution Explorer and then right click and select MANAGE NUGET PACKAGES.
 - Now in search box type sqlite-net install it.
 - Clean the Solution and you are good to go. :) 

ARM/x86/x64 Error :-
 
 - Go to Build and then Configration manager. Select x64 architecture. QED! :)

