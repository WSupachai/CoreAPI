using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// --- Connect MongoDB ---
// Connection String From appsettings.json
var connectionString = builder.Configuration.GetConnectionString("MongoDb");
var databaseName = builder.Configuration["DatabaseName"] ?? "PersonDB";
var collectionName = builder.Configuration["CollectionName"] ?? "People";

var client = new MongoClient(connectionString);
var database = client.GetDatabase(databaseName);
var collection = database.GetCollection<Person>(collectionName);


//CORS 
builder.Services.AddCors();


// --- Add Services ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); //Swagger UI

var app = builder.Build();

// --- Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//USE CORS (ต้องวางก่อน Map API)
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());


// app.UseHttpsRedirection(); // ปิดไว้ก่อน

// ==========================================
// API (Before app.Run allways)
// ==========================================

// 1. GET ALL
app.MapGet("/people", async () => 
{
    var people = await collection.Find(_ => true).ToListAsync();
    return Results.Ok(people);
});

// 2. GET BY ID
app.MapGet("/people/{id}", async (string id) =>
{
    var person = await collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    return person is not null ? Results.Ok(person) : Results.NotFound("ไม่พบข้อมูล");
});

// POST 
app.MapPost("/people", async (PersonInput input) =>
{
    var newPerson = new Person
    {
        FirstName = input.FirstName,
        LastName = input.LastName,
        Address = input.Address,
        BirthDate = input.BirthDate
    };

    await collection.InsertOneAsync(newPerson);

    return Results.Created($"/people/{newPerson.Id}", newPerson);
});

// ==========================================
// END API
// ==========================================

app.Run(); 

// --- Models ---
public class PersonInput
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
}

public class Person
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }

    [BsonIgnore]
    public int Age 
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var age = today.Year - BirthDate.Year;
            if (BirthDate > today.AddYears(-age)) age--;
            return age;
        }
    }
}