# BookStoreApi

## Description

BookStoreApi is a .NET Web API consisting of three projects:

- **BookStore**: The main project containing controllers, services, and application configuration.
- **Infrastructure**: The project containing elements mainly associated with the database, such as entities, repositories, migrations, and models - commands, queries, and contracts.
- **Tests**: The project containing tests.

## Running the Application

To run the application, follow these steps:

1. In the NuGet Package Console, type "update-database".
2. Go to `Tests/Integration/Samples/Seeder.cs` and find the "SeedToDb" method.
3. Uncomment the `[Test]` attribute.
4. Run this test.

## Obtaining an Authentication Token

To obtain an authentication token, use the `/Identity/Login` endpoint.

Login information for seeded accounts:
- Logins: "user", "manager", "admin"
- Password: "zaq1@WSX"

## Project Information

- The database has four entities:
  - **Book**: Contains information about a book, such as the author or title.
  - **BookInfo**: Contains information about the availability or price of a book.
  - **Order**: Contains information about an order, such as the owner or total price.
  - **OrderItem**: Contains information about items in an order, such as OrderId or quantity.
- All requests to the database are stored in repositories (except for the Identity service, which uses the Identity Framework).
- The application uses JWT tokens for request authentication.
- Tests utilize the NUnit framework.
