using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ShowMeAnExampleOfAutomapperFromLinkedSource.Tests.AutoMappers {
    [TestFixture]
    public class CallFuncExpressionTests {
        [Test]
        public void X()
        {
            var source = new MyCallFuncModel
            {
                X = "hey",
                Y = "you"
            };
            var exp = CreateExpressionThatInvokeMyFunc<MyCallFuncModel>("X","Y").Compile();

            var actual = exp(source);

            Assert.That(actual, Is.EqualTo("hey-you"));
        }

        static Expression<Func<T, string>> CreateExpressionThatInvokeMyFunc<T>(string propertyInDotNotationA, string propertyInDotNotationB)
        {
            var root = Expression.Parameter(typeof(T), "root");
            var propertyExpressionA = GenerateGetProperty<T>(propertyInDotNotationA, root);
            var propertyExpressionB = GenerateGetProperty<T>(propertyInDotNotationB, root);

            var call = Expression.Call(typeof(CallFuncExpressionTests), "MyFunc", new[] { typeof(string)}, propertyExpressionA, propertyExpressionB);
            return Expression.Lambda<Func<T, string>>(call,root);
        }

        private static Expression GenerateGetProperty<T>(string propertyInDotNotation, ParameterExpression root)
        {
            Expression propertyExpression = root;
            foreach (var property in propertyInDotNotation.Split('.'))
            {
                propertyExpression = Expression.PropertyOrField(propertyExpression, property);
            }
            return propertyExpression;
        }


        public static string MyFunc<T>(T a, T b)
        {
            return a + "-" + b;
        }

        static Expression<Func<T, TProperty>> CreateMemberExpression<T, TProperty>(string propertyInDotNotation) {
            var root = Expression.Parameter(typeof(T), "root");
            Expression lambdaBody = root;
            foreach (var property in propertyInDotNotation.Split('.')) {
                lambdaBody = Expression.PropertyOrField(lambdaBody, property);
            }

            return Expression.Lambda<Func<T, TProperty>>(lambdaBody, root);
        }
    }

    public class MyCallFuncModel {
        public string X { get; set; }
        public string Y { get; set; }
    }

}
