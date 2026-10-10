RaceDay – Part 2: Event Management API

Project Overview

RaceDay is an event management system developed using ASP.NET Core Web API and C#. The system provides functionality for managing running events, event categories, user accounts, enrolments, and race results.

Part 2 builds on the database design and documentation from Part 1 by implementing the API, database integration, authentication, authorisation, and automated testing.

Project Objectives

The objectives of RaceDay Part 2 are to:

* Develop a RESTful API for managing running events.
* Implement user registration and login.
* Support role-based access for administrators, organisers, and participants.
* Manage events and their categories.
* Allow participants to enrol in event categories.
* Manage and record race results.
* Integrate the application with a database using Entity Framework Core.
* Test API functionality using automated unit and integration tests.
* Use GitHub Actions to automate building and testing.

Technologies Used

Technology	Purpose
C#	Main programming language
ASP.NET Core Web API	API development
.NET 8	Application framework
Entity Framework Core	Database access and object-relational mapping
SQL Server	Application database
Swagger / OpenAPI	API documentation and endpoint testing
xUnit	Automated testing
Microsoft.AspNetCore.Mvc.Testing	API integration testing
Entity Framework Core InMemory	Isolated in-memory database for tests
GitHub	Source control and project collaboration
GitHub Actions	Automated build and test workflow

Main Features

1. User Authentication and Authorisation

The API supports user registration and login.

The system includes three roles:

* Admin: Administrative access.
* Organiser: Creates and manages events.
* Participant: Registers to participate in events.

Authentication and role-based authorisation help restrict access to protected API operations.

2. Event Management

Event functionality includes retrieving events and managing event information.

Organisers can create events and update events they are authorised to manage.

3. Event Categories

Events can contain categories that participants can select when enrolling.

4. Participant Enrolments

The system manages participant enrolments in event categories. The database design prevents a participant from enrolling in the same category more than once.

5. Race Results

The API includes functionality for managing race results associated with enrolments.

Database Design

The application uses Entity Framework Core with an ApplicationDbContext.

The main database entities are:

* Role – stores user roles.
* User – stores user account information.
* Event – stores event details.
* EventCategory – stores categories associated with events.
* Enrollment – records participant enrolments.
* Result – stores results associated with enrolments.

The database configuration defines relationships, foreign keys, unique indexes, and delete behaviours to support data integrity.

The application also defines initial role data for Admin, Organiser, and Participant.
