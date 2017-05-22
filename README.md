[![Stories in Ready](https://badge.waffle.io/Nashet/EconomicSimulation.png?label=bug&title=bugs)](https://waffle.io/Nashet/EconomicSimulation) [![GitHub closed issues](https://img.shields.io/github/issues-closed-raw/Nashet/EconomicSimulation.svg?style=plastic)](https://waffle.io/Nashet/EconomicSimulation)
# Economic simulation
## Demo
[There is very early browser demo of this game](http://nashet.github.io/EconomicSimulation/WEBGL/index.html) (6MB download, some mobile devices are not supported)
## Description
Several years ago I made [Economy analyzer for PDS's Victoria 2 game](https://github.com/aekrylov/vic2_economy_analyzer) (currently updating by (https://github.com/aekrylov))

Making that tool made me disappointed in how Victoria 2 economy is made. Since I didn't find better game-like economy simulation,  I decided to try to make my own.
So, main principles of that simulation are:
* free market agent based economy simulation (currently with one currency - gold)
* no ridiculous x5 price limits ( except 0.001 and 999), allowing realistic inflation
* population consume goods not in fixed order but - at first cheap goods, then expensive
* factories can compete for workforce by changeable salary
* factories have specific owner like government or population unit
* capitalists could take loans form national bank for business expansion 
## What it has now (v10)
 - population agents
 - basic trade & production
 - basic warfare
 - basic inventions
 - basic reforms (voting is not implemented yet)"
 - population demotion\promotion to other classes
 - migration (inside country)
 - assimilation
 
Map is generated randomly, you play as some country yet there is no much gameplay for now. You can try to growth economy or conquer the world.           
## Screenshots
## Contributing
I would love to have other people providing both ideas and code.  You can either:
- Create Tickets on the tracker - https://waffle.io/Nashet/EconomicSimulation
- Open a Pull Request and I will check it
- Ask to be a Contributor and I'll add you
## How to build
Project is build with Unity 5.4.2f2 - Unity 5.6.1f1 (5.4.2f2 will brake UI positions). Just add as project and open "Base" scene
