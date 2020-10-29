using FinchAPI;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Speech;
using System.Linq;
using System.Timers;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Timer = System.Timers.Timer;
using System.Runtime.InteropServices.ComTypes;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Collections;

namespace Finch_Starter
{
    class Program
    {
        // *************************************************************
        // Application:     Finch Starter Solution
        // Author:          Velis, John E
        // Description:
        // Date Created:    5/20/2016
        // Date Revised:    
        // *************************************************************
        //Application Additions Talent Show, Sensors, and a few others
        //Author: Sam G.
        //Description
        //Date Edited addition.10/1/2020
        //***************************************************************
        static void Main(string[] args)
        {
            //
            // create a new Finch object
            //
            Finch myFinch;
            myFinch = new Finch();
            bool endNow = false;
            string caseSwitch;

            //
            // call the connect method
            //
            TestFinchConnection(myFinch);

            //
            // begin your code
            //

            //color change test
            myFinch.setLED(0, 0, 255);
            myFinch.wait(5000);
            myFinch.setLED(0, 255, 0);
            myFinch.wait(5000);
            myFinch.setLED(255, 0, 0);
            //run set up and explanation functions
            SetUpDisplay();
            DisplayWelcomeMessage();
            do
            {
                DisplayMainMenu();
                caseSwitch = GatherInput();
                switch (caseSwitch)
                {
                    case "a":
                        Console.Clear();
                        TalentShow(myFinch);
                        break;
                    case "b":
                        Console.Clear();
                        AngryFinch(myFinch);
                        Console.Clear();
                        break;
                    case "c":
                        Console.Clear();
                        DisplayDataRecorder(myFinch);
                        break;
                    case "d":
                        Console.Clear();
                        DisplayLightDataRecorder(myFinch);
                        break;
                    case "e":
                        DisplaySentryMode(myFinch);
                        Console.Clear();
                        break;
                    case "quit":
                        Console.WriteLine("Thank you for using our robot!");
                        Thread.Sleep(5000);
                        endNow = true;
                        break;
                }
            } while (!endNow);
            #region testCode
            /**myFinch.setLED(0, 0, 255);
            myFinch.wait(1000);
            myFinch.setLED(0, 255, 0);
            myFinch.wait(1000);*/

            /**for (int i = 0; i<5; i++)
            {
                myFinch.setLED(0, 0, 255);
                myFinch.wait(100);
                myFinch.setLED(0, 255, 0);
                myFinch.wait(100);
                myFinch.setLED(255, 0, 0);
                myFinch.wait(100);
            }
            
            for (int  i = 0; i<255; i++)
            {
                myFinch.setLED(0, 0, i);
                myFinch.wait(100);
                myFinch.setLED(i, 0, 0);
                myFinch.wait(100);
            }

            for (int i = 255; i >0; i--)
            {
                myFinch.setLED(0, 0, i);
                myFinch.wait(100);
                myFinch.setLED(i, 0, 0);
                myFinch.wait(100);
            }
            
            for (int i = 0; i < 5; i++)
            {
                myFinch.noteOn(261);
                myFinch.wait(1000);
                myFinch.noteOff();
                myFinch.wait(100);
            }
            myFinch.setMotors(-255, 255);
            myFinch.wait(10000);
            */
            #endregion
            //
            //end of your code
            //

            //
            // call the disconnect method
            //
            myFinch.disConnect();
        }

