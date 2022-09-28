# VS Haskell Tools
This is a small extension to Visual Studio to enable the execution and debugging of Haskell programs. It uses GHCi both as compiler and interpreter, so you must have GHCi installed. The install location of GHCi can be costumised in the Tools -> Options in visual studio if needed.
The extension can also be found on the [Visual Studio Markedplace](https://marketplace.visualstudio.com/items?itemName=KristianSkovJohansen.VSHaskellTools).
This extension comes with a set of functions, that can be found under the Extensions tab:

![image](https://user-images.githubusercontent.com/22596587/189736144-2a8d9ea6-538c-4b08-aced-e8aa90d9472e.png)

![image](https://user-images.githubusercontent.com/22596587/192885481-546c56f3-2843-41af-b49b-e21a12280139.png)

![image](https://user-images.githubusercontent.com/22596587/192885524-0a3070b1-82d0-48f3-86e6-5935678beaff.png)

# Features
This extension comes with a good set of features, some explained or shown in a bit more detail after this list:
* Syntax Highlighting 
* Prelude Quick Info
* Haskell Editor Options
* Execution of Haskell files
* Execution of a specific function in Haskell files
* Light debugger with GHCi
* GHCi interactive window
* Quick access toolbar

## Syntax Highlighting
This extension includes syntax highlighting of Haskell programs. It is enabled for all files with the '.hs' extension

![image](https://user-images.githubusercontent.com/22596587/190104879-58d989f1-3cce-463a-aba7-161999b4eec0.png)

## Prelude Quick Info
When you hover your mouse over a function or operator from the Haskell Prelude module, a quick info will display with type info.

![image](https://user-images.githubusercontent.com/22596587/192885356-ace64318-dcc8-431b-92b3-9f66c01f517c.png)

## Run Haskell File
This simply runs the Haskell file thats open, and sends the output to the "output" window.

![image](https://user-images.githubusercontent.com/22596587/190102529-924892d3-61f8-4b83-93e8-cef4c7b5f66d.png)

## Run Selected Functions
There is also the option to execute the currently selected function in your open Haskell file.

![image](https://user-images.githubusercontent.com/22596587/190102894-02efaa0d-de5a-40bb-ae0d-820968c57a91.png)

## Debug Haskell File
This opens up a debugging tool, that can be used to get an idea of what is going on in your Haskell program. It is possible to set breakpoints as well as take "steps" for each line call. It also provides information on the variables in use for a given line, as well as evaluate what the variables values are. There are also a history trace (somewhat like a stack trace) that can show you what lines where called.

![image](https://user-images.githubusercontent.com/22596587/190103094-0b37d4aa-1762-4771-877c-f0bb5a02039e.png)

# Haskell Interactive Window
This opens a sessions of GHCi with your '.hs' file loaded into, where you can then manually interact with the file.

![image](https://user-images.githubusercontent.com/22596587/190103299-dc9f0402-a42a-43b9-b607-f7bc8c93d520.png)
