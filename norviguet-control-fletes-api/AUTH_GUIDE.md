# API de Autenticación - Guía de Uso (con Cookies HttpOnly)

## ?? Seguridad Implementada

Esta API utiliza un **enfoque híbrido de tokens** para máxima seguridad:

- ? **Access Token**: Se retorna en JSON (debe guardarse en memoria en el frontend)
- ? **Refresh Token**: Se envía en una **cookie HttpOnly segura** (no accesible por JavaScript)

### Ventajas de este enfoque:
- ??? Protección contra XSS: El refresh token no puede ser robado por scripts maliciosos
- ? Access tokens de corta duración (15 min) minimizan el impacto si son comprometidos
- ?? Cookies con flags `Secure`, `HttpOnly`, y `SameSite=Strict`

---

## Endpoints Implementados

### 1. Registro de Usuario
**POST** `/api/auth/register`

**Body:**
```json
{
  "name": "Juan Pérez",
  "email": "juan@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!"
}
```

**Respuesta exitosa (201 Created):**
```json
{
  "id": 1,
  "createdAt": "2026-01-28T10:00:00Z",
  "name": "Juan Pérez",
  "email": "juan@example.com",
  "role": "Pending"
}
```

**Respuesta de error (409 Conflict):**
```json
{
  "status": 409,
  "title": "Business rule violation",
  "detail": "Email already exists."
}
```

---

### 2. Inicio de Sesión
**POST** `/api/auth/login`

**Body:**
```json
{
  "email": "juan@example.com",
  "password": "Password123!",
  "rememberMe": true
}
```

**Respuesta exitosa (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "accessTokenExpiresAt": "2026-01-28T10:15:00Z"
}
```

**Cookie establecida automáticamente:**
```
Set-Cookie: refreshToken=kxv7Q8ZP9mN3rL5jK2wT6bH...; HttpOnly; Secure; SameSite=Strict; Expires=...
```

**Notas:**
- El `accessToken` expira en 15 minutos (debe guardarse en memoria, no en localStorage)
- El `refreshToken` se envía automáticamente en una cookie HttpOnly
- La cookie expira en 7 días (o 30 días si `rememberMe` es `true`)
- **El navegador enviará automáticamente la cookie en futuras peticiones**

**Respuesta de error (401 Unauthorized):**
```json
{
  "status": 401,
  "title": "Unauthorized",
  "detail": "Invalid email or password."
}
```

---

### 3. Renovar Token
**POST** `/api/auth/refresh`

**Body:** ? **No requiere body** (el refresh token se lee desde la cookie)

**Headers requeridos:**
```
Cookie: refreshToken=kxv7Q8ZP9mN3rL5jK2wT6bH...
```
(El navegador envía esto automáticamente)

**Respuesta exitosa (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "accessTokenExpiresAt": "2026-01-28T10:30:00Z"
}
```

**Cookie actualizada automáticamente:**
```
Set-Cookie: refreshToken=pL4mV9XK3zR6nN2jT8wB5hK...; HttpOnly; Secure; SameSite=Strict; Expires=...
```

**Respuesta de error (401 Unauthorized):**
```json
{
  "status": 401,
  "title": "Unauthorized",
  "detail": "Invalid or expired refresh token."
}
```

---

### 4. Revocar Token (Cerrar Sesión)
**POST** `/api/auth/revoke`

**Body:** ? **No requiere body** (el refresh token se lee desde la cookie)

**Headers requeridos:**
```
Cookie: refreshToken=kxv7Q8ZP9mN3rL5jK2wT6bH...
```

**Respuesta exitosa (204 No Content):**
Sin contenido en el cuerpo de la respuesta.
La cookie `refreshToken` se elimina automáticamente.

**Respuesta de error (404 Not Found):**
```json
{
  "status": 404,
  "title": "Resource not found",
  "detail": "Refresh token not found or already revoked."
}
```

---

## Validaciones del Registro

### Nombre (`name`)
- Requerido
- Longitud mínima: 2 caracteres
- Longitud máxima: 30 caracteres
- Solo permite letras, espacios, apóstrofes y guiones

### Email (`email`)
- Requerido
- Debe ser una dirección de email válida

### Contraseña (`password`)
- Requerida
- Longitud mínima: 8 caracteres
- Longitud máxima: 30 caracteres
- Debe contener al menos:
  - Una letra mayúscula
  - Un número
  - Un carácter especial

