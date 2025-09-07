# API Structuring Guide

This guide provides a step-by-step approach to structuring your .NET Web API application, along with important considerations for building robust and maintainable APIs.

## Folder Structure

A well-organized folder structure is crucial for maintainability. We recommend the following structure:

- **Controllers**: Handle incoming HTTP requests and send responses. They should be lightweight and delegate business logic to services.
- **Models**: Define the data structures (entities or DTOs) used in your application.
- **Services**: Contain the core business logic of your application. They are called by controllers and can interact with data repositories or other services.

## Core C# Concepts

### Namespaces, `using`, and Aliases

- **Namespace**: The full, official "address" of a type (e.g., `HotelBookingAPI.Models`). It's used to organize code and prevent naming conflicts.

- **`using` Directive**: A **shortcut** that imports an entire namespace, so you can use its types without writing their full address (e.g., `using HotelBookingAPI.Models;`).

- **`using` Alias**: A true **alias** for a single type (e.g., `using RoomModel = HotelBookingAPI.Models.Room;`). This is useful for resolving naming conflicts.

### Access Modifiers

These keywords control the visibility of your code.

| Modifier | Visibility | 
| :--- | :--- |
| `public` | Accessible from anywhere. |
| `internal` | Accessible only within the same project. |
| `protected` | Accessible within its class and by child classes. |
| `private` | Accessible only within the same class. |

### The `static` Keyword

The `static` keyword means there is only **one single copy** of that variable for the entire application. It is shared globally. A `static` variable is like the central DMV computer that issues plate numbers; there is only one, and the entire system shares it.

### The `readonly` Keyword

The `readonly` keyword ensures a variable can only be assigned a value **once**, either when it is declared or in a constructor. This prevents the variable from being reassigned to a new object. However, you can still change the contents of the object itself (e.g., you can add items to a `readonly` list).

## Data Models: `record` vs. `class`

For data models, modern C# provides the `record` keyword.

- **`record` (Modern)**:
  ```csharp
  public record Room(int Id, string Name);
  ```
  - Concise, immutable (safer), and has value-based equality.

- **`class` (Traditional)**:
  ```csharp
  public class Room 
  {
      public int Id { get; set; }
      public string Name { get; set; }
  }
  ```
  - More verbose and mutable by default.

**Recommendation**: Prefer using `record` for simple data models.

## Important Considerations

- **RESTful Resource Naming**: A core principle of REST API design is to have simple, predictable, and consistent URLs. The convention is to model your URLs as "resources."
  - **Use nouns, not verbs.** Your URLs should represent things, not actions.
  - **Use plural nouns for collections.** A list of all rooms should be `/rooms`, not `/room`.
  - **Use an ID for specific items.** To access a single room, you append its ID to the collection URL: `/rooms/{id}`.
  - **Use verbs for actions that don't fit CRUD:** `/rooms/{id}/checkout`.
  - **Use HTTP methods (verbs) to specify the action.** The same URL can be used for different actions based on the HTTP method.

| HTTP Method | URL | Action |
| :--- | :--- | :--- |
| `GET` | `/rooms` | Get a list of all rooms. |
| `GET` | `/rooms/{id}` | Get a single, specific room. |
| `POST` | `/rooms` | Create a new room. |
| `PUT` | `/rooms/{id}` | **Replace** an existing room completely. |
| `PATCH` | `/rooms/{id}` | **Partially update** an existing room. |
| `DELETE` | `/rooms/{id}` | Delete a specific room. |

- **Naming Conventions (Private Fields)**: For private fields within a class, it's a common C# convention to prefix their names with an underscore (`_`), e.g., `_roomService`.

- **Dependency Injection (DI)**: Use DI to provide services to your controllers.
- **Error Handling**: Implement a global error handling strategy.
- **Validation**: Validate incoming data.
- **Authentication and Authorization**: Secure your API.
- **Testing**: Write unit and integration tests.