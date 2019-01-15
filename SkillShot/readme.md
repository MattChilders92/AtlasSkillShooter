Matt Childers   1/10/19
      
This code is the very definition of messy purpose built spaghetti code and 
should not be viewed or referenced by anyone for anything.
Nothing more than proof on concept code that happens to work sometimes
      
Planned structure:
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
              
Enjoy
    
    
    <img src="http://yuml.me/diagram/class/[note: You can stick notes on diagrams too!{bg:cornsilk}],[Customer]<>1-orders 0..*>[Order], [Order]++*-*>[LineItem], [Order]-1>[DeliveryMethod], [Order]*-*>[Product], [Category]<->[Product], [DeliveryMethod]^[National], [DeliveryMethod]^[International]" >
