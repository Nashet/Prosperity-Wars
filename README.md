[![Stories in Ready](https://badge.waffle.io/Nashet/EconomicSimulation.png?label=bug&title=Bugs)](https://waffle.io/Nashet/EconomicSimulation)
# Economic simulation

[There is very early browser demo of this game](http://nashet.github.io/EconomicSimulation/WEBGL/index.html)

Several years ago I made [Economy analyzer for PDS's Vicoria 2 game](https://github.com/aekrylov/vic2_economy_analyzer) (currently updating by (https://github.com/aekrylov))

Making that tool made me disappointed in how Victoria 2 economy made.

Since I didn't find better game-like economy simulation,  I decided to try to make my own.

So, main principles of that simulation are:
* free market agent based economy simulation (currently with one currency - gold)
* no ridiculous x5 price limits ( except obvious 0.001 and 999), allowing realistic inflation
* population consume goods not in fixed order but - at first cheap goods, then expensive
* factories can compete for workforce by changeable salary
* factories have specific owner like government or population unit
* capitalists could take loans form national bank for business expansion 

Project is build with Unity 5.4.2f2 and MS VS 2015
