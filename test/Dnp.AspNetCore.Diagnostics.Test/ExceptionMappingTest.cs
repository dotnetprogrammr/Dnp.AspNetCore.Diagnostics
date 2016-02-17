using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FakeItEasy;

using Xunit;

namespace DNP.AspNetCore.Diagnostics.Test
{
    public class ExceptionMappingTest
    {
        [Fact]
        public void WhenInitialized_WithANullITransformCollection_ExceptionThrown()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new ExceptionMapping(null, 500));
            Assert.Equal("transformations", exception.ParamName);
        }

        [Fact]
        public void WhenInitialized_ShouldNotModifyTheStatusCode()
        {
            // Arrange
            var fakeITransformCollection = A.Fake<ITransformationCollection>();

            // Act
            var exceptionMapping = new ExceptionMapping(fakeITransformCollection, 500);

            // Assert
            Assert.Equal(500, exceptionMapping.StatusCode);
        }

        [Fact]
        public void WhenInitialized_ShouldUseTheITransformationCollectionInstanceGiven()
        {
            // Arrange
            var fakeITransformCollection = A.Fake<ITransformationCollection>();

            // Act
            var exceptionMapping = new ExceptionMapping(fakeITransformCollection, 500);

            // Assert
            Assert.Same(fakeITransformCollection, exceptionMapping.TransformationCollection);
        }

        [Fact]
        public void Map_WhenInvoked_ShouldReturnAnInstanceWithTheSameStatusCode()
        {
            // Arrange
            var fakeITransformCollection = A.Fake<ITransformationCollection>();
            var exceptionMapping = new ExceptionMapping(fakeITransformCollection, 500);

            // Act
            var statusCodeMapping = exceptionMapping.Map(404);

            // Assert
            Assert.NotNull(statusCodeMapping);
            Assert.IsAssignableFrom(typeof(StatusCodeMapping), statusCodeMapping);
            var typedMapping = statusCodeMapping as StatusCodeMapping;
            Assert.NotNull(typedMapping);
            Assert.Equal(404, typedMapping.StatusCode);
        }

        [Fact]
        public void Map_WhenInvoked_ShouldReturnAnInstanceWithTheSameITransformationCollection()
        {
            // Arrange
            var fakeITransformCollection = A.Fake<ITransformationCollection>();
            var exceptionMapping = new ExceptionMapping(fakeITransformCollection, 500);

            // Act
            var statusCodeMapping = exceptionMapping.Map(404);

            // Assert
            Assert.NotNull(statusCodeMapping);
            Assert.IsAssignableFrom(typeof(StatusCodeMapping), statusCodeMapping);
            var typedMapping = statusCodeMapping as StatusCodeMapping;
            Assert.Same(fakeITransformCollection, typedMapping.TransformationCollection);
        }

        [Fact]
        public void Or_WhenInvokedShouldReturnAnInstanceWithTheSameStatusCode()
        {
            // Arrange
            var fakeITransformCollection = A.Fake<ITransformationCollection>();
            var exceptionMapping = new ExceptionMapping(fakeITransformCollection, 500);

            // Act
            var newExceptionMapping = exceptionMapping.Or<Exception>();

            // Assert
            Assert.NotNull(newExceptionMapping);
            Assert.IsAssignableFrom(typeof(ExceptionMapping), newExceptionMapping);
            var typedMapping = newExceptionMapping as ExceptionMapping;
            Assert.Same(fakeITransformCollection, typedMapping.TransformationCollection);
        }

        [Fact]
        public void Or_WhenInvoked_ShouldAddANewTransformationIntoTheCollection()
        {
            // Arrange
            var transformationDictionary = new Dictionary<Type, int>();

            var fakeITransformCollection = A.Fake<ITransformationCollection>();
            A.CallTo(() => fakeITransformCollection.AddMappingFor<Exception>(A<int>._)).Invokes((int sc) =>
            {
                transformationDictionary.Add(typeof(Exception), sc);
            });
            var exceptionMapping = new ExceptionMapping(fakeITransformCollection, 500);

            // Act
            exceptionMapping.Or<Exception>();

            // Assert
            Assert.Equal(1, transformationDictionary.Count);
            Assert.True(transformationDictionary.ContainsKey(typeof(Exception)));
            var statusCode = transformationDictionary[typeof(Exception)];
            Assert.Equal(500, statusCode);

            A.CallTo(fakeITransformCollection).Where(call => call.Method.Name == "AddMappingFor").MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}