        static void DisplaySentryMode(Finch myFinch)
        {
            DisplaySentryMenu();
            string menuOption, sensorType;
            int dataFrequency = 1;
            double secondsOfOperation = 60;
            double[] minimumAndMaximum = null;
            sensorType = null;
            bool endNow = false;
            Tuple<int[], ArrayList, ArrayList> sentryData =null;
            do
            {
                menuOption = GatherInput().ToLower();
                switch (menuOption)
                {
                    case "a":
                        Console.Clear();
                        dataFrequency = LightDataRecorderDisplayGetNumberOfDataPoints();
                        break;
                    case "b":
                        Console.Clear();
                        sensorType = SelectSensorType();
                        break;
                    case "c":
                        Console.Clear();
                        minimumAndMaximum = GetMinimumMaximumRange(sensorType);
                        break;
                    case "d":
                        secondsOfOperation = GetSecondsSentryIsActive();
                        break;
                    case "e":
                        sentryData = SentryModeActivate(dataFrequency, sensorType, minimumAndMaximum, secondsOfOperation, myFinch);
                        break;
                    case "f":
                        DisplayAlertTripped(sentryData, sensorType);
                        break;
                    case "quit":
                        Console.WriteLine("Sentry arming down.");
                        Thread.Sleep(3000);
                        endNow = true;
                        break;
                }
            } while (!endNow);
        }

        private static void DisplayAlertTripped(Tuple<int[], ArrayList, ArrayList> sentryData, string sentryType)
        {
            DisplayHeader($"Display the {sentryType} alerted tripped.");
            for (int i = 0; i < sentryData.Item1.Length; i++)
            {
                Console.WriteLine($"Points tripped: {sentryData.Item1[i]}, Data: {sentryData.Item2[i]}, {sentryData.Item3[i]}");
            }
        }

        private static System.Timers.Timer aTimer;
        //
        //This is where the sentry will be activated to alert the user.
        //
        private static Tuple<int[],ArrayList, ArrayList> SentryModeActivate(int dataFrequency, string sensorType, double[] minimumAndMaximum, double secondsOfOperation, Finch myFinch)
        {
            ArrayList leftArray = new ArrayList();
            ArrayList rightArray = new ArrayList();
            int countStepped=0;
            double holdRight;
            double holdLeft;
            double temperatureHold;
            double temperatureF;
            int[] alertTrippedIndex = new int[(int)(secondsOfOperation)];
            Timer r = new System.Timers.Timer(secondsOfOperation*1000);
            r.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            r.Enabled = true;
            bool running = true;
            int i = 0;
            //
            //This system will gather when alerts go off.
            //
            while(running)
            {
                int a = 0;
                i++;
                Console.WriteLine($"Test {i}");
                Thread.Sleep(dataFrequency*1000);
                if (sensorType == "light")
                {
                    //
                    //When Light is selected we will get the light data from the left and right sensors and within the light range
                    //
                    holdLeft = myFinch.getLeftLightSensor();
                    holdRight = myFinch.getRightLightSensor();
                    if (holdLeft < minimumAndMaximum[0] && holdRight < minimumAndMaximum[0] && holdLeft > minimumAndMaximum[1] && holdRight > minimumAndMaximum[1])
                    {
                        countStepped = i;
                        leftArray.Add(holdLeft);
                        rightArray.Add(holdRight);
                        alertTrippedIndex[a] = countStepped;

                        Console.WriteLine("ALERT!");
                    }
                }
                //
                //When temperature is selected we will get the temperature data in both F and C
                //
                if (sensorType == "temperature")
                    {
                    temperatureHold = myFinch.getTemperature();
                    if (temperatureHold < minimumAndMaximum[0] && temperatureHold < minimumAndMaximum[1])
                    {
                        countStepped = i;
                        temperatureF = (temperatureHold * 9 / 5) + 32;

                        leftArray.Add(temperatureF);
                        rightArray.Add(temperatureHold);
                        alertTrippedIndex[a] = countStepped;
                        a++;
                        Console.WriteLine("ALERT!");
                    }
                }

            }
            r.Enabled = false;
            //
            //The timer off switch.
            //
            void timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                running = false;
            }
            Tuple<int[], ArrayList, ArrayList> sentryResults = new Tuple<int[], ArrayList, ArrayList>(alertTrippedIndex, leftArray, rightArray);
            return sentryResults; 

        }

        private static double GetSecondsSentryIsActive()
        {
            double secondsEntered;
            string userResponse;
            bool endNow = false;
            Console.WriteLine("Please enter the amount of seconds you wish to input.");
            userResponse = Console.ReadLine();
            do
            {
                if (double.TryParse(userResponse, out secondsEntered))
                {
                    endNow = true;
                }
                else
                {
                    Console.WriteLine("ERROR: Please enter proper response.");
                }
            } while (!endNow);
            return secondsEntered;
        }

