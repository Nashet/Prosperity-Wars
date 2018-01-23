# Economic simulation

## Demo
[Project now has patreon page](https://www.patreon.com/economicsimulation) where you can help contributing in this project

[There is early browser demo of this game](http://nashet.github.io/EconomicSimulation/WEBGL/index.html) (about 6MB download, some devices are not supported)

And there is standalone windows version in [releases](https://github.com/Nashet/EconomicSimulation/releases), which runs faster

## Description
I'm a huge fan of Victoria 2 game but I was disappointed in how Victoria 2 economy is made. Since I didn't find better game-like economy simulation, I decided to try to make my own.
Main principles of that simulation are:
* agent based simulation with real population units, fabrics, countries
* prices determined by supply / demand balance (currently with one market and one currency - gold)
* no price limits (except 0.001 and 999), allowing realistic inflation
* population consume goods not in fixed order but - at first cheap goods, then expensive
* factories can compete for workforce by changeable salary
* factories have specific owner like government or population unit
* capitalists can take loans form national bank for business expansion 
* governments can put extra money in bank
* governments can compete by wars

## What it has now (v0.16.2)
 - provinces and countries (generated randomly)
 - movements and Rebellions
 - factories and national banks
 - population agents (Aristocrats, Capitalists, Farmers, Soldiers, Workers, Tribesmen, Artisans)
 - basic production\free market trade\consumption
 - basic warfare
 - basic inventions
 - basic reforms (population can vote for reforms)
 - population demotion\promotion to other classes
 - migration\immigration\assimilation
 - political\culture\core\resource map mode
 - basic diplomacy (only relations for now)
 - [substitute products](https://github.com/Nashet/EconomicSimulation/wiki/Products)
 - [planned economy!](https://github.com/Nashet/EconomicSimulation/wiki/Economy-types#Planned_economy)
 - [![Bugs](https://badge.waffle.io/Nashet/EconomicSimulation.svg?columns=all)](https://waffle.io/Nashet/EconomicSimulation) 

Map is generated randomly, you play as some country yet there is no much gameplay for now. You can try to growth economy or conquer the world.

## Screenshots
![map](http://i.imgrpost.com/imgr/2017/08/14/VYAaererrerdsdVA.png)
![map](http://i.imgrpost.com/imgr/2017/08/14/VYsdffAaererdsdVA.png)
![map](http://i.imgrpost.com/imgr/2017/06/22/VYAaereVAVArdsdVA.png)
![diplomacy](http://i.imgrpost.com/imgr/2017/06/22/VYAaersderdsdVA.png)
![panels](http://i.imgrpost.com/imgr/2017/08/14/ES-14.png)
![Imgur](http://i.imgur.com/KevTH51.png)
![Imgur](http://i.imgur.com/uzEJCvM.png)

## Contributing
I would love to have other people providing ideas, code or questions.  You may:
- Create Tickets on the tracker - https://waffle.io/Nashet/EconomicSimulation
- Open a Pull Request and I will check it

License is GPL-3.0

## How to build
Project is build with Unity 5.4.2f2 - Unity 5.6.1f1 (5.4.2f2 will brake UI positions). Just add as project and open "Base" scene
