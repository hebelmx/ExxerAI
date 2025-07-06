
# Neo4j Data Access for Your .NET Core C# Microservice
[Why Graph Databases?](https://neo4j.com/why-graph-databases/)

Neo4j is the industry leader and pioneer of graph database technology and is currently the top-rated graph database management system in the world.

In my experience so far with designing solutions and applications around Neo4j, I came across many materials and online resources from Neo4j and its huge community. There are plenty of resources on Neo4j around various technologies. Neo4j provides excellent drivers for various programming languages to build seamless graph data connectivity with your application.

https://www.youtube.com/watch?v=14SVhbEFQpM&ab_channel=Farhad

[Drivers & Language Guides – Developer Guides](https://neo4j.com/developer/language-guides/)

C# is my first love as a programming language, and Visual Studio development experience will always be my preferred one. I was brought up as a programmer in the .NET and C# world. Hence, I decided to share an approach to effectively use the Neo4j C# driver to develop the data access layer for a .NET Core application.

I am going to share an example of a Neo4j-based Microservice (Web API). However, with out-of-the-box dependency injection (DI) and Service Provider features from .NET Core, this approach can be used for other types of applications, too.

Before you start with .NET Core implementation, I suggest you read a particularly good article by [David Allen](https://medium.com/@david.allen_3172) on Neo4j driver general best practices. These practices apply to all programming languages.

[Neo4j Driver Best Practices](https://medium.com/neo4j/neo4j-driver-best-practices-dfa70cf5a763)

### .NET Core Specific Implementation

For developing a Neo4j Data Access for .NET Core, I have divided the approach into the steps you’ll see below.

#### **Have a Neo4j Database Instance Up and Running**

If you already have a Neo4j database instance set up and running, you can skip this step. If not, you can easily set up a free Neo4j Aura (Database as a Service) in a hassle-free way. Use the below link to proceed.

[Neo4j Aura – Fully Managed Cloud Solution](https://neo4j.com/product/auradb/?ref=nav-get-started-cta)

Alternatively, you can use [Neo4j Desktop](https://neo4j.com/product/developer-tools/#desktop) on your local machine or a [Neo4j Sandbox](https://sandbox.neo4j.com/?ref=get-started-dropdown-cta&persona=data-scientist).

While loading the Aura instance or Sandbox, use the prebuilt Movies dataset. If you are using Neo4j Desktop or a plain database, run command :play Movies in your Cypher command bar and follow the data load step to load the test data.

#### **Add the Neo4j Official Driver NuGet Package to Your .NET Core Project**

You can install the driver package using the NuGet package manager console or package manager interface in Microsoft Visual Studio. Install this package to the data access project. I suggest using a separate project for data access instead of the main API or application project.

[Neo4j.Driver 4.4.0](https://www.nuget.org/packages/Neo4j.Driver)

> **_Application Settings_**

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ApplicationSettings": {
    "Neo4jConnection": "neo4j://localhost:7687",
    "Neo4jUser": "neo4j",
    "Neo4jPassword": "password",
    "Neo4jDatabase": "neo4j"
  }
}

    public class ApplicationSettings
    {
        public Uri Neo4jConnection { get; set; }

        public string Neo4jUser { get; set; }

        public string Neo4jPassword { get; set; }

        public string Neo4jDatabase { get; set; }
    }
Then go to your app settings file: Neo4j Bolt/Neo4j connection string, Neo4j username, password, and database instance name.

You should implement additional security measures for protecting your Neo4j password instead of specifying it in plain text. For example, try an encryption of your password text, or use some vault to keep your credentials safe.

#### **Startup: Dependency Registration / Dependency Injection for Neo4j Driver**
// Register application setting using IOption provider mechanism
services.Configure<ApplicationSettings>(Configuration.GetSection("ApplicationSettings"));

// Fetch settings object from configuration
var settings = new ApplicationSettings();
Configuration.GetSection("ApplicationSettings").Bind(settings);

// This is to register your Neo4j Driver Object as a singleton
services.AddSingleton(GraphDatabase.Driver(settings.Neo4jConnection, AuthTokens.Basic(settings.Neo4jUser, settings.Neo4jPassword)));

// This is your Data Access Wrapper over Neo4j session, that is a helper class for executing parameterized Neo4j Cypher queries in Transactions
services.AddScoped<INeo4jDataAccess, Neo4jDataAccess>();

// This is the registration for your domain repository class
services.AddTransient<IPersonRepository, PersonRepository>();
We are injecting the Neo4j Driver instance as a singleton so that we do not have to manage the driver instance on each request.

#### **IAsyncDisposable implementation of Neo4j Data Access Class**

[IAsyncDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.iasyncdisposable?view=net-6.0) interface provides a mechanism for releasing unmanaged resources asynchronously. We injected the Neo4j Data Access class as a scoped dependency. Neo4jData Access Wrapper will establish a database session. We will manage the Neo4j session using [IAsyncDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.iasyncdisposable?view=net-6.0).

I have developed the Wrapper methods keeping easy implementation and reusability in mind. You are free to develop your own version of Wrapper methods.

**Data Access Interface**
 public interface INeo4jDataAccess : IAsyncDisposable
    {
        Task<List<string>> ExecuteReadListAsync(string query, string returnObjectKey, IDictionary<string, object>? parameters = null);
       
        Task<List<Dictionary<string, object>>> ExecuteReadDictionaryAsync(string query, string returnObjectKey, IDictionary<string, object>? parameters = null);

        Task<T> ExecuteReadScalarAsync<T>(string query, IDictionary<string, object>? parameters = null);

        Task<T> ExecuteWriteTransactionAsync<T>(string query, IDictionary<string, object>? parameters = null);
    }
**Data Access Implementation**
  public class Neo4jDataAccess : INeo4jDataAccess
    {
        private IAsyncSession _session;

        private ILogger<Neo4jDataAccess> _logger;

        private string _database;

        /// <summary>
        /// Initializes a new instance of the <see cref="Neo4jDataAccess"/> class.
        /// </summary>
        public Neo4jDataAccess(IDriver driver, ILogger<Neo4jDataAccess> logger, IOptions<ApplicationSettings> appSettingsOptions)
        {
            _logger = logger;
            _database = appSettingsOptions.Value.Neo4jDatabase ?? "neo4j";
            _session = driver.AsyncSession(o => o.WithDatabase(_database));
        }

        /// <summary>
        /// Execute read list as an asynchronous operation.
        /// </summary>
        public async Task<List<string>> ExecuteReadListAsync(string query, string returnObjectKey, IDictionary<string, object>? parameters = null)
        {
            return await ExecuteReadTransactionAsync<string>(query, returnObjectKey, parameters);
        }

        /// <summary>
        /// Execute read dictionary as an asynchronous operation.
        /// </summary>
        public async Task<List<Dictionary<string, object>>> ExecuteReadDictionaryAsync(string query, string returnObjectKey, IDictionary<string, object>? parameters = null)
        {
            return await ExecuteReadTransactionAsync<Dictionary<string, object>>(query, returnObjectKey, parameters);
        }

        /// <summary>
        /// Execute read scalar as an asynchronous operation.
        /// </summary>
        public async Task<T> ExecuteReadScalarAsync<T>(string query, IDictionary<string, object>? parameters = null)
        {
            try
            {
                parameters = parameters == null ? new Dictionary<string, object>() : parameters;

                var result = await _session.ReadTransactionAsync(async tx =>
                {
                    T scalar = default(T);

                    var res = await tx.RunAsync(query, parameters);

                    scalar = (await res.SingleAsync())[0].As<T>();

                    return scalar;
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was a problem while executing database query");
                throw;
            }
        }

        /// <summary>
        /// Execute write transaction
        /// </summary>
        public async Task<T> ExecuteWriteTransactionAsync<T>(string query, IDictionary<string, object>? parameters = null)
        {
            try
            {
                parameters = parameters == null ? new Dictionary<string, object>() : parameters;

                var result = await _session.WriteTransactionAsync(async tx =>
                {
                    T scalar = default(T);

                    var res = await tx.RunAsync(query, parameters);

                    scalar = (await res.SingleAsync())[0].As<T>();

                    return scalar;
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was a problem while executing database query");
                throw;
            }
        }

        /// <summary>
        /// Execute read transaction as an asynchronous operation.
        /// </summary>
        private async Task<List<T>> ExecuteReadTransactionAsync<T>(string query, string returnObjectKey, IDictionary<string, object>? parameters)
        {
            try
            {                
                parameters = parameters == null ? new Dictionary<string, object>() : parameters;

                var result = await _session.ReadTransactionAsync(async tx =>
                {
                    var data = new List<T>();

                    var res = await tx.RunAsync(query, parameters);

                    var records = await res.ToListAsync();

                    data = records.Select(x => (T)x.Values[returnObjectKey]).ToList();

                    return data;
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was a problem while executing database query");
                throw;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources asynchronously.
        /// </summary>
        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await _session.CloseAsync();
        }
    }
#### **Domain Repository Methods for Your Cypher Query and Parameter Prep**
  public class PersonRepository : IPersonRepository
    {
        private INeo4jDataAccess _neo4jDataAccess;

        private ILogger<PersonRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRepository"/> class.
        /// </summary>
        public PersonRepository(INeo4jDataAccess neo4jDataAccess, ILogger<PersonRepository> logger)
        {
            _neo4jDataAccess = neo4jDataAccess;
            _logger = logger;
        }

        /// <summary>
        /// Searches the name of the person.
        /// </summary>
        public async Task<List<Dictionary<string, object>>> SearchPersonsByName(string searchString)
        {
            var query = @"MATCH (p:Person) WHERE toUpper(p.name) CONTAINS toUpper($searchString) 
                                RETURN p{ name: p.name, born: p.born } ORDER BY p.Name LIMIT 5";

            IDictionary<string, object> parameters = new Dictionary<string, object> { { "searchString", searchString } };

            var persons = await _neo4jDataAccess.ExecuteReadDictionaryAsync(query, "p", parameters);

            return persons;
        }

        /// <summary>
        /// Adds a new person
        /// </summary>
        public async Task<bool> AddPerson(Person person)
        {
            if (person != null && !string.IsNullOrWhiteSpace(person.Name))
            {
                var query = @"MERGE (p:Person {name: $name}) ON CREATE SET p.born = $born 
                            ON MATCH SET p.born = $born, p.updatedAt = timestamp() RETURN true";
                IDictionary<string, object> parameters = new Dictionary<string, object> 
                { 
                    { "name", person.Name },
                    { "born", person.Born ?? 0 }
                };
                return await _neo4jDataAccess.ExecuteWriteTransactionAsync<bool>(query, parameters);
            }
            else
            {
                throw new System.ArgumentNullException(nameof(person), "Person must not be null");
            }
        }

        /// <summary>
        /// Get count of persons
        /// </summary>
        public async Task<long> GetPersonCount()
        {
            var query = @"Match (p:Person) RETURN count(p) as personCount";
            var count = await _neo4jDataAccess.ExecuteReadScalarAsync<long>(query);
            return count;
        }
    }
You may notice I used the Cypher statement `RETURN p{ .name, born: p.born }` in my method to return Person data. Cypher supports a concept called “map projections,” which allows for easily constructing map projections from nodes, relationships, and other map values as shown in the below screenshot.

![Results fetched from the database using cypher map projection](https://dist.neo4j.com/wp-content/uploads/20220629133201/1tMelejP1UKlXsonI4725sA.jpeg)

[Maps – Neo4j Cypher Manual](https://neo4j.com/docs/cypher-manual/current/syntax/maps/#cypher-map-projection)

With Cypher projections, I populated `List<Dictionary<string, object>>` collection. If your API just needs to provide plain results without additional manipulation, you can simply return this collection. This helps you avoid extra boxing and unboxing. Clients can parse the JSON to their desired object types.

For example, below is the swagger response for my API that returns plain data.

![](https://dist.neo4j.com/wp-content/uploads/20220629133156/1Yk6nK-mCoD2vtBG0CDVkSg.png)

Thanks for spending the time to read this. I hope you find it helpful!

Please share so it reaches more people!

![](https://medium.com/_/stat?event=post.clientViewed&referrerSource=full_rss&postId=8dbf8a8d8a79)

* * *

[Neo4j Data Access for your Dot Net Core C# Microservice](https://medium.com/neo4j/neo4j-data-access-for-your-dot-net-core-c-microservice-8dbf8a8d8a79) was originally published in [Neo4j Developer Blog](https://medium.com/neo4j) on Medium, where people are continuing the conversation by highlighting and responding to this story.