        static double[] GetMinimumMaximumRange(string sensorType)
        {
            double[] getMaxMin;
            double listValue;
            string userResponse;
            bool endNow = false;
            int i = 0;
            int falseFlags = 0;
            DisplayHeader("Get Minimum and Maximum Range.");
            Console.WriteLine("Please enter in maximum.");
            getMaxMin = new double[2];
            do
            {
                userResponse = GatherInput();
                if (double.TryParse(userResponse, out listValue))
                {
                    getMaxMin[i] =listValue;
                    i++;
                }
                if(!double.TryParse(userResponse, out listValue))
                {
                    Console.WriteLine("ERROR: This statement is not a number.");
                    Console.WriteLine("Please enter in a true value.");
                    falseFlags++;
                }
                if (i == 2)
                {
                    endNow = true;
                }
                if(falseFlags >= 4)
                {
                    endNow = true;
                    Console.WriteLine("Error, max attempts reached.");
                    Console.WriteLine($"Setting based levels for {sensorType}.");
                    if(sensorType == "light")
                    {
                        getMaxMin[0] = 10;
                        getMaxMin[1] = 30;
                    }
                    if(sensorType == "temperature")
                    {
                        getMaxMin[0] = 30;
                        getMaxMin[1] = 70;
                    }
                }
            } while (!endNow);

            return getMaxMin;
        }

        //
        //Use inputs what type of sensor they want the robot to use.
        //
        static string SelectSensorType()
        {
            DisplayHeader("Sensor Selection");
            string sensorSelection;
            bool properChoice = false;
            Console.WriteLine("Please enter in an option of sensor selection.[light/temperature]");
            do
            {
                sensorSelection = GatherInput();
                if(sensorSelection == "light"|| sensorSelection == "temperature")
                {
                    properChoice = true;
                }
                else
                {
                    Console.WriteLine("ERROR: Incorrect option selected.");
                    Thread.Sleep(2000);
                    Console.WriteLine("Please enter in proper option.\n Please enter proper option.");
                }
            } while (!properChoice);
            return sensorSelection;
        }


