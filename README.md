# Fleet Management Microservices

A microservices-based backend system for managing fleet operations such as vehicles, drivers, trips and notifications.

Key Features
============

User registration and login
JWT-based authentication
Vehicle management and location tracking
Driver registration and vehicle assignment
Trip creation and completion
Redis caching for vehicle data
Kafka and RabbitMQ event messaging
Notification management
Separate databases for independent services

Services
========
------------------------------------------------------------
Service	            Responsibility	              Database
------------------------------------------------------------
AuthService	    Registration and login	           SQLite
VehicleService	Vehicle records and locations	     SQL
DriverService	  Driver records and assignments	   MongoDB
TripService	    Trip records and event processing	 SQL
AlertService	  Notifications and alerts	         SQLite
----------------------------------------------------------

Technology Stack
===============

C# and ASP.NET Core
Entity Framework Core
JWT and BCrypt
MongoDB
Redis
RabbitMQ
Apache Kafka
SQLite

High-Level Operation
==================
  User
  ↓ Login
AuthService
  ↓ JWT Token
Vehicle, Driver and Trip Services
  ↓
Database, Redis, Kafka and RabbitMQ
  ↓
AlertService
  ↓
Notifications

Typical Fleet Workflow
=======================
A user logs in.
A vehicle is registered.
A driver is registered.
The vehicle is assigned to the driver.
A trip is started.
Vehicle location and trip data are updated.
The trip is completed.
Kafka and RabbitMQ events generate notifications.

Architecture Characteristics
===========================
Independent backend services
REST API communication
Separate service data storage
Event-based communication
Redis caching
JWT authorization
Repository and service-based application structure
