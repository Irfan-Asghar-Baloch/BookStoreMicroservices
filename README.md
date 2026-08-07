# 📚 BookStore Microservices

A production-oriented BookStore application built with ASP.NET Core and React, demonstrating secure authentication, microservices communication, payment processing, asynchronous messaging, and automated notifications.

## 🚀 Key Features

- 🔐 JWT Authentication
- 👥 Role-Based Authorization
- 🏗️ Microservices Architecture
- 🔄 Service-to-Service Communication using HttpClient
- 💳 Stripe Payment Gateway
- 🔔 Stripe Webhooks for Payment Verification
- 🛡️ Duplicate Payment Protection using PaymentIntentId
- 📨 RabbitMQ for Asynchronous Communication
- 📧 Email Notifications
- 💬 WhatsApp Cloud API Notifications
- 📝 Approved WhatsApp Message Templates
- ⚛️ React Frontend
- 🛡️ Middleware & Error Handling
- 📊 Logging and Monitoring

## 🏗️ Architecture

```text
                    ┌──────────────┐
                    │ React Client │
                    └──────┬───────┘
                           │
                           ▼
                  ┌─────────────────┐
                  │   API Gateway   │
                  └────────┬────────┘
                           │
             ┌─────────────┴─────────────┐
             ▼                           ▼
      ┌──────────────┐             ┌──────────────┐
      │ Auth Service │             │ Order Service│
      └──────────────┘             └──────┬───────┘
                                          │
                                          ▼
                                  ┌──────────────┐
                                  │ Book Service │
                                  └──────────────┘

                           Order Payment
                                │
                                ▼
                         ┌─────────────┐
                         │    Stripe   │
                         └──────┬──────┘
                                │
                            Webhook
                                │
                                ▼
                         Payment Verified
                                │
                                ▼
                         ┌─────────────┐
                         │  RabbitMQ   │
                         └──────┬──────┘
                                │
                                ▼
                      ┌────────────────────┐
                      │ NotificationService│
                      └───────┬─────┬──────┘
                              │     │
                              ▼     ▼
                           Email  WhatsApp