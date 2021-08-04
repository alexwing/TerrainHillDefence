# Hill Defence


This game is a tower defence game. The player controls a tower which is placed on a terrain. The tower can shoot at enemies and deal damage to them.

## Requirements

Unity 2019.4.1f1

## Controls


- WASD : basic movement
- SHIFT : Makes camera accelerate
- Mouse : Mouse look
- Scroll wheel : Height over terrain
- mouse click right: set lock/unlock movement 
- mouse click left: Placing the tower on the terrain (TO DO)
  
## Implemented

* Team flags are randomly generated on the ground, through an algorithm, at a minimum distance from each other.

* The colours of each team are generated through an array of colours. This is applied to the flag and the soldiers.

* The nearest flag is set as the enemy of each team.

* Soldiers are generated around the flag, taking into account that there is a minimum distance between them.

* The soldiers aim to destroy the enemy flag, but if they encounter enemies in their attack range, they will try to destroy them.

* Soldiers and flags have a number of hits to be destroyed.

## TO DO

* Place turrets to defend a flag.

* Improve the movement of soldiers to avoid colliding with turrets.

* Improve the soldiers AI, take into account the soldiers of other teams, or who are under attack, as well as defend the flag when it is under attack.

* Show the percentage of available health for each soldier.

* When a base is eliminated the team must go for the next one.

* Create a UI with information about the game and the winner.

* Improve the initial performance and find enemy performance

## Code description

- SceneConfig: static class with general game parameters.

- FlyCamera controller for the game camera and interactivity.

- HillDefenceCreator: Main class in charge of generating flags and soldiers.

- TeamSoldier Each soldier has this class which is in charge of searching for enemies, firing and damage assessment.

- TargetTerrain: Generate the hills that cover the target of the flag. It receives collisions from soldiers' shots and modifies the terrain and generates explosions.

  