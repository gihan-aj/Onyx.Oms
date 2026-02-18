# Onyx Identity Provider - API Reference

This API allows trusted client applications (like the Order Management System) to manage roles and onboard users programmatically.

## Authentication

All API endpoints require an OAuth 2.0 Access Token obtained via the **Client Credentials Flow**.

### Get Access Token

**Endpoint:** `POST /connect/token`

**Headers:**
- `Content-Type: application/x-www-form-urlencoded`

**Form Body:**
| Parameter | Value | Description |
| :--- | :--- | :--- |
| `grant_type` | `client_credentials` | Required. |
| `client_id` | `{your_client_id}` | Your assigned Client ID (e.g., `order-system`). |
| `client_secret` | `{your_client_secret}` | Your assigned Client Secret. |
| `scope` | `idp_roles_manage` | Required scope for these APIs. |

**Response:**
```json
{
  "access_token": "eyJhbGciOi...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "scope": "idp_roles_manage"
}
```

---

## Role Management

### Create Role
Defines a new role within the Identity Provider.
> **Note:** The role name will be automatically prefixed with your Client ID (e.g., sending `RefundClerk` creates `OrderSystem_RefundClerk`) to avoid collisions with other systems.

**Endpoint:** `POST /api/roles`

**Request Body:**
```json
{
  "name": "RefundClerk"
}
```

**Response:** `200 OK`
```json
{
  "message": "Role 'RefundClerk' created successfully."
}
```

---

## User Management

### Get User
Checks if a user already exists in the system by email.

**Endpoint:** `GET /api/users`

**Query Parameters:**
- `email`: The email address to search for.

**Response:** `200 OK`
```json
{
  "id": "guid-user-id",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "isActive": true
}
```
*Returns `404 Not Found` if the user does not exist.*

### Invite User
Creates a new user account, sends a welcome email with a password setup link, and assigns an initial role.

**Endpoint:** `POST /api/users`

**Request Body:**
```json
{
  "email": "john@company.com",
  "roleName": "RefundClerk",
  "firstName": "John",
  "lastName": "Doe"
}
```
*Note: `roleName` should be the short name (e.g., "RefundClerk"). The system will automatically look for `OrderSystem_RefundClerk`.*

**Response:** `200 OK`
```json
{
  "id": "new-guid-user-id",
  "email": "john@company.com",
  "firstName": "John",
  "lastName": "Doe",
  "isActive": true
}
```

### Assign Role
Assigns an additional role to an existing user.

**Endpoint:** `POST /api/users/{userId}/roles`

**Path Parameters:**
- `userId`: The GUID of the user.

**Request Body:**
```json
{
  "roleName": "InventoryManager"
}
```

**Response:** `200 OK`
```json
{
  "message": "Role 'InventoryManager' assigned successfully."
}
```
