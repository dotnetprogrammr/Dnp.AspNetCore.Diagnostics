using System;
using System.Collections.Generic;

using FakeItEasy;

using Xunit;

namespace DNP.AspNetCore.Diagnostics.Test
{
    public class StatusCodeMappingTest
    {
        [Fact]
        public void WhenInitialized_WithANullITransformCollection_ExceptionThrown()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new StatusCodeMapping(null, 500));
            Assert.Equal("transformations", exception.ParamName);
        }

        [Fact]
        public void WhenInitialized_ShouldNotModifyTheStatusCode()
        {
            // Arrange
            var fakeITransformCollection = A.Fake<ITransformationCollection>();

            // Act
            var statusCodeMapping = new StatusCodeMapping(fakeITransformCollection, 500);

            // Assert
            Assert.Equal(500, statusCodeMapping.StatusCode);
        }

        [Fact]
        public void WhenInitialized_ShouldUseTheITransformationCollectionInstanceGiven()
        {
            // Arrange
            var fakeITransformCollection = A.Fake<ITransformationCollection>();

            // Act
            var statusCodeMapping = new StatusCodeMapping(fakeITransformCollection, 500);

            // Assert
            Assert.Same(fakeITransformCollection, statusCodeMapping.TransformationCollection);
        }

        [Fact]
        public void To_WhenInvoked_ShouldReturnAnInstanceWithTheSameStatusCode()
        {
            // Arrange
            var fakeITransformCollection = A.Fake<ITransformationCollection>();
            var statusCodeMapping = new StatusCodeMapping(fakeITransformCollection, 500);

            // Act
            var exceptionMapping = statusCodeMapping.To<Exception>();

            // Assert
            Assert.NotNull(exceptionMapping);
            Assert.IsAssignableFrom(typeof(ExceptionMapping), exceptionMapping);
            var typedExceptionMapping = exceptionMapping as ExceptionMapping;
            Assert.NotNull(typedExceptionMapping);
            Assert.Equal(500, typedExceptionMapping.StatusCode);
        }

        [Fact]
        public void To_WhenInvoked_ShouldAddANewTransformationIntoTheCollection()
        {
            // Arrange
            var transformationDictionary = new Dictionary<Type, int>();

            var fakeITransformCollection = A.Fake<ITransformationCollection>();
            A.CallTo(() => fakeITransformCollection.AddMappingFor<Exception>(A<int>._)).Invokes((int sc) =>
                {
                    transformationDictionary.Add(typeof(Exception), sc);
                });
            var statusCodeMapping = new StatusCodeMapping(fakeITransformCollection, 500);

            // Act
            statusCodeMapping.To<Exception>();

            // Assert
            A.CallTo(fakeITransformCollection).Where(call => call.Method.Name == "AddMappingFor").MustHaveHappened(Repeated.Exactly.Once);

            Assert.Equal(1, transformationDictionary.Count);
            Assert.True(transformationDictionary.ContainsKey(typeof(Exception)));
            var statusCode = transformationDictionary[typeof(Exception)];
            Assert.Equal(500, statusCode);
        }
    }
}
