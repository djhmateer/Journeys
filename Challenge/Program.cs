using System;
using System.Collections.Generic;

namespace Challenge
{
    public class Program
    {
        // Based on the challenge defined by Mike Hadlow
        // https://github.com/mikehadlow/Journeys
        // another implementation by Mark Seemann
        // https://blog.ploeh.dk/2019/10/28/a-basic-haskell-solution-to-the-robot-journeys-coding-exercise/

        static void Main()
        {
            while (true)
            {
                Console.WriteLine("Welcome to the Robot Journey (x to exit)\n");
                const string robot1 = "5 3\n1 1 E\nRFRFRFRF";
                const string robot2 = "5 3\n3 2 N\nFRRFLLFFRRFLL";
                const string robot3 = "0 3 W\nLLFFFLFLFL";
                Console.WriteLine($"\n1 for robot1:\n{robot1}");
                Console.WriteLine($"\n2 for robot2:\n{robot2}");
                Console.WriteLine($"\n3 for robot2 and robot3:\n{robot2}\n{robot3}");

                var keypress = Console.ReadKey().Key;
                var list = new List<string>();
                if (keypress == ConsoleKey.X) break;
                if (keypress == ConsoleKey.D1) list.Add(robot1);
                if (keypress == ConsoleKey.D2) list.Add(robot2);
                if (keypress == ConsoleKey.D3) list.AddRange(new[] { robot2, robot3 });
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n\n Output: {Run(list.ToArray())} \n\n");
                Console.ResetColor();
            }
        }

        // Runs the engine - accepts input and returns output
        // accepts an array of strings which are robots:
        // gridMaxX, gridMaxY
        // x,y, orientation
        // instructions
        // eg 5 3\n1 1 E\nRFRFRFRF
        public static string Run(string[] inputs)
        {
            // If there are multiple robtos the grid is defined only by the first ship
            // currentPosX, Y, currentOrientation, isLost are defined here as need to be returned from this method
            int gridMaxX = 0, gridMaxY = 0, currentPosX = 0, currentPosY = 0;
            string currentOrientation = null;
            var isLost = false;
            // warningCoordsList needs to be passed between robots eg see robot2 and robot3 test
            var warningCoordsList = new List<(int x, int y, string direction)>();

            // Loop over each robot 
            for (var robot = 0; robot < inputs.Length; robot++)
            {
                // split input lines based on newline character \n
                var lines = inputs[robot].Split("\n");

                // If multiple robots then only the first includes the grid size on the first line
                var gridSizeLine = 0;
                var startPositionLine = 1;
                var instructionPositionLine = 2;

                // If it is the first robot assume gridsize is on first line
                if (robot == 0)
                {
                    // make the grid based on the the first line
                    var gridSizeStringArray = lines[gridSizeLine].Split(' '); // eg 5 3  assuming a single space
                    gridMaxX = int.Parse(gridSizeStringArray[0]); // eg 5 
                    gridMaxY = int.Parse(gridSizeStringArray[1]); // eg 3
                    if (gridMaxX > 50 || gridMaxY > 50) throw new ArgumentException("Max grid size is 50");
                }
                else
                // Not the first robot so first line will be startPositionLine.. 
                {
                    startPositionLine--;
                    instructionPositionLine--;
                }

                // Starting position of the robot
                var startPosStringArray = lines[startPositionLine].Split(' '); //eg 1 1 E
                var startPosX = int.Parse(startPosStringArray[0]); // eg 1
                var startPosY = int.Parse(startPosStringArray[1]); // eg 1
                if (startPosX > 50 || startPosY > 50)
                    throw new ArgumentException("StartPositions should less or equal to 50");
                var startPosOrientation = startPosStringArray[2]; // eg E

                // Instructions
                var instructionsString = lines[instructionPositionLine];
                if (instructionsString.Length >= 100)
                    throw new ArgumentException("Instructions should be less than 100");

                // Set current position and orientation of robot (starting position)
                currentPosX = startPosX;
                currentPosY = startPosY;
                currentOrientation = startPosOrientation;
                isLost = false;
                // Iterate over each instruction
                foreach (char c in instructionsString)
                {
                    var instruction = c.ToString();
                    if (instruction == "L" || instruction == "R")
                        currentOrientation = Rotate(currentOrientation, instruction);
                    if (instruction == "F")
                    {
                        (currentPosX, currentPosY, isLost) = Move(currentPosX, currentPosY, currentOrientation, gridMaxX, gridMaxY,
                            warningCoordsList);
                    }

                    if (isLost)
                    {
                        // Add to the warningCoordsList for next robot if there is one
                        warningCoordsList.Add((currentPosX, currentPosY, currentOrientation));
                        break; // Gone off the grid and robot is lost
                    }
                }
            }
            return currentPosX + " " + currentPosY + " " + currentOrientation + (isLost ? " LOST" : null);
        }

        public static (int x, int y, bool isLost) Move(int x, int y, string currentOrientation, int gridMaxX, int gridMaxY,
            List<(int, int, string)> warningCoordsList)
        {
            // Make an empty list if no warningCoordsList to make easier below
            if (warningCoordsList == null) warningCoordsList = new List<(int, int, string)>();
            var isLost = false;

            if (currentOrientation == "N")
            {
                // Will this go out of bounds?
                if (y + 1 > gridMaxY)
                    if (warningCoordsList.Contains((x, y, "N"))) { }
                    else
                        isLost = true;
                else
                    y++;
            }

            if (currentOrientation == "S")
            {
                if (y - 1 < 0)
                    if (warningCoordsList.Contains((x, y, "S"))) { }
                    else
                        isLost = true;
                else
                    y--;
            }

            if (currentOrientation == "E")
            {
                if (x + 1 > gridMaxX)
                    if (warningCoordsList.Contains((x, y, "E"))) { }
                    else
                        isLost = true;
                else
                    x++;
            }

            if (currentOrientation == "W")
            {
                if (x - 1 < 0)
                    if (warningCoordsList.Contains((x, y, "W"))) { }
                    else
                        isLost = true;
                else
                    x--;
            }

            return (x, y, isLost);
        }

        public static string Rotate(string current, string direction)
        {
            var compass = "NESW";
            var index = compass.IndexOf(current); // eg 1 is East

            if (direction == "R") index++; else index--;

            if (index == 4) index = 0; // from W to N (rotate Right)
            if (index == -1) index = 3; // from N to W (rotate Left) 

            return compass[index].ToString();
        }
    }
}
