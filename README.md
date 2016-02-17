# Exception Transformation Middleware
This is a middleware example that handles a set of known exceptions and returns the defined status code for the exception. It allows you to throw exceptions from any layer in your code and returns a specific response to the client (i.e. a 404 (Not Found) response when you throw a CustomerNotFoundException).

> I recommend not using the middleware as is in a production environment.
> It was created as an example for others and as a learning exercise for myself.
> Responses should be more than just a status code.

## How to use
The syntax required to configue and add the middleware into the pipeline is relatively simple, in the startup.cs file include the following in the configure method:
```csharp
    var transformations = new TransformationCollectionBuilder()
        .Return(404)
        .For<CustomerNotFoundException>()
        .Transformations;
    app.UseExceptionTransformations(transformations);
```

You can assign the same status code to multiple exceptions:
```csharp
    var transformations = new TransformationCollectionBuilder()
        .Return(404)
        .For<CustomerNotFoundException>()
        .Or<AddressNotFoundException>()
        .Transformations;
```

## Contributions
Your feedback is welcome, open an issue or submit a pull request.