# Economic simulation

## Demo
[There is very early browser demo of this game](http://nashet.github.io/EconomicSimulation/WEBGL/index.html) (6MB download, some mobile devices are not supported)
And there is standalone windows version in [releases](https://github.com/Nashet/EconomicSimulation/releases), which runs faster

## Description
Several years ago I made [Economy analyzer for PDS's Victoria 2 game](https://github.com/aekrylov/vic2_economy_analyzer) (currently updating by @aekrylov)

Making that tool made me disappointed in how Victoria 2 economy is made. Since I didn't find better game-like economy simulation,  I decided to try to make my own.
So, main principles of that simulation are:
* free market agent based economy simulation (currently with one currency - gold)
* no ridiculous x5 price limits (except 0.001 and 999), allowing realistic inflation
* population consume goods not in fixed order but - at first cheap goods, then expensive
* factories can compete for workforce by changeable salary
* factories have specific owner like government or population unit
* capitalists can take loans form national bank for business expansion 
* governments can put extra money in bank
* player should have as much freedom as possible, even if player is going to do something stupid 

## What it has now (v0.12.1)
 - provinces and countries
 - factories and national banks
 - population agents (Aristocrats, Capitalists, Farmers, Soldiers, Workers, Tribesmen)
 - basic production\trade\consumption
 - basic warfare
 - basic inventions
 - basic reforms (population can vote for reforms)
 - population demotion\promotion to other classes
 - migration\immigration\assimilation
 - political\culture\core map mode
 - basic diplomacy (only relations for now)
 - [![Bugs](https://badge.waffle.io/Nashet/EconomicSimulation.png?label=bug&title=bugs)](https://waffle.io/Nashet/EconomicSimulation) 

Map is generated randomly, you play as some country yet there is no much gameplay for now. You can try to growth economy or conquer the world.           

## Current targets
 - more gameplay
 - diplomacy
 - perfomance

## Screenshots
![map](http://i.imgrpost.com/imgr/2017/06/22/VYAaereVAVArdsdVA.png)
![diplomacy](http://i.imgrpost.com/imgr/2017/06/22/VYAaersderdsdVA.png)
![Imgur](http://i.imgur.com/KevTH51.png)
![Imgur](http://i.imgur.com/uzEJCvM.png)

## Contributing
I would love to have other people providing ideas, code or questions.  You may:
- Create Tickets on the tracker - https://waffle.io/Nashet/EconomicSimulation
- Open a Pull Request and I will check it

License is GPL-3.0

## How to build
Project is build with Unity 5.4.2f2 - Unity 5.6.1f1 (5.4.2f2 will brake UI positions). Just add as project and open "Base" scene