### Confirmar Contraseña (`confirmPassword`)
- Debe coincidir con el campo `password`

---

## Uso del Access Token

Para usar los endpoints protegidos, incluye el `accessToken` en el header de autorización:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Roles de Usuario Disponibles
- `Pending` - Usuario recién registrado (por defecto)
- `Admin` - Administrador
- `Logistics` - Logística
- `Purchasing` - Compras
- `Payments` - Pagos

---

## ?? Flujo Recomendado de Autenticación

### 1. **Login (Inicio de Sesión)**
```javascript
// Frontend: React/Vue/Angular
const response = await fetch('/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  credentials: 'include', // ?? IMPORTANTE: Permite enviar/recibir cookies
  body: JSON.stringify({
    email: 'user@example.com',
    password: 'Password123!',
    rememberMe: true
  })
});

const { accessToken, accessTokenExpiresAt } = await response.json();

// ? Guarda el access token en MEMORIA (no en localStorage)
// Ejemplo usando React Context o Vue Store
setAccessToken(accessToken);
```

### 2. **Peticiones Autenticadas**
```javascript
// Usa el access token en cada petición
const response = await fetch('/api/customers', {
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json'
  },
  credentials: 'include' // Envía la cookie automáticamente
});
```

### 3. **Refresh Token (Cuando el Access Token Expira)**
```javascript
// Detecta error 401 y renueva el token
const response = await fetch('/api/customers', {
  headers: { 'Authorization': `Bearer ${accessToken}` },
  credentials: 'include'
});

if (response.status === 401) {
  // Intenta renovar el token
  const refreshResponse = await fetch('/api/auth/refresh', {
    method: 'POST',
    credentials: 'include' // ?? IMPORTANTE: Envía la cookie con el refresh token
  });

  if (refreshResponse.ok) {
    const { accessToken: newAccessToken } = await refreshResponse.json();
    setAccessToken(newAccessToken);
    
    // Reintenta la petición original
    return fetch('/api/customers', {
      headers: { 'Authorization': `Bearer ${newAccessToken}` },
      credentials: 'include'
    });
  } else {
    // Refresh token expirado o inválido ? redirigir al login
    redirectToLogin();
  }
}
```

### 4. **Logout (Cierre de Sesión)**
```javascript
await fetch('/api/auth/revoke', {
  method: 'POST',
  credentials: 'include' // Envía la cookie para revocarla
});

// Limpia el access token de memoria
setAccessToken(null);

// Redirige al login
window.location.href = '/login';
```

---

## ?? Configuración del Frontend

### Axios (Interceptor automático)
```javascript
import axios from 'axios';

axios.defaults.withCredentials = true; // Habilita cookies globalmente

// Interceptor para renovar token automáticamente
axios.interceptors.response.use(
  response => response,
  async error => {
    const originalRequest = error.config;
    
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        const { data } = await axios.post('/api/auth/refresh');
        const { accessToken } = data;
        
        // Actualiza el token
        setAccessToken(accessToken);
        originalRequest.headers['Authorization'] = `Bearer ${accessToken}`;
        
        return axios(originalRequest);
      } catch (refreshError) {
        // Redirigir al login
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }
    
    return Promise.reject(error);
  }
);
```

### Fetch API
```javascript
// Wrapper personalizado para manejar refresh automáticamente
async function authFetch(url, options = {}) {
  options.credentials = 'include';
  options.headers = {
    ...options.headers,
    'Authorization': `Bearer ${getAccessToken()}`
  };
  
  let response = await fetch(url, options);
  
  if (response.status === 401) {
    // Intentar refresh
    const refreshResponse = await fetch('/api/auth/refresh', {
      method: 'POST',
      credentials: 'include'
    });
    
    if (refreshResponse.ok) {
      const { accessToken } = await refreshResponse.json();
      setAccessToken(accessToken);
      
      // Reintentar con nuevo token
      options.headers['Authorization'] = `Bearer ${accessToken}`;
      response = await fetch(url, options);
    } else {
      window.location.href = '/login';
      throw new Error('Session expired');
    }
  }
  
  return response;
}
```

---

## Seguridad

