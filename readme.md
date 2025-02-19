# Personal Project

## Overview

This project is a comprehensive web application built using .NET Core, implementing a microservices architecture. It consists of multiple APIs (Auth, Course, Student) and an API Gateway for centralized routing and authentication.

## Demo
https://github.com/VipPro67/PersonalProjectUI

## Project Structure

The project is organized into several key components:

- **ApiWebApp**: API Gateway using Ocelot
- **AuthApi**: Handles user authentication and authorization
- **CourseApi**: Manages course-related operations
- **StudentApi**: Handles student-related functionalities
- **Test Projects**: Corresponding test projects for each API

## Key Features

- Microservices Architecture
- JWT Authentication
- API Gateway with Ocelot
- Entity Framework Core for data access
- Serilog for structured logging
- FluentValidation for input validation
- AutoMapper for object mapping
- Unit Testing with xUnit
- gRPC for request between service
- HybridCache with MemCache and Redis
