ImPatho README
==============================================

- `ImPatho` - it's the name of technology whose software part is this application. Hardware includes a cheap microscope cum USB camera for compatible with computer.

- This software was developed by [`Shalin Shah`](www.guptalab.org/shalinshah), Vijay Dhameliya, Shantanu Jain, and Nihar Trivedi under the supervision and guidance of [`Prof. Anil K Roy`](http://intranet.daiict.ac.in/~anil_roy/). 

- This software was a part of research project which was presented at an [`International IEEE conference`](http://ieeer10htc.org).

You can report bugs and feedback at :- `anil_roy@daiict.ac.in`                                      
To know more, visit our website :- `http://intranet.daiict.ac.in/~anil_roy/impatho/`                    
You can also find our extended paper(and abstract) at :- [`ImPatho Website!`](http://intranet.daiict.ac.in/~anil_roy/impatho/files/impatho.pdf)                                   
We are in process of applying for patent of this technology. Also, we are working on developing the mobile version of ImPatho.

**Copyright (C)-2013**


Folder Organisation
---------------------------------------------------------
- `Dependencies` - This folder contains all the dependencies for required for building the code. You may add this folder to NuGet Packages location.

- `Health Orgaznier` - This is the main folder where the entire code for app lies. Just go in this folder and open .sln file to run the project in Visual Studio 2013.

- `Matlab_Code` - This folder contains `SCA_RBC.m` matlab file which contains an algorithm to detect disease from magnified image of RBC. 

- `Server Code` - This folder contains the PHP code for server. Server is used to syncronize data across all the systems of a `health organization`.(Please refer the paper to understand meaning of health organization)

**Note :- Please don't add WindowsApp/WindowsApp/Generated Files in Commits**


Build Instructions
---------------------------------------------------------
**a)Requirements**

- Windows 8.1 Operating System

- Visual Studio 2013 Ultimate

- Math Work's Matlab


**b)Extra Libraries Required:-**

- `Callisto Metro UI Chart Library` - This library is required for displaying aesthetic visual apperance in Ananlysis part of application.

- `SQLite for WinRT` - This library is required for storing the exhaustive DB on system.

- `Send Mail WinRT` - This library is required for sending EMail directly from application.

- `SyncClient SQLite` - This library is required as dependency for SQLite library.

**c)Build Steps:-**

- Download the source code for the app by `downloading as zip file` or writing following line in terminal :- 

        git clone https://github.com/shalinshah1993/ImPatho.git

- Go to Health Organizer folder and double click on `.sln` file to open the project in Visual Studio 2013.

- In the `Project Solution`, you'll have References containing all the dependencies. Remove the four libraries mentioned above. 

- Add SQLite support by following steps mentioned at the end of this README. Please note that while adding dependecy folder for NuGet add `Dependencies` folder inside the project.

- After installing SQLite support, now, open `NuGet Package Manager Console`, change the source to Dependencies folder, and write :- 

        Install-Package <Package_Name>

Please note that by pressing <TAB> a drop down list with Package_Name will appear.

- After adding all the four libraries like this, clean the project and `Run it ON Local Machine`


Detailed Steps for error messages
-----------------------------------------------

**a)Add SQLite Support:-**

 - Follow steps given on this blog post `http://syncwinrt.codeplex.com/wikipage?title=w81temp`
 - Now, go to References in Solution Explorer and then right click and select MANAGE NUGET PACKAGES.
 - Now in search box type sqlite-net install it.
 - Clean the Solution and you are good to go. :) 

**b)ARM/x86/x64 Error :-**
 
 - Go to Build and then Configration manager. Select x64 architecture. QED! :)

**Note :- This is a Windows 8.1 store App and it doesn't work on other platforms.**