- Las contraseñas se almacenan hasheadas con BCrypt
- Los tokens JWT están firmados con HMAC-SHA256
- Los refresh tokens se pueden revocar en cualquier momento
- Los access tokens tienen una vida útil corta (15 minutos) para mayor seguridad

---

## ?? Características de Seguridad Implementadas

### 1. **Cookies HttpOnly Seguras**
```
Set-Cookie: refreshToken=...; 
  HttpOnly;         // No accesible por JavaScript (protección XSS)
  Secure;           // Solo se envía por HTTPS
  SameSite=Strict;  // Protección contra CSRF
  Expires=...       // Expiración automática
```

### 2. **Access Tokens de Corta Duración**
- Los access tokens expiran en **15 minutos**
- Minimiza el impacto si un token es comprometido
- El frontend debe renovarlos periódicamente

### 3. **Refresh Token Rotation**
- Cada vez que se usa un refresh token, se genera uno nuevo
- El refresh token anterior se revoca inmediatamente
- Previene reutilización de tokens robados

### 4. **Validación JWT Estricta**
- Validación de firma (HMAC-SHA256)
- Validación de Issuer y Audience
- Validación de tiempo de expiración
- `ClockSkew = TimeSpan.Zero` (sin tolerancia de tiempo)

### 5. **Hash de Contraseñas con BCrypt**
- Algoritmo resistente a ataques de fuerza bruta
- Cost factor ajustable (por defecto: 11)
- Salting automático

---

## ?? Consideraciones de Producción

### 1. **HTTPS Obligatorio**
En producción, las cookies `Secure` requieren HTTPS:
```csharp
// En appsettings.Production.json o middleware
app.UseHttpsRedirection();
app.UseHsts(); // Fuerza HTTPS
```

### 2. **CORS Configurado Correctamente**
Ya está configurado en `ServiceCollectionExtensions.cs`:
```csharp
options.AddPolicy("AllowSpecificOrigin",
    policy => policy
        .WithOrigins("https://tu-dominio.com")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()); // ?? Necesario para cookies
```

### 3. **Rotación de Claves JWT**
Considera rotar la clave secreta periódicamente:
```json
// appsettings.json
{
  "AppSettings": {
    "Token": "clave-secreta-larga-y-compleja-minimo-32-caracteres"
  }
}
```

### 4. **Limpieza de Tokens Expirados**
Implementa un job en segundo plano para limpiar refresh tokens expirados:
```csharp
// Ejemplo con Hangfire o Quartz.NET
var expiredTokens = await context.RefreshTokens
    .Where(rt => rt.ExpiresAt < DateTime.UtcNow || rt.RevokedAt != null)
    .ToListAsync();

context.RefreshTokens.RemoveRange(expiredTokens);
await context.SaveChangesAsync();
```

### 5. **Rate Limiting en Login**
Considera agregar rate limiting para prevenir ataques de fuerza bruta:
```csharp
// Usando Microsoft.AspNetCore.RateLimiting (NET 7+)
builder.Services.AddRateLimiter(options => 
{
    options.AddFixedWindowLimiter("login", opt => 
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5; // 5 intentos por minuto
    });
});

// En el endpoint
[EnableRateLimiting("login")]
[HttpPost("login")]
public async Task<IActionResult> Login(...)
```

---

## ?? Troubleshooting

### Problema: "Cookies not being sent"
**Solución:**
- Frontend: Asegúrate de usar `credentials: 'include'` en fetch/axios
- Backend: Verifica CORS con `AllowCredentials()`
- Navegador: Habilita cookies de terceros si usas dominios diferentes

### Problema: "401 Unauthorized después de refresh"
**Solución:**
- Verifica que la cookie no haya expirado
- Asegúrate de que el navegador envía la cookie
- Revisa que el refresh token no haya sido revocado

### Problema: "SameSite warnings en consola"
**Solución:**
- En desarrollo: Usa HTTPS incluso en localhost
- O temporalmente cambia a `SameSite=Lax` (menos seguro)

---

## ?? Referencias

- [RFC 6749 - OAuth 2.0](https://tools.ietf.org/html/rfc6749)
- [RFC 7519 - JSON Web Token (JWT)](https://tools.ietf.org/html/rfc7519)
- [RFC 7807 - Problem Details for HTTP APIs](https://tools.ietf.org/html/rfc7807)
- [OWASP - Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
