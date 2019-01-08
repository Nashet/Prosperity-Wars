# Prosperity Wars game

## Demo
There is an early browser demo of this game located [**here**](https://nashet.github.io/Prosperity-Wars/WEBGL/index.html)
It is roughly 6MB in size, some devices are not supported.

Currently development is paused. Maybe someday I'll have time to implement it in better form, with better graphics and real business plan.

There is also a standalone windows edition under [releases](https://github.com/Nashet/Prosperity-Wars/releases) which runs faster.

## Description
I'm a huge fan of the game Victoria 2, but I was disappointed in the economy mechanics of the game. Unable to find a strategy simulation game with a better economy, I decided to make my own.
Main simulation principles in PosperityWars include:
* Agent based simulation with real population units, fabrics, countries
* Prices determined by supply / demand balance (currently with one market and one currency - gold)
* No price limits (except 0.001 and 999), allowing realistic inflation
* Population consume goods ordered by price from cheap to expensive (making more demand for cheaper goods)
* Factories can compete for workforce by competitive wages
* Factories have specified owners like the government or population unit
* Capitalists can take loans form national bank for business expansion 
* Governments can put extra money in bank
* Governments can compete by wars

## What it has now (v0.20.6)
 - Provinces and Countries (generated randomly OR generated from image file)
 - Movements and Rebellions
 - Factories and National banks
 - Population agents (Aristocrats, Capitalists, Farmers, Soldiers, Workers, Tribesmen, Artisans)
 - Basic production\free market trade\consumption
 - Basic warfare
 - Basic inventions
 - Basic reforms (population can vote for reforms)
 - Population demotion\promotion to other classes
 - Migration\immigration\assimilation
 - Political\culture\core\resource\population\prosperity map mode
 - Basic diplomacy (only relations for now)
 - [Substitute products](https://github.com/Nashet/Prosperity-Wars/wiki/Products)
 - [Planned economy!](https://github.com/Nashet/Prosperity-Wars/wiki/Economy-types#Planned_economy)
 - [![Bugs](https://badge.waffle.io/Nashet/Prosperity-Wars.svg?columns=all)](https://waffle.io/Nashet/Prosperity-Wars) [![Codacy Badge](https://api.codacy.com/project/badge/Grade/32a3f6b804334fc1bdb2cea878329a77)](https://www.codacy.com/project/Nashet/Prosperity-Wars/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=Nashet/Prosperity-Wars&amp;utm_campaign=Badge_Grade_Dashboard)

Maps are generated randomly, you play as a leader of a country. For now you are able to grow the prosperity or try to conquer the world.

## Screenshots
![map](http://i.imgrpost.com/imgr/2017/08/14/VYAaererrerdsdVA.png)
![map](http://i.imgrpost.com/imgr/2017/08/14/VYsdffAaererdsdVA.png)
![map](http://i.imgrpost.com/imgr/2017/06/22/VYAaereVAVArdsdVA.png)

![Imgur](https://i.imgur.com/ir7pqgV.png)
![Imgur](https://i.imgur.com/U85ZjSV.png)
![Imgur](https://i.imgur.com/knumBN0.png)
![Imgur](https://i.imgur.com/goICtvL.png)
![Imgur](https://i.imgur.com/6YLDnnq.png)

## Contributing
I would appreciate any assitance in providing ideas, code or feedback.  You may:
- Create Tickets on the tracker - https://waffle.io/Nashet/Prosperity-Wars
- Open a Pull Request and I will check it.

License is GPL-3.0

## How to build
Since 0.19.2 version the project is built with Unity 2017.40f1 (5.4.2f2 will brake UI positions). Just add as project and open "Base" scene. Unity 2018 doesn't build WebGL for that project so far. Game's version 0.20.7 causes errors in WebGL.
