### 12/10/25 - 19/10/25

- Continued Work on the Vehicle Suspension Models
- Started Work on the Vehicle Controller Methods

### 6/10/25 - 12/10/25

- Futher Experimentation into avenues to simulate vehicle suspension
- Experimenting with Mesh manipulation for the Car Controller
- Started on the Project Plan, taking into account any barriers I experienced while trying to build a basic suspension system
- Project Plan Completed
- Basic Raycast Suspension Completed

### 29/9/25 - 5/10/25

- Set up Unity Editor
- Experimented in Unity with Suspension Physics
- Experienced Problems in Unity 6 Regarding Lists, Downgraded to Unity 2022.3 LTS for enhanced stability

### Temporary Haitus

### 24/11/25 - 30/11/25

- Finally Implemented a Basic Raycast Suspension System to this Git Repository
- Added Damping relative to suspension compression / extension speed
- Started Work on a Vehicle Controller, with frictional forces to enhance vehicle stability from random sliding

### 1/12/25 - 7/12/25

- Enhanced Vehicle Controller Physics with tyre traction that varies based on the normal force exerted on the tyres by the weight of the car (F = uN)
- Tyre Slip Angle Physics are based on the rotation of the suspension empties, making modelling other vehicle features such as 4-wheel steering, AWD, etc far easier
- Meshes for the Wheels and Calipers are transformed using the height data from the Raycast Suspension System
- Wheel and Caliper Meshes also turn with the Steered Wheels

### 8/12/25 - 14/12/25

- Worked on Interim Report

### 16/02/26 - 22/03/26

- Researching Unity's ML agents and how to train the required models for my project

### 23/02/26 - 1/03/26

- Completed more research on Unity's ML Agents and how to implement it into my project
- Continued Debugging odd tyre grip issues, specifically tyre grip moving the vehicle instead of resisting movement of the vehicle

### 2/03/26 - 8/03/26

- Fixed Tyre Grip Model issues, which made progressing on the project finally possible
- Made changes to the vehicle controller to allow two scripts, one for player inputs and one for ML agent inputs
- Refactored some code to use unity native features instead of less efficient mathematical features
- Implemented a Basic Checkpoint System
- Refactored CarController.cs

### 9/03/26 - 15/03/26

- Built a Proof Of Concept ML Agent Controlled Vehicle
- Updated Car Controller With Engine Simulation and Gearing
- Worked On Final Report

### 16/03/26 - 23/03/26

- Updated Engine Simulation and Gearing
- Added HUD for the player
- Added Main Menu and Scene Switching
- Implement the New Unity Input System
- Build a Bootstrapper Script to Initialise Events in Unity Scenes
- Created a Lap Timer
- Added a Code Style Standard (CSharpier)
- Brought all code inline with CSharpier Standards
- Created a Plain C# Service Class to Handle all Car Calculations
- Implemented UTF Testing Framework
- Implemented a Position Tracker for races
- Worked On Final Report

### 30/03/26 - 05/04/26

- Added Improvements to CarController class
- Added Engine Sounds To Cars
- Worked On Final Report