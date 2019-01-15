Matt Childers   1/10/19
      
This code is the very definition of messy purpose built spaghetti code and 
should not be viewed or referenced by anyone for anything.
Nothing more than proof on concept code that happens to work sometimes
      
Planned structure:

  ```ditaa {cmd=true args=["-E"]}
    Form  <--- Core  <---->  State <---> AppConfig
                ^  ^
                |  |
                |  --------------->DecisionMaker 
                |                       |
                V                       V
    AtlasInterface                MouseInterface
        ^             Timer              |
        |               |                |
        |               V                V
    Locator <--  ScreenInterface <---  GAME
  ```      
Enjoy
