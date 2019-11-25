using LaYumba.Functional;
using NUnit.Framework;
using System;
using static LaYumba.Functional.F;
using Unit = System.ValueTuple;

namespace Exercises.Chapter08
{
    public static class Exercises
    {
        // 1. Implement Apply for Either and Exceptional


        // Apply: A<x -> y> -> A<x> -> A<y>
        public static Either<L, R> Apply<L, T, R>(this Either<L, Func<T, R>> @this, Either<L, T> arg)
            => @this.Match(
                error => Left(error),
                f => arg.Match(
                    error => Left(error),
                    value => (Either<L, R>)Right(f(value))));

        // Apply: A<x -> y -> z> -> A<x> -> A<y> -> A<z>
        public static Either<L, Func<T2, R>> Apply<L, T1, T2, R>(this Either<L, Func<T1, T2, R>> @this, Either<L, T1> arg)
            => @this.Match(
                error => Left(error),
                f => arg.Match(
                    error => Left(error),
                    val => (Either<L, Func<T2, R>>)Right(f.Apply(val))));


        // Apply: E<x -> y> -> E<x> -> E<y>
        public static Exceptional<R> Appply<T, R>(this Exceptional<Func<T, R>> liftedFunc, Exceptional<T> arg)
            => liftedFunc.Match(
                exception => exception,
                theFunc => arg.Match(
                    exception => exception,
                    value => Exceptional(theFunc(value))));

        // Apply: E<x -> y -> z> -> E<x> -> E<y> -> E<z>
        public static Exceptional<Func<R, RR>> Appply<T, R, RR>(this Exceptional<Func<T, R, RR>> liftedFunc, Exceptional<T> arg)
            => liftedFunc.Match(
                exception => exception,
                theFunc => arg.Match(
                    exception2 => exception2,
                    value => Exceptional(theFunc.Apply(value))));



        public class ApplyTestsForEitherAndExceptional
        {
            Func<int, int, int> multiply = (x, y) => y * x;
            Func<int, Func<int, int>> mult => x => y => x * y;


            [Test]
            public void ShouldApplyElevatedFunctionOnElevatedValue()
            {
                Either<string, Func<int, int, int>> liftedMult = Right(multiply);
                Either<string, int> possitiveNumber = Right(3);
                Either<string, int> possitiveNumber2 = Right(4);

                Either<string, int> possitiveNumMultiplication =
                    liftedMult
                    .Apply(possitiveNumber)
                    .Apply(possitiveNumber2);

                Assert.AreEqual((Either<string, int>)Right(12), possitiveNumMultiplication);

            }

            [Test]
            public void ExceptionalApplyTest()
            {
                Exceptional<Func<int, int, int>> liftedMultPos = Exceptional(multiply);
                Exceptional<int> arg1 = Exceptional(2);
                Exceptional<int> arg2 = Exceptional(5);
                //Exceptional<int> arg2 = new Exceptional<int>(new ArgumentException("something went wrong"));

                Exceptional<int> sum = liftedMultPos
                    .Appply(arg1)
                    .Appply(arg2);

                Assert.AreEqual(Exceptional(10), sum);

            }

        }


        // 2. Implement the query pattern for Either and Exceptional. Try to write down 
        // the signatures for Select and SelectMany without looking at any examples. for
        // the implementation, just follow the types--if it type checks, it's probably right!


        // Select: (R -> RR) -> Either<T,R> -> Either<T,RR>
        public static Either<T, RR> Select<T, R, RR>(this Either<T, R> @this, Func<R, RR> f)
            => @this.Match(
                Left: left => Left(left),
                Right: right => (Either<T, RR>)Right(f(right)));

        // SelectMany: (R -> Either<L,RR>) -> Either<L,R> -> Either<L,RR>
        public static Either<L, RR> SelectMany<L, R, RR>(this Either<L, R> @this, Func<R, Either<L, RR>> f)
            => @this.Match(
                left => Left(left),
                right => f(right));

        // SelectMany2: (T -> R -> RR) -> (T -> Either<L,R>) -> Either<L,T> -> Either<L,RR>
        public static Either<L, RR> SelectMany<L, T, R, RR>(
            this Either<L, T> @this,
            Func<T, Either<L, R>> bind,
            Func<T, R, RR> project)
            => @this.Match(
                left1 => Left(left1),
                right1 => bind(right1).Match(
                    left2 => Left(left2),
                    right2 => (Either<L, RR>)Right(project(right1, right2))));

        // Exceptional
        // Select: (T -> R) -> Exceptional<T> -> Exceptional<R>
        public static Exceptional<R> Select<T, R>(this Exceptional<T> @this, Func<T, R> f)
            => @this.Match(
                ex => ex,
                value => Exceptional(f(value)));

        // SelectMany: (T -> Exceptional<R>) -> Exceptional<T> -> Exceptional<R>
        public static Exceptional<R> SelectMany<T, R>(this Exceptional<T> @this, Func<T, Exceptional<R>> bind)
            => @this.Match(
                ex => ex,
                value => bind(value));

