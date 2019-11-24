namespace Exercises.Chapter2
{
    using System;
    using static System.Math;
    using NUnit.Framework;

    // 1. Write a console app that calculates a user's Body-Mass Index:
    //   - prompt the user for her height in metres and weight in kg
    //   - calculate the BMI as weight/height^2
    //   - output a message: underweight(bmi<18.5), overweight(bmi>=25) or healthy weight
    // 2. Structure your code so that structure it so that pure and impure parts are separate
    // 3. Unit test the pure parts
    // 4. Unit test the impure parts using the HOF-based approach
    public static class Bmi
    {
        public static void Run()
        {
            run(read, write);
            Console.ReadLine();
        }

        public static double read(string what)
        {
            Console.WriteLine($"Enter your {what}");
            var input = Console.ReadLine();
            return Double.Parse(input);
        }

        public static void write(HealthStatus status)
        {
            Console.Write($"Your health status: {status.ToString()}");
        }

        internal static void run(Func<string, double> read, Action<HealthStatus> write)
        {
            var weight = read("weight");
            var height = read("height");

            var healthStatus = BmiHealthStatus(CalculateBmi(weight, height));

            write(healthStatus);
        }

        public enum HealthStatus { UnderWeight, OverWeight, Healthy };

        public static double CalculateBmi(double weight, double height)
            => Round((weight / Pow(height, 2)), 2);

        public static HealthStatus BmiHealthStatus(double bmi)
            => (bmi > 25) ? HealthStatus.OverWeight
            : (bmi < 18.5) ? HealthStatus.UnderWeight
            : HealthStatus.Healthy;

    }


    public class TestBMIExercise
    {
        [TestCase(80, 1.85, ExpectedResult = 23.37)]
        public double CalculateBmiTest(double weight, double height)
        {
            return Bmi.CalculateBmi(weight, height);
        }

        [TestCase(30, ExpectedResult = Bmi.HealthStatus.OverWeight)]
        [TestCase(20, ExpectedResult = Bmi.HealthStatus.Healthy)]
        [TestCase(10, ExpectedResult = Bmi.HealthStatus.UnderWeight)]
        public Bmi.HealthStatus BmiStatusTest(double bmi)
        {
            return Bmi.BmiHealthStatus(bmi);
        }

        [Test]
        public void TestIO()
        {
            Func<string, double> read =
                input => input.Equals("weight") ? 80 :
                         input.Equals("height") ? 1.85 : 0;


            Bmi.HealthStatus resOutput = default(Bmi.HealthStatus);

            Bmi.run(read, status => resOutput = status);

            Assert.AreEqual(Bmi.HealthStatus.Healthy, resOutput);

        }


    }


}