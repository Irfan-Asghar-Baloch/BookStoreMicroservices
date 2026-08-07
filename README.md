# 📚 BookStore Microservices

A backend-focused BookStore Microservices application built with **ASP.NET Core**, demonstrating secure authentication, role-based authorization, service-to-service communication, Stripe payment processing, webhook-based payment verification, RabbitMQ messaging, and automated Email and WhatsApp notifications.

---

## 🚀 Project Overview

This project demonstrates a real-world backend architecture for an online bookstore using a **Microservices Architecture**.

The application currently focuses on backend services and integrations including:

- User Authentication
- JWT Authentication
- Role-Based Authorization
- Book Management
- Order Processing
- Service-to-Service Communication
- Stripe Payment Gateway
- Stripe Webhooks
- Duplicate Payment Protection
- RabbitMQ
- Background Notification Processing
- Email Notifications
- WhatsApp Cloud API
- WhatsApp Message Templates
- Middleware
- Error Handling
- Logging

> 🚧 Frontend development is planned separately and is not included in the current version of this repository.

---

# 🏗️ Architecture

```text
                    ┌──────────────────┐
                    │     Client       │
                    │  Postman/Swagger │
                    └────────┬─────────┘
                             │
                             ▼
                  ┌─────────────────────┐
                  │   ASP.NET Core APIs │
                  └──────────┬──────────┘
                             │
             ┌───────────────┼────────────────┐
             │               │                │
             ▼               ▼                ▼
      ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
      │ AuthService │ │ BookService │ │ OrderService│
      └─────────────┘ └─────────────┘ └──────┬──────┘
                                             │
                                             │ HttpClient
                                             ▼
                                      ┌──────────────┐
                                      │ BookService  │
                                      └──────────────┘

                                      Order Payment
                                           │
                                           ▼
                                     ┌───────────┐
                                     │  Stripe   │
                                     └─────┬─────┘
                                           │
                                        Webhook
                                           │
                                           ▼
                                  Payment Verification
                                           │
                                           ▼
                                     ┌───────────┐
                                     │ RabbitMQ  │
                                     └─────┬─────┘
                                           │
                                           ▼
                              ┌────────────────────────┐
                              │   NotificationService  │
                              └───────────┬────────────┘
                                          │
                              ┌───────────┴───────────┐
                              ▼                       ▼
                         ┌─────────┐             ┌──────────┐
                         │  Email  │             │ WhatsApp │
                         └─────────┘             └──────────┘
🔐 Authentication & Authorization

The application uses JWT-based authentication and role-based authorization.

Authentication

Authentication verifies the identity of the user.

After successful login, the API generates a JWT containing the required user claims.

The client sends the token using the Authorization header:

Authorization: Bearer <JWT>
Authorization

Authorization determines what an authenticated user is allowed to access.

The application currently uses roles such as:

Admin
User

Example:

Admin
 ├── Add Book
 ├── Update Book
 ├── Delete Book
 └── View Books

User
 ├── View Books
 ├── View Book Details
 └── Create Orders
📚 Book Service

The Book Service is responsible for book-related operations.

Typical operations include:

Get all books
Get book by ID
Add book
Update book
Delete book

Access to protected operations is controlled through JWT authentication and role-based authorization.

🛒 Order Service

The Order Service handles order-related operations.

The service communicates with other services using HTTP-based service-to-service communication.

For example:

OrderService
      │
      │ HttpClient
      ▼
BookService
      │
      ▼
Book Data

Each service is responsible for its own functionality rather than directly accessing another service's database.

💳 Stripe Payment Integration

The project integrates Stripe Payment Gateway for payment processing.

The payment flow is:

Create Order
     ↓
Initialize Stripe Payment
     ↓
Customer Completes Payment
     ↓
Stripe Processes Payment
     ↓
Stripe Webhook
     ↓
Payment Verification
     ↓
Update Payment / Order
🔔 Stripe Webhooks

The application uses Stripe webhooks to verify successful payments.

The backend does not rely only on the frontend payment response.

This is important because a customer can:

Close the payment page
Lose internet connection
Refresh the page
Leave the payment process unexpectedly

Stripe independently sends the payment event to the backend.

Example:

Stripe
   │
   │ payment_intent.succeeded
   ▼
Webhook Endpoint
   │
   ▼
Verify Payment
   │
   ▼
Update Order Status

This provides a more reliable payment confirmation mechanism.

🛡️ Duplicate Payment Protection

Webhook events can potentially be received more than once.

To prevent duplicate payment records, the application checks the Stripe:

PaymentIntentId

before creating a new payment record.

Flow:

Webhook Received
       ↓
Get PaymentIntentId
       ↓
Check Existing Payment
       ↓
   ┌───┴────┐
   │        │
 Exists    Not Exists
   │        │
   ▼        ▼
 Return    Save Payment
   │        │
   └────┬───┘
        ▼
   Update Order

This provides idempotent payment processing.

📨 RabbitMQ

RabbitMQ is used for asynchronous communication between services.

After a successful payment, a payment-success event is published to RabbitMQ.

Example event:

{
  "OrderId": 4,
  "UserId": 1,
  "Amount": 6234,
  "Currency": "usd",
  "Status": "succeeded"
}

The NotificationService consumes this event.

Payment Success
       ↓
Publish Event
       ↓
RabbitMQ
       ↓
NotificationService
       ↓
Email + WhatsApp

This keeps notification processing separate from the main payment flow.

⚙️ NotificationService

The NotificationService is responsible for sending customer notifications after successful payment.

It consumes payment-success events from RabbitMQ.

The service currently supports:

Email notifications
WhatsApp notifications
📧 Email Notifications

After successful payment, the NotificationService sends an email to the customer.

Example:

Payment Successful

Your payment has been received successfully.

Email delivery is handled by the notification service.

💬 WhatsApp Notifications

The application integrates Meta WhatsApp Cloud API.

WhatsApp messages are sent through the Meta Graph API.

The project uses:

WhatsApp Cloud API
Phone Number ID
Access Token
Approved WhatsApp message templates
Template parameters

Example notification flow:

Payment Successful
       ↓
RabbitMQ
       ↓
NotificationService
       ↓
WhatsApp Cloud API
       ↓
Customer WhatsApp

The application also handles the WhatsApp 24-hour messaging policy by using approved templates where required.

🔄 Background Processing

The NotificationService uses ASP.NET Core BackgroundService to continuously consume RabbitMQ messages.

The basic flow is:

RabbitMQ Queue
      ↓
BackgroundService
      ↓
Receive Event
      ↓
Send Email
      ↓
Send WhatsApp

This allows notification processing to happen asynchronously instead of making the main API request wait for external notification services.

🧩 Middleware & Error Handling

The backend uses middleware-based request processing and error handling.

Middleware can be used for concerns such as:

Exception handling
Request processing
Logging
Authentication
Authorization

This helps keep controllers focused on business logic.

📊 Logging

Logging is used to monitor application behavior and troubleshoot issues.

Examples include:

RabbitMQ consumer activity
Received events
HTTP requests
External API responses
Notification processing
Payment-related operations
🛠️ Technology Stack
Backend
C#
ASP.NET Core
ASP.NET Core Web API
Microservices
Entity Framework Core
Dapper
SQL Server
JWT Authentication
Role-Based Authorization
HttpClient
BackgroundService
Messaging
RabbitMQ
Payment
Stripe
Stripe Webhooks
Notifications
SMTP / Email
Meta WhatsApp Cloud API
WhatsApp Message Templates
Development & Testing
Visual Studio
Swagger
Postman
Git
GitHub
ngrok
📁 Project Structure
BookStore-Microservices/
│
├── AuthService/
│
├── BookService/
│
├── OrderService/
│
├── PaymentService/
│
├── NotificationService/
│
├── README.md
│
└── .gitignore

Update the folder names above if your actual solution structure is different.

⚙️ Configuration

The application requires configuration for external services and infrastructure.

Typical configuration includes:

SQL Server
JWT
RabbitMQ
Stripe
SMTP / Email
WhatsApp Cloud API

Example configuration structure:

{
  "RabbitMQ": {
    "HostName": "localhost"
  },
  "Stripe": {
    "SecretKey": ""
  },
  "WhatsApp": {
    "AccessToken": "",
    "PhoneNumberId": "",
    "ApiVersion": "v23.0"
  }
}
🔒 Security

Never commit real credentials to GitHub.

Do not upload:

API Tokens
Passwords
Stripe Secret Keys
WhatsApp Access Tokens
JWT Secrets
SMTP Passwords
Database Passwords

Use environment variables, .NET User Secrets, or another secure secrets-management solution.

🧪 API Testing

The backend can currently be tested using:

Swagger
Postman

Typical flow:

Register
   ↓
Login
   ↓
Receive JWT
   ↓
Authorize Request
   ↓
Manage / View Books
   ↓
Create Order
   ↓
Initialize Payment
   ↓
Complete Stripe Payment
   ↓
Stripe Webhook
   ↓
Payment Verification
   ↓
RabbitMQ
   ↓
NotificationService
   ↓
Email + WhatsApp
📸 Screenshots

Screenshots can be added here to demonstrate the working backend.

Swagger

Add Swagger screenshot here.

Postman

Add Postman screenshot here.

Stripe Payment

Add Stripe payment screenshot here.

Stripe Webhook

Add Stripe webhook screenshot here.

RabbitMQ

Add RabbitMQ screenshot here.

Email Notification

Add email screenshot here.

WhatsApp Notification

Add WhatsApp screenshot here.

🎯 What This Project Demonstrates

This project demonstrates practical experience with:

Microservices architecture
Secure API development
JWT authentication
Role-based authorization
RESTful APIs
Service-to-service communication
SQL Server
Entity Framework Core
Dapper
Stripe payment integration
Webhook-based payment verification
Idempotent payment processing
RabbitMQ messaging
Background processing
Email integration
WhatsApp Cloud API integration
Middleware
Logging
External API integration
💡 Key Engineering Decisions
Why Microservices?

Services are separated by responsibility and communicate through APIs or asynchronous messaging instead of directly accessing another service's database.

Why HttpClient?

HttpClient is used for service-to-service communication when one service requires data or functionality from another service.

Why Stripe Webhooks?

The backend needs an authoritative payment confirmation from Stripe instead of relying only on the frontend payment result.

Why PaymentIntentId?

PaymentIntentId is used to identify an existing Stripe payment and prevent duplicate payment records when duplicate webhook events occur.

Why RabbitMQ?

RabbitMQ allows notification processing to happen asynchronously and separates notification delivery from the main payment workflow.

Why BackgroundService?

BackgroundService allows the NotificationService to continuously consume and process messages from RabbitMQ without requiring a normal HTTP endpoint for every notification.

🚧 Current Status
Backend

Completed

Authentication
Authorization
Book APIs
Order processing
Stripe integration
Stripe webhook handling
Duplicate payment protection
RabbitMQ integration
Email notifications
WhatsApp notifications
Notification background processing
Frontend

Not implemented yet.

The React frontend will be developed separately as the next phase of the project.

🔮 Future Improvements

Planned improvements may include:

React frontend
Docker containerization
CI/CD pipeline
Automated tests
Redis caching improvements
Advanced RabbitMQ retry handling
Dead-letter queues
Distributed tracing
Cloud deployment
Production monitoring
👨‍💻 Author

Irfan Asghar

.NET Backend Developer

ASP.NET Core | Web API | Microservices | C# | SQL Server | RabbitMQ | JWT | Redis

Open to Remote Opportunities.