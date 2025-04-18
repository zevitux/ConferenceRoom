# Conference Room Booking System

A project designed to help manage meeting room availability, reservations, and scheduling in an organized and efficient way.

---

## 🚀 Technologies Used

- ASP.NET Core Web API (.NET 9)
- Entity Framework Core
- SQL Server (via Docker)
- JWT Authentication
- Repository Pattern
- SOLID Principles
- Serilog (for logging)

---

## 📌 Features

- Room registration and management
- Booking creation and conflict checking
- User authentication and authorization with JWT
- Clean architecture with separation of concerns
- Logging and error handling

---

## 📦 How to Run

1. Clone the repository:
   ```bash
   git clone https://github.com/zevitux/conference-room.git
   ```

2. Navigate to the project directory:
   ```bash
   cd conference-room
   ```

3. Create and run the SQL Server container:
   ```bash
   docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 -d mcr.microsoft.com/mssql/server
   ```

4. Update the connection string in `appsettings.json`.

5. Apply migrations and run the application:
   ```bash
   dotnet ef database update
   dotnet run

---

## 📄 License

This project is for educational purposes and is open for contributions.
