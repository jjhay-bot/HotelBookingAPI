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

### Domain Models vs. DTOs (Data Transfer Objects)

It's crucial to distinguish between your internal domain models and the data structures you use for API communication.

- **Domain Model (e.g., `User.cs`)**:
  - Represents the **internal, authoritative definition** of an entity within your application (e.g., how a user is stored in your database).
  - Contains all properties relevant to your application's logic, including sensitive data like `PasswordHash`.

- **DTO / Input Model (e.g., `RegisterRequest.cs`)**:
  - Represents the **data contract for API requests or responses**.
  - Contains only the data necessary for the specific API operation.
  - For input DTOs, sensitive data like plain passwords are included (as they come from the client), but never `Id`s or hashed passwords (which are server-generated/managed).

**Why both? (Separation of Concerns)**
- **Security**: Prevents exposing internal/sensitive data (like `PasswordHash`) to clients.
- **Flexibility**: Allows your API's input/output format to differ from your internal data storage.
- **Clarity**: Clearly defines what data is expected for each API operation.
- **Validation**: Enables specific validation rules for API requests.

## Authentication (JWT)

JSON Web Token (JWT) is a popular and robust method for securing modern APIs. Here's a high-level overview of the flow and implementation steps:

### JWT Flow
1.  **Login**: User sends credentials (username/password) to a `/login` endpoint.
2.  **Token Issuance**: API validates credentials, generates a signed JWT, and sends it back to the client.
3.  **Client Storage**: Client stores the JWT.
4.  **Subsequent Requests**: Client sends the JWT in the `Authorization` header (as a "Bearer" token) with requests to protected endpoints.
5.  **Token Validation**: API validates the JWT's signature and extracts user information.

### Implementation Steps
1.  **User Model & Service**: Define a `User` model (e.g., `User.cs` with `Username`, `PasswordHash`). Create a `UserService` for user management and password hashing.
2.  **JWT Configuration**: Add necessary NuGet packages (e.g., `Microsoft.AspNetCore.Authentication.JwtBearer`). Define JWT settings (secret key, issuer, audience, expiration) in `appsettings.json`.
3.  **Authentication Scheme**: Configure JWT Bearer authentication in `Program.cs`.
4.  **Login Endpoint**: Create a new controller (e.g., `AuthController.cs`) with a `/login` endpoint that generates and returns a JWT upon successful login.
5.  **Protect Endpoints**: Apply the `[Authorize]` attribute to controllers or action methods to require a valid JWT for access.

### Controlling Access with `[Authorize]` and `[AllowAnonymous]`

These attributes are used to define which parts of your API require authentication and which are publicly accessible.

- **`[Authorize]`**
  - **Purpose**: Requires that the user making the request is authenticated (i.e., has a valid token).
  - **Placement**: Can be applied at the **controller level** (protects all action methods within that controller) or at the **individual action method level**.
  - **Example**: `[Authorize]` on `RoomController` means all `Room` operations (Create, Update, Delete) require a token.

- **`[AllowAnonymous]`**
  - **Purpose**: Overrides `[Authorize]` to explicitly allow unauthenticated access.
  - **Placement**: Applied to **individual action methods**.
  - **Example**: If `RoomController` is `[Authorize]`, adding `[AllowAnonymous]` to `GetAll()` and `GetById()` methods makes them publicly viewable without a token.
  - **Common Use**: Used on `Register` and `Login` endpoints in `AuthController` so users can get a token in the first place.

**Important**: Always use HTTPS in production to protect tokens during transmission. Keep your JWT secret key absolutely confidential.

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
