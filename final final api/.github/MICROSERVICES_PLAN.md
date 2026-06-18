# Microservices Plan for ChineseAuction API

## Overview
This document outlines the plan to refactor the ChineseAuction API into a microservices architecture.

---

## Microservices Breakdown
1. **User Service**:n
   - Handles user management (registration, login, profile updates).
   - Endpoints:
     - `POST /users/register`
     - `POST /users/login`
     - `GET /users/{id}`

2. **Gift Service**:
   - Manages gifts and their details.
   - Endpoints:
     - `GET /gifts`
     - `POST /gifts`
     - `PUT /gifts/{id}`
     - `DELETE /gifts/{id}`

3. **Order Service**:
   - Handles orders and transactions.
   - Endpoints:
     - `POST /orders`
     - `GET /orders/{id}`
     - `GET /orders/user/{userId}`

4. **Lottery Service**:
   - Manages lottery-related operations.
   - Endpoints:
     - `POST /lotteries`
     - `GET /lotteries`
     - `GET /lotteries/{id}`

5. **Auth Service**:
   - Handles authentication and token management.
   - Endpoints:
     - `POST /auth/token`
     - `POST /auth/validate`

---

## Architecture
- **API Gateway**:
  - Centralized entry point for all requests.
  - Routes requests to the appropriate microservice.

- **Database**:
  - Each microservice has its own database to ensure data isolation.

- **Communication**:
  - Services communicate via REST APIs or a message broker (e.g., RabbitMQ).

---

## Steps to Implement
1. Split the current project into separate ASP.NET Core Web API projects.
2. Define contracts (DTOs) for communication between services.
3. Implement an API Gateway for routing.
4. Set up Docker for containerization.
5. Use Kubernetes for orchestration (optional).

---

## Tools and Technologies
- **Framework**: .NET 8
- **Database**: SQL Server (one per service)
- **Message Broker**: RabbitMQ (optional)
- **API Gateway**: Ocelot or YARP
- **Authentication**: JWT

---

## Notes
- Ensure each service is independently deployable.
- Use CI/CD pipelines for automated deployment.
- Monitor services using tools like Prometheus and Grafana.