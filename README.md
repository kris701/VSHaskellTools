# VS Haskell Tools
This is a small extension to Visual Studio to enable the execution and debugging of Haskell programs. It uses GHCi both as compiler and interpreter, so you must have GHCi installed. The install location of GHCi can be costumised in the Tools -> Options in visual studio if needed.
The extension can also be found on the [Visual Studio Markedplace](https://marketplace.visualstudio.com/items?itemName=KristianSkovJohansen.VSHaskellTools).
This extension comes with a set of functions, that can be found under the Extensions tab:

![image](https://user-images.githubusercontent.com/22596587/189736144-2a8d9ea6-538c-4b08-aced-e8aa90d9472e.png)

## Syntax Highlighting
This extension includes syntax highlighting of Haskell programs. It is enabled for all files with the '.hs' extension

![image](https://user-images.githubusercontent.com/22596587/189486405-f3926f86-6dc1-42be-875b-b96fdbd31173.png)

## Run Haskell File
This simply runs the Haskell file thats open, and sends the output to the "output" window

![image](https://user-images.githubusercontent.com/22596587/189736392-123c4490-a0fe-4583-bbb8-3a701febacb0.png)

## Debug Haskell File
This opens up a debugging tool, that can be used to get an idea of what is going on in your Haskell program. It is possible to set breakpoints as well as take "steps" for each line call. It also provides information on the variables in use for a given line, as well as evaluate what the variables values are. There are also a history trace (somewhat like a stack trace) that can show you what lines where called.

![image](https://user-images.githubusercontent.com/22596587/189736547-42063da3-2cfd-43f0-b7c2-80d663795539.png)

# Haskell Interactive Window
This opens a sessions of GHCi with your '.hs' file loaded into, where you can then manually interact with the file.
