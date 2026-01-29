# ?? Implementación de Autenticación con Cookies HttpOnly

## ? Resumen de Cambios

### Archivos Nuevos Creados:
1. **`LoginResponseDto.cs`** - Nuevo DTO que solo retorna access token (sin refresh token en JSON)

### Archivos Modificados:

#### 1. **`IAuthService.cs`**
- Los métodos ahora reciben `HttpContext` para acceder a cookies
- `LoginAsync` retorna `LoginResponseDto` en lugar de `TokenResponseDto`
- `RefreshTokenAsync` y `RevokeTokenAsync` ya no reciben DTOs (leen cookies directamente)

#### 2. **`AuthService.cs`**
- ? `LoginAsync`: Establece refresh token en cookie HttpOnly
- ? `RefreshTokenAsync`: Lee refresh token desde cookie, genera nuevo token y lo actualiza
- ? `RevokeTokenAsync`: Lee refresh token desde cookie y lo revoca
- ? Nuevos métodos helper:
  - `SetRefreshTokenCookie()` - Configura cookie segura
  - `DeleteRefreshTokenCookie()` - Elimina cookie al hacer logout

#### 3. **`AuthController.cs`**
- Los endpoints ahora pasan `HttpContext` al servicio
- `RefreshToken` y `RevokeToken` ya no requieren body
- Actualizado `ProducesResponseType` a `LoginResponseDto`

#### 4. **`AUTH_GUIDE.md`**
- Documentación completamente actualizada con:
  - Explicación del enfoque híbrido de seguridad
  - Ejemplos de uso en JavaScript/TypeScript
  - Configuración de Axios y Fetch
  - Sección completa de seguridad
  - Troubleshooting común
  - Consideraciones de producción

---

## ?? Flujo de Tokens Implementado

### Antes (JSON puro):
```
Login ? { accessToken, refreshToken } ? Frontend guarda ambos
                ?? Vulnerable a XSS si hay exploit en frontend
```

### Ahora (Híbrido con Cookies HttpOnly):
```
Login ? { accessToken } + Cookie(refreshToken) 
         ? Access token en memoria (no persiste)
         ? Refresh token en cookie HttpOnly (JS no puede leerlo)
```

---

## ??? Configuración de Cookies

```csharp
new CookieOptions
{
    HttpOnly = true,        // ? No accesible por JavaScript
    Secure = true,          // ? Solo HTTPS
    SameSite = Strict,      // ? Protección CSRF
    Expires = expiresAt     // ? Expiración automática
}
```

---

## ?? Comparación: Antes vs Ahora

| Aspecto | Antes | Ahora |
|---------|-------|-------|
| **Refresh Token en JSON** | ? Sí | ? No |
| **Refresh Token en Cookie** | ? No | ? Sí (HttpOnly) |
| **Vulnerable a XSS** | ?? Sí | ? Protegido |
| **Body en /refresh** | ? Requerido | ? No necesario |
| **Body en /revoke** | ? Requerido | ? No necesario |
| **credentials: 'include'** | ? No necesario | ? Requerido en frontend |

---

## ?? Cambios Necesarios en el Frontend

### 1. Agregar `credentials: 'include'` en todas las peticiones

**Fetch API:**
```javascript
fetch('/api/auth/login', {
  method: 'POST',
  credentials: 'include', // ?? NUEVO: Permite cookies
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email, password, rememberMe })
})
```

**Axios:**
```javascript
axios.defaults.withCredentials = true; // Configurar una vez
```

### 2. Actualizar endpoint de Refresh Token

**Antes:**
```javascript
await fetch('/api/auth/refresh', {
  method: 'POST',
  body: JSON.stringify({ userId, refreshToken }) // ? Ya no necesario
})
```

**Ahora:**
```javascript
await fetch('/api/auth/refresh', {
  method: 'POST',
  credentials: 'include' // ? Cookie enviada automáticamente
  // Sin body
})
```

### 3. Guardar Access Token en Memoria (NO en localStorage)

**Antes (inseguro):**
```javascript
localStorage.setItem('accessToken', token); // ?? Vulnerable a XSS
```

**Ahora (seguro):**
```javascript
// React Context
const [accessToken, setAccessToken] = useState(null);

// Vue Store
const accessToken = ref(null);

// Se pierde al refrescar la página ? Usar refresh token automáticamente
```

---

## ? Testing de la Implementación

### Usando cURL:

**1. Login:**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Password123!","rememberMe":true}' \
  -c cookies.txt \
  -v
```

**2. Refresh Token:**
```bash
curl -X POST http://localhost:5000/api/auth/refresh \
  -b cookies.txt \
  -v
```

**3. Revoke Token:**
```bash
curl -X POST http://localhost:5000/api/auth/revoke \
  -b cookies.txt \
  -v
```

### Usando Swagger:

?? **Nota**: Swagger UI tiene limitaciones con cookies HttpOnly. Para testing completo, usa:
- Postman (habilitar "Enable cookies" en settings)
- Insomnia
- Frontend real
- cURL

---

## ?? Beneficios de Seguridad

| Ataque | Antes | Ahora |
|--------|-------|-------|
| **XSS (Cross-Site Scripting)** | ?? Vulnerable | ? Protegido |
| **Token theft via malware** | ?? Alto riesgo | ? Riesgo reducido |
| **CSRF (Cross-Site Request Forgery)** | ? No aplica | ? Protegido (SameSite) |
| **Man-in-the-Middle** | ? Protegido (HTTPS) | ? Protegido (HTTPS + Secure) |

---

## ?? Notas Importantes

1. ? **La cookie se maneja automáticamente** - El navegador la envía en cada petición
2. ? **CORS debe permitir credentials** - Ya configurado en `ServiceCollectionExtensions.cs`
3. ?? **Requiere HTTPS en producción** - Las cookies `Secure` solo funcionan con HTTPS
4. ? **Refresh token rotation** - Cada refresh genera un nuevo token y revoca el anterior
5. ? **El access token aún se envía en headers** - `Authorization: Bearer <token>`

---

## ?? Migración desde Implementación Anterior

Si tenías frontend usando la API anterior:

1. ? Actualiza todas las peticiones para incluir `credentials: 'include'`
2. ? Elimina el refresh token del localStorage/sessionStorage
3. ? Actualiza los endpoints `/refresh` y `/revoke` para no enviar body
4. ? Guarda solo el access token en memoria (no en almacenamiento persistente)
5. ? Implementa renovación automática cuando el access token expira

---

## ?? ¡Listo para Producción!

La implementación ahora sigue las **mejores prácticas de seguridad** recomendadas por OWASP y la industria:

- ?? Tokens sensibles en cookies HttpOnly
- ? Tokens de acceso de corta duración
- ?? Rotación automática de refresh tokens
- ??? Múltiples capas de protección (HttpOnly, Secure, SameSite)
- ? Compatible con aplicaciones SPA modernas
