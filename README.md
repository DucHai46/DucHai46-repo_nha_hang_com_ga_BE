# Restaurant Management System Backend

This is the backend system for a restaurant management application, built using .NET 6.0. The system is designed with a microservices architecture and includes multiple services for different functionalities.

## 🚀 Technology Stack

- **Framework**: .NET 6.0
- **Database**: MongoDB
- **Authentication**: JWT (JSON Web Tokens)
- **API Documentation**: Swagger/OpenAPI
- **Containerization**: Docker
- **Reverse Proxy**: Nginx

## 🏗️ Project Structure

The project consists of three main microservices:

1. **Main Service** (`repo_nha_hang_com_ga_BE`)
   - Core business logic
   - API endpoints
   - MongoDB integration

2. **Auth Service** (`auth.service`)
   - User authentication
   - Authorization
   - JWT token management

3. **File Service** (`file.service`)
   - File upload/download
   - File management

## 🛠️ Prerequisites

- .NET 6.0 SDK
- Docker and Docker Compose
- MongoDB
- Nginx (for production deployment)

## 🚀 Getting Started

1. **Clone the repository**
   ```bash
   git clone [repository-url]
   cd repo_nha_hang_com_ga_BE
   ```

2. **Build and run using Docker Compose**
   ```bash
   docker-compose up -d
   ```

3. **Access the services**
   - Main Service: http://localhost:8080
   - Auth Service: http://localhost:8081
   - File Service: http://localhost:8082

## 🔧 Configuration

The application uses the following configuration files:
- `appsettings.json` - Main configuration
- `appsettings.Development.json` - Development-specific settings
- `docker-compose.yml` - Docker services configuration

## 📦 Dependencies

- AutoMapper.Extensions.Microsoft.DependencyInjection (12.0.0)
- Microsoft.AspNetCore.Authentication.JwtBearer (6.0.36)
- MongoDB.Driver (3.2.1)
- Swashbuckle.AspNetCore (6.5.0)

## 🔐 Security

- JWT-based authentication
- HTTPS support through Nginx
- Secure file handling

## 🐳 Docker Deployment

The application is containerized using Docker with the following services:
- Nginx reverse proxy
- Main service container
- Auth service container
- File service container

## 📝 API Documentation

API documentation is available through Swagger UI at:
- Main Service: http://localhost:8080/swagger
- Auth Service: http://localhost:8081/swagger
- File Service: http://localhost:8082/swagger

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Create a new Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.
