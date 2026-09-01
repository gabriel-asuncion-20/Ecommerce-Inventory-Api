# 🛒 Ecommerce & Inventory Management RESTful Web API

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/Language-C%23-239120?logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![EF Core](https://img.shields.io/badge/ORM-Entity%20Framework%20Core-68217A)](https://docs.microsoft.com/en-us/ef/core/)
[![JWT Auth](https://img.shields.io/badge/Security-JWT%20Bearer-000000?logo=json-web-tokens)](https://jwt.io/)
[![Swagger](https://img.shields.io/badge/Documentation-Swagger%20OpenAPI-85EA2D?logo=swagger)](https://swagger.io/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Backend robusto, escalable y profesional desarrollado en **C# con ASP.NET Core** para la gestión integral de un comercio electrónico (E-Commerce) y control de stock de inventario en tiempo real.

---

## 📐 Arquitectura del Sistema y Flujo de Datos

El proyecto aplica los principios de **Clean Architecture (Arquitectura Limpia)** desacoplando la lógica de negocio, los modelos de dominio y la persistencia de datos mediante **Repository Pattern** y **Unit of Work**.

### 🔗 Diagrama de Conexión de Componentes

```text
 ┌─────────────────────────────────────────────────────────────────────────────────┐
 │                                   CLIENTE                                       │
 │              (Swagger UI / Frontend Web App / Mobile App / Postman)             │
 └───────────────────────────────────────┬─────────────────────────────────────────┘
                                         │  HTTP / HTTPS Request
                                         ▼
 ┌─────────────────────────────────────────────────────────────────────────────────┐
 │                                  CAPA DE API                                    │
 │  ┌───────────────────────────┐           ┌───────────────────────────────────┐  │
 │  │ Controllers (Rutas REST)  │ ────────> │ ExceptionHandlingMiddleware       │  │
 │  │ (Auth, Products, Orders)  │           │ (Formato RFC 7807 ProblemDetails) │  │
 │  └─────────────┬─────────────┘           └───────────────────────────────────┘  │
 └────────────────┼────────────────────────────────────────────────────────────────┘
                  │
                  ▼  Usa DTOs & Interfaces
 ┌─────────────────────────────────────────────────────────────────────────────────┐
 │                                 CAPA DE CORE                                    │
 │  ┌───────────────────────────┐           ┌───────────────────────────────────┐  │
 │  │ Domain Entities & Enums   │           │ DTOs & Query Filters              │  │
 │  │ (User, Product, Order)    │           │ (RegisterDto, ProductQueryFilter) │  │
 │  └───────────────────────────┘           └───────────────────────────────────┘  │
 └────────────────┼────────────────────────────────────────────────────────────────┘
                  │
                  ▼  Implementa Interfaces DI
 ┌─────────────────────────────────────────────────────────────────────────────────┐
 │                            CAPA DE INFRAESTRUCTURA                              │
 │  ┌───────────────────────────┐           ┌───────────────────────────────────┐  │
 │  │ Repository Pattern        │           │ Services                          │  │
 │  │ Generic Repository<T>     │           │ - AuthService (JWT + BCrypt)      │  │
 │  │ UnitOfWork                │           │ - PaymentGatewayService (Stripe)  │  │
 │  └─────────────┬─────────────┘           └───────────────────────────────────┘  │
 └────────────────┼────────────────────────────────────────────────────────────────┘
                  │
                  ▼  Entity Framework Core ORM
 ┌─────────────────────────────────────────────────────────────────────────────────┐
 │                                BASE DE DATOS                                    │
 │                     ApplicationDbContext (SQLite / SQL Server)                  │
 └─────────────────────────────────────────────────────────────────────────────────┘
```

---

## ⚡ Tecnologías Utilizadas y su Rol

| Tecnología | Rol en el Proyecto |
| :--- | :--- |
| **C# / .NET 10** | Lenguaje y framework de alto rendimiento para desarrollo Web API REST. |
| **ASP.NET Core Web API** | Exposición de endpoints RESTful controladores y ruteo HTTP. |
| **Entity Framework Core (EF Core)** | ORM para mapeo objeto-relacional, consultas LINQ optimizadas y migraciones. |
| **SQLite (Desarrollo)** | Motor de base de datos embebido sin dependencias externas (facilidad de despliegue local). |
| **JWT (JSON Web Tokens)** | Autenticación basada en claims e identificadores con firmas HMAC-SHA256. |
| **BCrypt.Net** | Hashing criptográfico seguro para almacenamiento de contraseñas. |
| **Repository & Unit of Work** | Patrones de diseño para aislamiento de capa de datos y transacciones atómicas. |
| **Swashbuckle Swagger UI** | Documentación interactiva de la API con botón de prueba `Bearer <token>`. |

---

## ✨ Características Principales

- 🔐 **Autenticación y Autorización por Roles**: Sistema de roles (`Admin` y `Customer`). Los clientes pueden navegar y comprar; los administradores gestionan inventario y estados de orden.
- 📦 **Control de Inventario en Tiempo Real**: Filtrado de productos por rango de precios, categorías, búsqueda por texto, ordenamiento dinámico y paginación.
- ⚡ **Actualización Rápida de Stock**: Endpoint especializado `PATCH /api/products/{id}/stock` para ajustar existencias de almacén.
- 🛒 **Carrito de Compras y Transacciones Atómicas**: Verificación automática de stock disponible al crear una orden. Descuento de stock en la misma transacción.
- 💳 **Pasarela de Pagos Simulada (Stripe/PayPal)**: Procesamiento de cobros por tarjeta, generación de IDs de transacción (`TXN_...`) y cambio automático de estado a `Paid`.
- 🛡️ **Manejo Global de Excepciones**: Middleware que captura errores inesperados y los estandariza en formato **RFC 7807 (ProblemDetails)**.
- 🌱 **Database Seeder**: Creación e inserción automática de datos iniciales en la primera ejecución.

---

## 📁 Estructura del Código

```text
EcommerceInventoryApi/
├── Core/                              # Capa de Dominio y Lógica Pura
│   ├── Entities/                      # Modelos de base de datos (User, Product, Order, Category, Payment)
│   ├── DTOs/                          # Objetos de Transferencia de Datos
│   └── Interfaces/                    # Contratos e Interfaces (IRepository, IUnitOfWork, IAuthService)
├── Infrastructure/                    # Capa de Persistencia y Servicios Externos
│   ├── Data/                          # ApplicationDbContext y DatabaseSeeder
│   ├── Repositories/                  # Implementación de Repository<T> y UnitOfWork
│   └── Services/                      # AuthService y PaymentGatewayService
├── API/                               # Capa de Presentación Web API
│   ├── Controllers/                   # Auth, Categories, Products, Orders, Payments
│   └── Middlewares/                   # ExceptionHandlingMiddleware
├── appsettings.json                   # Configuración del sistema y JWT
├── Program.cs                         # Configuración de Inyección de Dependencias y Middleware Pipeline
└── EcommerceInventoryApi.csproj       # Configuración del proyecto .NET
```

---

## 🔌 Resumen de Endpoints de la API

### 🗝️ Autenticación (`/api/auth`)
- `POST /api/auth/register` - Registro de nuevos usuarios.
- `POST /api/auth/login` - Inicio de sesión y entrega del token JWT Bearer.

### 📦 Inventario y Productos (`/api/products`)
- `GET /api/products` - Obtener productos (con búsqueda, filtros, ordenación y paginación).
- `GET /api/products/{id}` - Obtener detalle de un producto.
- `POST /api/products` - Crear un producto nuevo *(Rol Admin)*.
- `PUT /api/products/{id}` - Modificar un producto existente *(Rol Admin)*.
- `PATCH /api/products/{id}/stock` - Actualizar únicamente las existencias/stock *(Rol Admin)*.
- `DELETE /api/products/{id}` - Eliminar un producto *(Rol Admin)*.

### 📁 Categorías (`/api/categories`)
- `GET /api/categories` - Listar todas las categorías con conteo de productos.
- `POST /api/categories` - Crear nueva categoría *(Rol Admin)*.
- `PUT /api/categories/{id}` - Editar categoría *(Rol Admin)*.
- `DELETE /api/categories/{id}` - Eliminar categoría *(Rol Admin)*.

### 🛒 Órdenes de Compra (`/api/orders`)
- `POST /api/orders` - Crear orden de compra y descontar stock automáticamente.
- `GET /api/orders` - Listado de órdenes (`Customer` ve las suyas, `Admin` ve todas).
- `GET /api/orders/{id}` - Ver detalle de una orden por ID.
- `PATCH /api/orders/{id}/status` - Cambiar el estado de la orden (`Pending`, `Paid`, `Shipped`, `Cancelled`) *(Rol Admin)*.

### 💳 Pasarela de Pagos (`/api/payments`)
- `POST /api/payments/process` - Procesar cobro de tarjeta para una orden activa.

---

## 🔑 Usuarios de Prueba Iniciales (Database Seeder)

La base de datos SQLite se crea e inicializa automáticamente en la primera ejecución con los siguientes usuarios:

| Rol | Correo Electrónico | Contraseña | Permisos |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@ecommerce.com` | `Admin123!` | Acceso completo (CRUD productos, stock, estado órdenes). |
| **Customer** | `customer@ecommerce.com` | `Customer123!` | Compras, carrito de productos e historial propio. |

---

## ⚙️ Instalación y Ejecución Local

### Prerrequisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) o posterior.

### Pasos
1. Clonar el repositorio:
   ```bash
   git clone https://github.com/tu-usuario/aspnet-ecommerce-inventory-api.git
   cd aspnet-ecommerce-inventory-api
   ```
2. Restaurar dependencias y compilar:
   ```bash
   dotnet build
   ```
3. Ejecutar la aplicación:
   ```bash
   dotnet run
   ```
4. Abrir la interfaz interactiva de Swagger UI en tu navegador:
   ```text
   http://localhost:5200/swagger
   ```

> 💡 **Nota para la Autenticación en Swagger**: Al usar el botón **Authorize** en Swagger UI, pega **únicamente el token** (la cadena larga que empieza en `eyJ...`), ya que Swagger antepone automáticamente la palabra `Bearer`.

---

## 📄 Licencia

Este proyecto está bajo la Licencia MIT.
