RaceDay – Race Event Management System

1. Project Description

RaceDay is a web-based race event management system developed using ASP.NET Core Web API and ASP.NET Core MVC.

The system allows organisers to create and manage race events, categories, participant enrolments and race results. Participants can browse events, enrol in available categories, view their enrolments and results, and manage their profiles.

2. Technologies Used

* C#
* ASP.NET Core Web API
* ASP.NET Core MVC
* .NET 8
* Entity Framework Core
* Microsoft SQL Server LocalDB
* SQL Server Management Studio
* JWT Authentication
* Cookie Authentication
* Session Management
* Bootstrap 5
* xUnit Testing
* Git and GitHub

3. Solution Structure

The solution contains three main projects:

* RaceDay: ASP.NET Core Web API responsible for application data and business operations.
* RaceDay.Web: ASP.NET Core MVC frontend that communicates with the API.
* RaceDay.Test: Automated tests for the API.

4. System Features

Organiser Features

* Register and log in as an organiser.
* View the organiser dashboard and event statistics.
* Create, view, edit and delete race events.
* Manage event categories.
* View participant enrolments.
* Update enrolment statuses.
* Capture and view race results.

Participant Features

* Register and log in as a participant.
* View the participant dashboard.
* Browse available race events.
* View event categories.
* Enrol in race categories.
* View personal enrolments.
* View race results.
* View and update profile information.

Security Features

* JWT authentication between the MVC application and API.
* Cookie-based authentication in the MVC application.
* Session management.
* Role-based authorisation.
* Protected organiser and participant pages.
* Logout functionality.

5. Database

The application uses Microsoft SQL Server LocalDB.

The API uses Entity Framework Core Code First to manage the database structure.

The database contains entities for users, roles, events, event categories, enrolments and results.

6. How to Run the Application

1. Open the RaceDay solution in Visual Studio 2022.
2. Ensure the required .NET SDK and SQL Server LocalDB are installed.
3. Check the API database connection string in appsettings.json.
4. Check the MVC API base URL in RaceDay.Web/appsettings.json.
5. Set the API and MVC projects as startup projects.
6. Start the application using Visual Studio.
7. Open the MVC application in your browser.
8. Register an organiser or participant account.
9. Log in to access the relevant dashboard.

The API Swagger interface can be used to inspect and test API endpoints.

7. Authentication and Roles

RaceDay supports two self-registration roles:

* Organiser
* Participant

Administrators are managed separately and cannot self-register through the normal registration page.

Users receive access to features according to their assigned roles.

8. Version Control

The project is maintained using Git and GitHub.

Development work for Part 3 is maintained on the Part3 branch of the RaceDay-2 repository.

9. Conclusion

RaceDay provides a centralised platform for managing race events and participant activities. The MVC frontend communicates with the Web API, while authentication and role-based authorisation help protect the application’s features.