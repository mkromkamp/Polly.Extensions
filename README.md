# Polly.Extensions

Small collection of Polly.Net extensions

## Polly.Extensions.Distributed

Contains distributed implementations of Polly.Net policies.

### Distributed bulkhead

The `DistributedBulkheadPolicy` can be used to create bulkhead policy with shared state. 
These can come in handy if you need to limit the concurrent access to a resource accross application instances. For example if you are limited to the concurrent access of a downstream services.

#### Usage

To create a distributed bulkhead you can use the normal Polly context to create a `Policy` and add the distributed bulkhead;

``` csharp

IConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect("localhost");
IDistributedBulkheadBackplan backplane = new RedisDistrubutedBulkheadBackplane(multiplexer);

var policy = Policy
                .Handle<Expection>()
                .DistributedBulkhead(backplane, "example", 3);
```

In the code sample above we create a `Policy` that handles all expections. The distributed bulkhead is limited to 3 concurrent excecutions and has the bulkhead key `example`. Redis is being used for the shared state.

#### Bulkhead key

To make use of more than one distributed bulkhead you have to specify the bulkhead key during the creation of the policy. This bulkhead key is the unique identifier of this bulkhead and can be used accross applications.