        private static void TestFinchConnection(Finch myFinch)
        {
            do
            {
                Console.WriteLine("Wait a minute...");
                Thread.Sleep(2000);
                Console.WriteLine("Why am I not awake?");
                Console.WriteLine("Please check connection plz then hit any key to continue my boot up.");
                Console.ReadKey();
            } while (!myFinch.connect());
            if (myFinch.connect() == true)
            {
                Console.WriteLine("Wait....that was meant for later.");
                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine(".");
                    Thread.Sleep(1000);
                }
                Console.WriteLine("Sorry lets continue to main menu.");
                Thread.Sleep(5000);
                Console.Clear();
            }

        }
        #region RANDOM FUNCTIONS
        //
        //Finch is angry
        //
        private static void AngryFinch(Finch myFinch)
        {
            myFinch.setLED(255, 0, 0);
            myFinch.noteOn(500);
            myFinch.wait(1000);
            myFinch.noteOff();
            myFinch.setLED(0, 0, 255);
        }
        #endregion


        #region TALENT SHOW
        //
        //Talent Show menu selection
        //
        private static void TalentShow(Finch myFinch)
        {
            string caseSwitch;
            bool endNowSequal;
            do
            {
                endNowSequal = false;
                DisplayTalentShow();
                Console.WriteLine("Please select an option.");
                caseSwitch = Console.ReadLine();
                switch (caseSwitch)
                {
                    case "a":
                        RoboticSinging(myFinch);
                        break;
                    case "b":
                        RoboticDancing(myFinch);
                        break;
                    case "c":
                        RoboticSCREAMING(myFinch);
                        break;
                    case "d":
                        RobotLightShow(myFinch);
                        break;
                    case "quit":
                        endNowSequal = true;
                        break;
                }
            } while (!endNowSequal);
        }
        //
        //Light show where the robot switches between the colors after glowing them brighter and brighter
        //
        private static void RobotLightShow(Finch myFinch)
        {
            myFinch.setLED(0, 0, 0);
            for (int i = 0; i < 255;)
            {
                i += 10;
                if (i > 255)
                {
                    i = 255;
                }
                myFinch.setLED(0, i, 0);
                myFinch.wait(1000);
                myFinch.setLED(i, 0, 0);
                myFinch.wait(1000);
                myFinch.setLED(0, 0, i);
                myFinch.wait(1000);
            }
        }
        //
        //Robot begins playing a series of notes
        //
        private static void RoboticSinging(Finch myFinch)
        {
            myFinch.setLED(255, 0, 0);
            myFinch.noteOn(233);
            myFinch.wait(2000);
            myFinch.noteOff();
            myFinch.wait(10);
            myFinch.noteOn(233);
            myFinch.wait(2000);
            myFinch.noteOff();
        }

        //
        //Finch begins dancing around a simple series of movements
        //
        private static void RoboticDancing(Finch myFinch)
        {
            for (int i = 0; i < 5; i++)
            {
                myFinch.setMotors(255, -255);
                myFinch.wait(1000);
                myFinch.setMotors(100, 100);
                myFinch.wait(3000);
                myFinch.setMotors(-255, -255);
                myFinch.wait(6000);
                myFinch.setMotors(255, -255);
                myFinch.wait(4000);
                myFinch.setMotors(-255, -255);
                myFinch.wait(2000);
                myFinch.setMotors(0, 0);
            }
        }

        //
        //Finch has no mind and must scream
        //
        private static void RoboticSCREAMING(Finch myFinch)
        {
            bool levelTrue = false;
            do
            {
                for (int i = 0; i <= 255; i++)
                {
                    myFinch.noteOn(1500 + i);
                    myFinch.setLED(0, 0 + i, 0);
                    if (i == 255)
                    {
                        levelTrue = true;
                        myFinch.noteOff();
                    }
                }
            }
            while (!levelTrue);
            myFinch.noteOff();
        }
        #endregion

        #region LIGHT DATA RECORDER
        private static void DisplayLightDataRecorder(Finch myFinch)
        {
            string caseSwitch;
            bool endNowData = false;
            double numberHold = 0;
            double[] leftLightData, rightLightData, averageLightData;
            int numberOfDataPoints = 0;
            double frequencyOfDataPointsSeconds = 0;
            Tuple<double[], double[]> lightData = null;
            leftLightData = null;
            rightLightData = null;
            averageLightData = null;

            do
            {
                DisplayHeader("Data Recorder Menu");


                Console.WriteLine("\ta) Get number of data points.");
                Console.WriteLine("\tb) Get the freuquency of data points");
                Console.WriteLine("\tc) Get Light Measurements of data points.");
                Console.WriteLine("\td) Display Table of Light");
                Console.WriteLine("\te) ");
                Console.WriteLine("\t\tMain Menu");
                Console.WriteLine("Please select an option.");
                caseSwitch = Console.ReadLine();
                switch (caseSwitch)
                {
                    case "a":
                        numberOfDataPoints = LightDataRecorderDisplayGetNumberOfDataPoints();
                        Console.ReadKey();
                        Console.Clear();
                        break;

                    case "b":
                        frequencyOfDataPointsSeconds = LightDataRecorderDisplayGetFrequencyOfDataPoints();
                        Console.ReadKey();
                        Console.Clear();
                        break;

                    case "c":
                        if (numberOfDataPoints == 0 || frequencyOfDataPointsSeconds == 0)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Please enter the number and frequency");
                        }
                        else
                        {
                            lightData = LightDataRecorderDisplayGetFrequencyOfDataSet(numberOfDataPoints, frequencyOfDataPointsSeconds, myFinch);
                        }
                        leftLightData = lightData.Item1;
                        rightLightData = lightData.Item2;
                        averageLightData = new double[leftLightData.Length];
                        for (int i = 0; i < leftLightData.Length; i++)
                        {
                            numberHold = (leftLightData[i] + rightLightData[i]) / 2;
                            averageLightData.SetValue(numberHold, i);
                        }
                        Console.ReadKey();
                        Console.Clear();
                        break;
                    case "d":
                        LightDataRecorderDisplayGetFrequencyOfDataSet(leftLightData, rightLightData, averageLightData);
                        Console.ReadKey();
                        Console.Clear();
                        break;

                    case "quit":
                        Console.WriteLine("Back to main menu.");
                        endNowData = true;
                        break;
                }
            } while (!endNowData);
        }



        //Data Recorder >Display data table
        static void LightDataRecorderDisplayDataTable(double[] leftLightData, double[] rightLightData, double[] averageLightData)
        {
            Console.WriteLine(
                "Reading #".PadLeft(15) +
                "Light Sensor Left".PadLeft(15) + "Light Sensor Right".PadLeft(15) +
                "Average: ".PadLeft(15)
        );
            for (int i = 0; i < leftLightData.Length; i++)
            {
                Console.WriteLine(
                    (i + 1).ToString().PadLeft(15) +
                   leftLightData[i].ToString("n2").PadLeft(15) + rightLightData[i].ToString("n2").PadLeft(15) +
                   averageLightData[i].ToString("n2").PadLeft(15)
                    );
            }
        }
        static void LightDataRecorderDisplayGetFrequencyOfDataSet(double[] leftLightData, double[] rightLightData, double[] averageLightData)
        {
            DisplayHeader("Data Set");
            LightDataRecorderDisplayDataTable(leftLightData, rightLightData, averageLightData);
        }



        /// <summary>
        /// Data Recorder > Get the Data Points
        /// </summary>
        /// <param name="numberOfDataPoints"></param>
        /// <param name="frequencyOfDataPointsSeconds"></param>
        /// <param name="myFinch"></param>
        /// <returns></returns>
        static Tuple<double[], double[]> LightDataRecorderDisplayGetFrequencyOfDataSet(int numberOfDataPoints, double frequencyOfDataPointsSeconds, Finch myFinch)
        {
            double[] lightReadingRight = new double[numberOfDataPoints];
            double[] lightReadingLeft = new double[numberOfDataPoints];
            DisplayHeader("Get Data Set");

            Console.WriteLine($"\tNumber of Data Points: {numberOfDataPoints}");
            Console.WriteLine($"\tFrequency of Data Points: {frequencyOfDataPointsSeconds}");
            Console.WriteLine();

            Console.WriteLine("\tFinch robot is ready to record light measurement data.");
            Console.WriteLine("\tPress any key to begin.");
            Console.ReadKey();

            double lightLeft, lightRight;
            int waitVariable;
            for (int index = 0; index < numberOfDataPoints; index++)
            {
                lightLeft = myFinch.getLeftLightSensor();
                lightRight = myFinch.getRightLightSensor();

                Console.WriteLine($"Left light reading at {index + 1}: {lightLeft}");
                Console.WriteLine($"Right light reading at {index + 1}: {lightRight}");
                lightReadingLeft[index] = lightLeft;
                lightReadingRight[index] = lightRight;
                waitVariable = (int)(frequencyOfDataPointsSeconds * 1000);
                myFinch.wait(waitVariable);
            }
            Tuple<double[], double[]> lightReadingVarable = new Tuple<double[], double[]>(lightReadingLeft, lightReadingRight);
            return lightReadingVarable;
        }

        /// <summary>
        /// Data Recorder > Get the Frequency of Data Points
        /// </summary>
        /// <returns></returns>
        static double LightDataRecorderDisplayGetFrequencyOfDataPoints()
        {
            double frequencyOfDataPoints;

            DisplayHeader("Frequency of Data Points");

            Console.Write("Enter the Frequency of Data Points");

            double.TryParse(Console.ReadLine(), out frequencyOfDataPoints);
            Console.WriteLine();
            Console.WriteLine($"\tNumber of Data Points: {frequencyOfDataPoints}");
            Console.ReadKey();


            return frequencyOfDataPoints;
        }

        /// <summary>
        /// Data Recorder > Get the Number of Data Points
        /// </summary>
        /// <returns></returns>
        static int LightDataRecorderDisplayGetNumberOfDataPoints()
        {
            int numberOfDataPoints;

            DisplayHeader("Number Of Data Points");

            Console.Write("Enter the Number of Data Points");

            int.TryParse(Console.ReadLine(), out numberOfDataPoints);
            Console.WriteLine();
            Console.WriteLine($"Number of Data Points: {numberOfDataPoints}");
            Console.ReadKey();

            return numberOfDataPoints;
        }

        #endregion



        #region DATA RECORDER
        private static void DisplayDataRecorder(Finch myFinch)
        {
            string caseSwitch;
            bool endNowData = false;

            int numberOfDataPoints = 0;
            double frequencyOfDataPointsSeconds = 0;
            double[] temperaturesC = null;

            do
            {
                DisplayHeader("Data Recorder Menu");


                Console.WriteLine("\ta) Get number of data points.");
                Console.WriteLine("\tb) Get the freuquency of data points");
                Console.WriteLine("\tc) Get of data points.");
                Console.WriteLine("\td) Display data table.");
                Console.WriteLine("\te)");
                Console.WriteLine("\t\tMan Menu");
                Console.WriteLine("Please select an option.");
                caseSwitch = Console.ReadLine();
                switch (caseSwitch)
                {
                    case "a":
                        numberOfDataPoints = DataRecorderDisplayGetNumberOfDataPoints();
                        break;

                    case "b":
                        frequencyOfDataPointsSeconds = DataRecorderDisplayGetFrequencyOfDataPoints();
                        break;

                    case "c":
                        if (numberOfDataPoints == 0 || frequencyOfDataPointsSeconds == 0)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Please enter the number and frequency");
                        }
                        else
                        {
                            temperaturesC = DataRecorderDisplayGetFrequencyOfDataSet(numberOfDataPoints, frequencyOfDataPointsSeconds, myFinch);
                        }
                        break;

                    case "d":
                        DataRecorderDisplayGetFrequencyOfDataSet(temperaturesC);
                        break;

                    case "quit":
                        Console.WriteLine("Back to main menu.");
                        endNowData = true;
                        break;
                }
            } while (!endNowData);
        }



        //Data Recorder >Display data table
        static void DataRecorderDisplayDataTable(double[] temperaturesC)
        {
            Console.WriteLine(
                "Reading #".PadLeft(15) +
                "Temperature".PadLeft(15)
        );

            for (int i = 0; i < temperaturesC.Length; i++)
            {
                Console.WriteLine(
                    (i + 1).ToString().PadLeft(15) +
                    temperaturesC[i].ToString("n2").PadLeft(15)
                    );
            }
        }
        static void DataRecorderDisplayGetFrequencyOfDataSet(double[] temperaturesC)
        {
            DisplayHeader("Data Set");
            DataRecorderDisplayDataTable(temperaturesC);
        }



        /// <summary>
        /// Data Recorder > Get the Data Points
        /// </summary>
        /// <param name="numberOfDataPoints"></param>
        /// <param name="frequencyOfDataPointsSeconds"></param>
        /// <param name="myFinch"></param>
        /// <returns></returns>
        static double[] DataRecorderDisplayGetFrequencyOfDataSet(int numberOfDataPoints, double frequencyOfDataPointsSeconds, Finch myFinch)
        {
            double[] temperatures = new double[numberOfDataPoints];

            DisplayHeader("Get Data Set");

            Console.WriteLine($"\tNumber of Data Points: {numberOfDataPoints}");
            Console.WriteLine($"\tFrequency of Data Points: {frequencyOfDataPointsSeconds}");
            Console.WriteLine();

            Console.WriteLine("\tFinch robot is ready to record temperature data.");
            Console.WriteLine("\tPress any key to begin.");
            Console.ReadKey();

            double temperature;
            int waitVariable;
            for (int index = 0; index < numberOfDataPoints; index++)
            {
                temperature = myFinch.getTemperature();
                Console.WriteLine($"Temperature Reading {index + 1}: {temperature} C");
                temperatures[index] = temperature;
                waitVariable = (int)(frequencyOfDataPointsSeconds * 1000);
                myFinch.wait(waitVariable);
            }

            return temperatures;
        }

        /// <summary>
        /// Data Recorder > Get the Frequency of Data Points
        /// </summary>
        /// <returns></returns>
        static double DataRecorderDisplayGetFrequencyOfDataPoints()
        {
            double frequencyOfDataPoints;

            DisplayHeader("Frequency of Data Points");

            Console.Write("Enter the Number of Data Points");

            double.TryParse(Console.ReadLine(), out frequencyOfDataPoints);
            Console.WriteLine();
            Console.WriteLine($"\tNumber of Data Points: {frequencyOfDataPoints}");
            Console.ReadKey();


            return frequencyOfDataPoints;
        }

        /// <summary>
        /// Data Recorder > Get the Number of Data Points
        /// </summary>
        /// <returns></returns>
        static int DataRecorderDisplayGetNumberOfDataPoints()
        {
            int numberOfDataPoints;

            DisplayHeader("Number Of Data Points");

            Console.Write("Enter the Number of Data Points");

            int.TryParse(Console.ReadLine(), out numberOfDataPoints);
            Console.WriteLine();
            Console.WriteLine($"Number of Data Points: {numberOfDataPoints}");
            Console.ReadKey();

            return numberOfDataPoints;
        }




        #endregion

        #region DISPLAY SETUP

        //
        //Set up the Terminal Display
        //
        private static void SetUpDisplay()
        {
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.SetWindowSize(70, 40);
            Console.Clear();
        }

        //
        //Create welcome message and explanation
        //
        private static void DisplayWelcomeMessage()
        {
            DisplayHeader("Finch Commandment Console.");
            Console.WriteLine("Welcome to this our simple menu here!");
            Console.WriteLine("Please take a look at the menu and sample one of our many options.");
            Console.WriteLine("Enjoy.");
            Thread.Sleep(4000);
            Console.Clear();
        }

        //
        //Function to collect user input.
        //
        private static string GatherInput()
        {
            Console.WriteLine("Please enter a response below based on the options above.");
            string a = Console.ReadLine();
            return a;

        }
        //
        //Simple display header function
        //
        private static void DisplayHeader(string displayHeader)
        {
            Console.WriteLine($"\t\t{displayHeader}");
            Console.WriteLine();
        }

        //
        //Main Menu Display
        //
        private static void DisplayMainMenu()
        {
            Console.WriteLine("Welcome here are our options currently.");
            Console.WriteLine("a. TALENT SHOW.");
            Console.WriteLine("b. Make Finch angry.");
            Console.WriteLine("c. Data Collection.");
            Console.WriteLine("d. Light Data Collection.");
            Console.WriteLine("e. Sentry Mode.");
            Console.WriteLine("quit to quit.");
        }
        private static void DisplayTalentShow()
        {
            DisplayHeader("Talent Show");
            Console.WriteLine("Welcome to Talent Show.");
            Console.WriteLine("We have five options to select from.");
            Console.WriteLine("a. will get the finch to sing");
            Console.WriteLine("b. will get the finch to dance for you.");
            Console.WriteLine("c. will get the finch to scream for it has no mouth.");
            Console.WriteLine("d will get the finch to perform a lightshow.");
            Console.WriteLine("quit to quit.");
        }
        private static void DisplaySentryMenu()
        {
            DisplayHeader("Sentry Controls");
            Console.WriteLine("Sentry Mode Options.");
            Console.WriteLine("a. Set the frequency of data being checked in seconds.");
            Console.WriteLine("b. Select temperature or light data.");
            Console.WriteLine("c. Set Minimum and Maximum.");
            Console.WriteLine("d. Activate Sentry Mode.");
            Console.WriteLine("quit to quit.");
        }

        //
        //Sentry Menu
        //
        #endregion

    }
}