        // SelectMany2: (T -> R -> RR) -> (T -> Exceptional<R>) -> Exceptional<T> -> Exceptional<RR>
        public static Exceptional<RR> SelectMany<T, R, RR>(
            this Exceptional<T> @this,
            Func<T, Exceptional<R>> bind,
            Func<T, R, RR> project)
            => @this.Match(
                ex1 => ex1,
                val1 => bind(val1).Match(
                    ex2 => ex2,
                    val2 => Exceptional(project(val1, val2))));

        public class QueryPatternTests
        {

            [Test]
            public void EitherLINQSelect_Test()
            {

                Either<string, int> res =
                    from x in (Either<string, int>)Right(10)
                    select x * 2;

                res.Match(
                    left => Assert.Fail(),
                    right => Assert.AreEqual(20, right));

            }

            [Test]
            public void EitherLINQSelectMany_Test()
            {
                Either<string, int> right1 = Right(5);
                Either<string, int> right2 = Right(10);

                Either<string, int> res =
                    from x in right1
                    from y in right2
                    select x + y;

                // Same result using extension methods:
                // *The query pattern is preferable when working with multiple data sources.

                // Either<string, int> res2 = 
                //     right1.SelectMany(x => right2
                //         .Select(y => x + y));

                Either<string, int> res2 =
                    right1.SelectMany(x => right2, (x, y) => x + y);


                Assert.AreEqual(res, res2);
            }

            [Test]
            public void ExceptionalLINQSelect_Test()
            {
                Exceptional<string> firstName = Exceptional("Polly");

                Exceptional<string> firstNameUpperCase = from f in firstName
                                                         select f.ToUpper();

                firstNameUpperCase.Match(
                    (ex) => Assert.Fail(),
                    (val) => Assert.AreEqual("POLLY", val));
            }

            [Test]
            public void ExceptionalLINQSelectMany_Test()
            {
                Exceptional<string> firstName = Exceptional("Polly");
                Exceptional<string> lastname = Exceptional("Andersson");

                Exceptional<string> fullName = from fname in firstName
                                               from lname in lastname
                                               select $"{fname} {lname}";

                fullName.Match(
                    ex => Assert.Fail(),
                    val => Assert.AreEqual("Polly Andersson", val));


                Exceptional<string> fullName2 =
                    firstName.Bind(f => lastname.Map(l => $"{f} {l}"));

                Assert.AreEqual(fullName, fullName2);


                Func<string, string, string> fullNamer = (fname, lname) => $"{fname} {lname}";
                Exceptional<string> fullName3 = Exceptional(fullNamer)
                    .Appply(firstName)
                    .Appply(lastname);

                Assert.AreEqual(fullName, fullName3);

            }

        }

        // 3. Come up with a scenario in which various Either-returning operations are chained with Bind.
        // (If you're short of ideas, you can use the favorite-dish example from chaper 6.) Rewrite the code using a LINQ expression.
        public class EitherReturningOperationsTest
        {
            class Reason { }
            class Ingredients { }
            class Food { }

            private Func<int, Either<Reason, Unit>> WakeUpEarly =
                 wakeupTime => wakeupTime > 8
                    ? (Either<Reason, Unit>)Left(new Reason())
                    : Right(Unit());

            private Func<int, Either<Reason, Ingredients>> ShopForIngredients =
                ingredientsCost => ingredientsCost > 200
                    ? (Either<Reason, Ingredients>)Left(new Reason())
                    : Right(new Ingredients());

            private Func<Ingredients, bool, Either<Reason, Food>> CookRecipe =
                (ingredients, burnedIt) => burnedIt
                    ? (Either<Reason, Food>)Left(new Reason())
                    : Right(new Food());

            [Test]
            public void AllGoesWellTest()
            {
                var resLinqQueryPattern =
                    from wakeup in WakeUpEarly(6)
                    from ingredients in ShopForIngredients(150)
                    select CookRecipe(ingredients, false);

                resLinqQueryPattern.Match(
                     Right: food => Assert.Pass("Nice dinner"),
                     Left: reason => Assert.Fail("Should have a good dinner"));


                var resTranslatedQueryPattern = WakeUpEarly(6)
                    .SelectMany(unit => ShopForIngredients(150), (u, ingredients) => CookRecipe(ingredients, false));

                var resWithBindAndMap = WakeUpEarly(6)
                    .Bind(unit => ShopForIngredients(150).Map(ingredients => CookRecipe(ingredients, false)));

                Assert.IsInstanceOf(typeof(Either<Reason, Either<Reason, Food>>),
                    ShopForIngredients(150).Select(ingredients => CookRecipe(ingredients, false)));


                WakeUpEarly(6)
                   .Bind(unit => ShopForIngredients(150))
                   .Bind(ingredients => CookRecipe(ingredients, false))
                   .Match(
                        Right: food => Assert.Pass("Nice dinner"),
                        Left: reason => Assert.Fail("Should have a good dinner"));
            }
        }
    }
}