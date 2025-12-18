# Change Password API Documentation

## Table of Contents
- [Overview](#overview)
- [Endpoint Details](#endpoint-details)
- [Request Specification](#request-specification)
- [Response Specification](#response-specification)
- [Error Handling](#error-handling)
- [Business Rules](#business-rules)
- [Security Considerations](#security-considerations)
- [Implementation Examples](#implementation-examples)
- [UI/UX Best Practices](#uiux-best-practices)
- [Testing](#testing)

---

## Overview

The Change Password endpoint allows authenticated users to update their account password. This endpoint requires the user to provide their current password for verification before setting a new one.

**Key Features:**
- Requires authentication (JWT Bearer token)
- Validates current password before allowing change
- Enforces strong password requirements
- Ensures new password differs from current password
- Rate-limited to prevent abuse
- Supports both email/password and external login accounts

---

## Endpoint Details

### HTTP Method & URL
```
PUT /me/change-password
```

### Base URL
```
https://your-api-domain.com
```

### Full URL
```
https://your-api-domain.com/me/change-password
```

### Authentication
**Required:** Yes - JWT Bearer Token

### Rate Limiting
- **Type:** Per authenticated user
- **Limit:** Configurable server-side
- **Response on exceed:** 429 Too Many Requests

---

## Request Specification

### Headers

| Header | Value | Required | Description |
|--------|-------|----------|-------------|
| Content-Type | application/json | Yes | Request body format |
| Authorization | Bearer {token} | Yes | JWT access token from login |

### Request Body Schema

```json
{
  "currentPassword": "string",
  "newPassword": "string"
}
```

### Request Body Fields

| Field | Type | Required | Min Length | Max Length | Description |
|-------|------|----------|------------|------------|-------------|
| currentPassword | string | Yes | 8 | - | User's current password |
| newPassword | string | Yes | 8 | - | User's desired new password |

### Validation Rules

#### currentPassword
- **Required:** Yes
- **Format:** Must match password pattern
- **Business Rule:** Must be the user's actual current password
- **Error on failure:** 400 Bad Request with "PasswordMismatch" code

#### newPassword
- **Required:** Yes
- **Minimum length:** 8 characters
- **Must contain:**
  - At least 1 uppercase letter (A-Z)
  - At least 1 lowercase letter (a-z)
  - At least 1 digit (0-9)
  - At least 1 special character: `!@#$%^&*()[]{}\_+=~\`|:;"'<>,./?-`
- **Business Rule:** Must be different from currentPassword
- **Pattern:** `(?=(.*[0-9]))(?=.*[\!@#$%^&*()\\[\]{}\\-_+=~\`|:;"'<>,./?])(?=.*[a-z])(?=(.*[A-Z]))(?=(.*)).{8,}`

### Request Examples

#### Valid Request
```bash
curl -X PUT https://your-api-domain.com/me/change-password \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -d '{
    "currentPassword": "OldSecure@123",
    "newPassword": "NewSecure@456"
  }'
```

#### Postman Example
```json
PUT /me/change-password HTTP/1.1
Host: your-api-domain.com
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "currentPassword": "OldSecure@123",
  "newPassword": "NewSecure@456"
}
```

---

## Response Specification

### Success Response

**HTTP Status Code:** `204 No Content`

**Response Body:** Empty

**Description:** 
- Password has been successfully changed
- User remains logged in
- Current access and refresh tokens remain valid
- No need to re-authenticate

**Headers:**
```
HTTP/1.1 204 No Content
Date: Sun, 08 Dec 2025 10:30:00 GMT
Server: Kestrel
```

---

## Error Handling

### Error Response Format

All error responses follow a consistent format:

```json
{
  "code": "string",
  "description": "string"
}
```

Or for validation errors:

```json
{
  "type": "string",
  "title": "string",
  "status": number,
  "errors": {
    "fieldName": ["error message"]
  }
}
```

### Error Scenarios

#### 1. Validation Errors (400 Bad Request)

**Scenario:** Invalid password format

```json
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "currentPassword": [
      "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one digit, and one special character. It must match the pattern."
    ],
    "newPassword": [
      "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one digit, and one special character. It must match the pattern."
    ]
  }
}
```

**Possible Validation Errors:**
- `'currentPassword' must not be empty.`
- `'newPassword' must not be empty.`
- `Password must be at least 8 characters long...` (for either field)

---

#### 2. Same Password (400 Bad Request)

**Scenario:** New password is identical to current password

```json
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "": [
      "New password must be different from the current password."
    ]
  }
}
```

**When it occurs:** User submits the same password for both fields

**User Action:** Must choose a different password

---

#### 3. Incorrect Current Password (400 Bad Request)

**Scenario:** Current password doesn't match the user's actual password

```json
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "code": "PasswordMismatch",
  "description": "Incorrect password."
}
```

**When it occurs:** User enters wrong current password

**User Action:** Re-enter correct current password

**Security Note:** Does not reveal if account exists (user is already authenticated)

---

#### 4. No Password Set (400 Bad Request)

**Scenario:** User signed up via Google OAuth and hasn't set a password

```json
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "code": "User.NoPasswordSet",
  "description": "This account does not have a password. Please set a password before attempting to change it."
}
```

**When it occurs:** 
- User registered using Google sign-in
- User has never set a password for email/password login

**User Action:** Redirect to Set Password endpoint (`/me/set-password`)

**Mobile Implementation:**
```kotlin
// Show dialog or navigate to Set Password screen
when (errorCode) {
    "User.NoPasswordSet" -> navigateToSetPassword()
}
```

---

#### 5. Unauthorized (401)

**Scenario:** Missing, invalid, or expired JWT token

```json
HTTP/1.1 401 Unauthorized
Content-Type: application/json

{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

**When it occurs:**
- No Authorization header provided
- Invalid JWT token format
- Token has expired
- Token signature is invalid

**User Action:** Redirect to login screen

**Mobile Implementation:**
```swift
// Handle token refresh or logout
if response.statusCode == 401 {
    // Try refresh token first
    if canRefreshToken {
        await refreshAccessToken()
        retry()
    } else {
        logout()
        navigateToLogin()
    }
}
```

---

#### 6. Rate Limit Exceeded (429)

**Scenario:** Too many password change requests in short time

```json
HTTP/1.1 429 Too Many Requests
Content-Type: application/json

{
  "type": "https://tools.ietf.org/html/rfc6585#section-4",
  "title": "Too Many Requests",
  "status": 429,
  "detail": "Rate limit exceeded. Please try again later."
}
```

**When it occurs:** User (or attacker) makes too many requests

**User Action:** Wait before retrying

**Mobile Implementation:**
```dart
// Implement exponential backoff
var retryAfter = 60; // seconds
await Future.delayed(Duration(seconds: retryAfter));
retry();
```

**Response Headers:**
```
Retry-After: 60
X-RateLimit-Limit: 10
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1701172800
```

---

#### 7. Internal Server Error (500)

**Scenario:** Unexpected server error

```json
HTTP/1.1 500 Internal Server Error
Content-Type: application/json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500
}
```

**User Action:** Show generic error, retry after delay, or contact support

---

## Business Rules

### 1. Authentication Required
- User must be authenticated with valid JWT token
- Token is extracted from Authorization header
- User ID is derived from token claims

### 2. Password Verification
- Current password must match user's actual password
- Verification happens server-side only
- Failed verification returns generic "Incorrect password" message

### 3. Password Requirements
- **Minimum 8 characters**
- **At least 1 uppercase letter** (A-Z)
- **At least 1 lowercase letter** (a-z)
- **At least 1 digit** (0-9)
- **At least 1 special character** from: `!@#$%^&*()[]{}\_+=~\`|:;"'<>,./?-`
- **Must differ from current password**

### 4. External Login Accounts
- Users who signed up via Google may not have a password
- These users get "NoPasswordSet" error
- Must use `/me/set-password` endpoint first

### 5. Token Persistence
- Existing access tokens remain valid after password change
- Existing refresh tokens remain valid
- No forced logout or re-authentication required

### 6. Rate Limiting
- Applied per authenticated user
- Prevents brute-force password discovery
- Configurable limits (check server config)

### 7. Password History
- System may prevent reuse of recent passwords (check server config)
- New password must be different from current password
- Additional history rules may apply

### 8. Audit Logging
- All password changes are logged server-side
- Logs include: user ID, timestamp, IP address, success/failure
- Used for security monitoring and compliance

---

## Security Considerations

### 1. Password Transmission
- **Always use HTTPS** - Passwords transmitted over encrypted connection
- Never send passwords in URL parameters or headers (except Authorization)
- Use POST/PUT body for password fields

### 2. Token Security
- Store JWT tokens securely:
  - **Android:** EncryptedSharedPreferences or Keystore
  - **iOS:** Keychain
  - **Flutter:** flutter_secure_storage
  - **React Native:** react-native-keychain
- Never log tokens in debug output
- Clear tokens on logout

### 3. Password Validation
- Validate password strength client-side for better UX
- Server-side validation is always authoritative
- Use password strength meters (e.g., zxcvbn library)

### 4. Error Messages
- Generic error for wrong current password
- Don't reveal password hints or patterns
- Don't indicate if account exists (user is authenticated)

### 5. Password Storage (Server-Side)
- Passwords are never stored in plain text
- Uses secure hashing (BCrypt, PBKDF2, or Argon2)
- Salt is unique per user

### 6. Brute Force Protection
- Rate limiting per user
- Account lockout policies (check server config)
- CAPTCHA may be added for repeated failures

### 7. Session Management
- Current session remains active after password change
- Consider forcing logout on other devices (optional feature)
- Refresh tokens may be revoked (check server config)

### 8. HTTPS Certificate Pinning
- Implement in production apps
- Prevents man-in-the-middle attacks

```kotlin
// Android OkHttp example
val certificatePinner = CertificatePinner.Builder()
    .add("your-api-domain.com", "sha256/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")
    .build()

val client = OkHttpClient.Builder()
    .certificatePinner(certificatePinner)
    .build()
```

---

## Implementation Examples

### Android (Kotlin) with Retrofit

```kotlin
// API Service Interface
interface AccountApi {
    @PUT("me/change-password")
    suspend fun changePassword(
        @Header("Authorization") token: String,
        @Body request: ChangePasswordRequest
    ): Response<Unit>
}

// Request Model
data class ChangePasswordRequest(
    val currentPassword: String,
    val newPassword: String
)

// Error Response Models
data class ApiError(
    val code: String,
    val description: String
)

data class ValidationError(
    val type: String,
    val title: String,
    val status: Int,
    val errors: Map<String, List<String>>
)

// Repository
class PasswordRepository(
    private val api: AccountApi,
    private val tokenManager: TokenManager
) {
    suspend fun changePassword(
        currentPassword: String,
        newPassword: String
    ): Result<Unit> {
        return try {
            val token = tokenManager.getAccessToken()
            val request = ChangePasswordRequest(currentPassword, newPassword)
            
            val response = api.changePassword("Bearer $token", request)
            
            when {
                response.isSuccessful -> Result.success(Unit)
                
                response.code() == 400 -> {
                    val errorBody = response.errorBody()?.string()
                    val error = parseError(errorBody)
                    Result.failure(Exception(error))
                }
                
                response.code() == 401 -> {
                    Result.failure(UnauthorizedException("Session expired"))
                }
                
                response.code() == 429 -> {
                    Result.failure(RateLimitException("Too many attempts"))
                }
                
                else -> Result.failure(Exception("Unknown error"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    private fun parseError(errorBody: String?): String {
        return try {
            val gson = Gson()
            val apiError = gson.fromJson(errorBody, ApiError::class.java)
            
            when (apiError.code) {
                "PasswordMismatch" -> "Current password is incorrect"
                "User.NoPasswordSet" -> "Please set a password first"
                else -> apiError.description
            }
        } catch (e: Exception) {
            "An error occurred"
        }
    }
}

// ViewModel
class ChangePasswordViewModel(
    private val repository: PasswordRepository
) : ViewModel() {
    
    private val _uiState = MutableStateFlow<ChangePasswordUiState>(ChangePasswordUiState.Idle)
    val uiState: StateFlow<ChangePasswordUiState> = _uiState.asStateFlow()
    
    fun changePassword(currentPassword: String, newPassword: String) {
        viewModelScope.launch {
            _uiState.value = ChangePasswordUiState.Loading
            
            // Client-side validation
            val validationError = validatePassword(currentPassword, newPassword)
            if (validationError != null) {
                _uiState.value = ChangePasswordUiState.Error(validationError)
                return@launch
            }
            
            // API call
            repository.changePassword(currentPassword, newPassword)
                .onSuccess {
                    _uiState.value = ChangePasswordUiState.Success
                }
                .onFailure { error ->
                    _uiState.value = ChangePasswordUiState.Error(
                        error.message ?: "An error occurred"
                    )
                }
        }
    }
    
    private fun validatePassword(current: String, new: String): String? {
        val passwordPattern = Regex(
            "(?=(.*[0-9]))(?=.*[\\!@#\$%^&*()\\\\[\\]{}\\\\-_+=~`|:;\"'<>,./?])(?=.*[a-z])(?=(.*[A-Z]))(?=(.*)).{8,}"
        )
        
        return when {
            current.isEmpty() -> "Current password is required"
            new.isEmpty() -> "New password is required"
            current == new -> "New password must be different"
            !new.matches(passwordPattern) -> 
                "Password must be at least 8 characters with uppercase, lowercase, number, and special character"
            else -> null
        }
    }
}

sealed class ChangePasswordUiState {
    object Idle : ChangePasswordUiState()
    object Loading : ChangePasswordUiState()
    object Success : ChangePasswordUiState()
    data class Error(val message: String) : ChangePasswordUiState()
}

// Compose UI
@Composable
fun ChangePasswordScreen(
    viewModel: ChangePasswordViewModel = viewModel()
) {
    var currentPassword by remember { mutableStateOf("") }
    var newPassword by remember { mutableStateOf("") }
    var confirmPassword by remember { mutableStateOf("") }
    
    val uiState by viewModel.uiState.collectAsState()
    
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp)
    ) {
        // Current Password
        OutlinedTextField(
            value = currentPassword,
            onValueChange = { currentPassword = it },
            label = { Text("Current Password") },
            visualTransformation = PasswordVisualTransformation(),
            modifier = Modifier.fillMaxWidth()
        )
        
        Spacer(modifier = Modifier.height(16.dp))
        
        // New Password
        OutlinedTextField(
            value = newPassword,
            onValueChange = { newPassword = it },
            label = { Text("New Password") },
            visualTransformation = PasswordVisualTransformation(),
            modifier = Modifier.fillMaxWidth()
        )
        
        // Password strength indicator
        PasswordStrengthIndicator(password = newPassword)
        
        Spacer(modifier = Modifier.height(16.dp))
        
        // Confirm Password
        OutlinedTextField(
            value = confirmPassword,
            onValueChange = { confirmPassword = it },
            label = { Text("Confirm New Password") },
            visualTransformation = PasswordVisualTransformation(),
            modifier = Modifier.fillMaxWidth(),
            isError = confirmPassword.isNotEmpty() && confirmPassword != newPassword
        )
        
        Spacer(modifier = Modifier.height(24.dp))
        
        // Submit Button
        Button(
            onClick = { viewModel.changePassword(currentPassword, newPassword) },
            enabled = uiState !is ChangePasswordUiState.Loading &&
                     currentPassword.isNotEmpty() &&
                     newPassword.isNotEmpty() &&
                     newPassword == confirmPassword,
            modifier = Modifier.fillMaxWidth()
        ) {
            if (uiState is ChangePasswordUiState.Loading) {
                CircularProgressIndicator(
                    modifier = Modifier.size(24.dp),
                    color = MaterialTheme.colorScheme.onPrimary
                )
            } else {
                Text("Change Password")
            }
        }
        
        // Error/Success Messages
        when (val state = uiState) {
            is ChangePasswordUiState.Error -> {
                Text(
                    text = state.message,
                    color = MaterialTheme.colorScheme.error,
                    modifier = Modifier.padding(top = 16.dp)
                )
            }
            is ChangePasswordUiState.Success -> {
                Text(
                    text = "Password changed successfully!",
                    color = MaterialTheme.colorScheme.primary,
                    modifier = Modifier.padding(top = 16.dp)
                )
            }
            else -> {}
        }
    }
}
```

### iOS (Swift) with URLSession

```swift
import Foundation
import Combine

// Models
struct ChangePasswordRequest: Codable {
    let currentPassword: String
    let newPassword: String
}

struct ApiError: Codable {
    let code: String
    let description: String
}

enum ChangePasswordError: Error {
    case incorrectPassword
    case noPasswordSet
    case samePassword
    case weakPassword
    case unauthorized
    case rateLimited
    case networkError(String)
    
    var localizedDescription: String {
        switch self {
        case .incorrectPassword:
            return "Current password is incorrect"
        case .noPasswordSet:
            return "Please set a password first"
        case .samePassword:
            return "New password must be different"
        case .weakPassword:
            return "Password must be at least 8 characters with uppercase, lowercase, number, and special character"
        case .unauthorized:
            return "Session expired. Please login again"
        case .rateLimited:
            return "Too many attempts. Please try again later"
        case .networkError(let message):
            return message
        }
    }
}

// Service
class PasswordService {
    private let baseURL = "https://your-api-domain.com"
    private let tokenManager: TokenManager
    
    init(tokenManager: TokenManager) {
        self.tokenManager = tokenManager
    }
    
    func changePassword(
        current: String,
        new: String
    ) async throws {
        // Validate locally first
        guard validatePassword(new) else {
            throw ChangePasswordError.weakPassword
        }
        
        guard current != new else {
            throw ChangePasswordError.samePassword
        }
        
        // Prepare request
        let url = URL(string: "\(baseURL)/me/change-password")!
        var request = URLRequest(url: url)
        request.httpMethod = "PUT"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        
        if let token = tokenManager.getAccessToken() {
            request.setValue("Bearer \(token)", forHTTPHeaderField: "Authorization")
        }
        
        let body = ChangePasswordRequest(
            currentPassword: current,
            newPassword: new
        )
        request.httpBody = try JSONEncoder().encode(body)
        
        // Make request
        let (data, response) = try await URLSession.shared.data(for: request)
        
        guard let httpResponse = response as? HTTPURLResponse else {
            throw ChangePasswordError.networkError("Invalid response")
        }
        
        // Handle response
        switch httpResponse.statusCode {
        case 204:
            // Success
            return
            
        case 400:
            let error = try? JSONDecoder().decode(ApiError.self, from: data)
            switch error?.code {
            case "PasswordMismatch":
                throw ChangePasswordError.incorrectPassword
            case "User.NoPasswordSet":
                throw ChangePasswordError.noPasswordSet
            default:
                throw ChangePasswordError.networkError(
                    error?.description ?? "Invalid request"
                )
            }
            
        case 401:
            throw ChangePasswordError.unauthorized
            
        case 429:
            throw ChangePasswordError.rateLimited
            
        default:
            throw ChangePasswordError.networkError(
                "Error \(httpResponse.statusCode)"
            )
        }
    }
    
    private func validatePassword(_ password: String) -> Bool {
        let passwordPattern = "(?=(.*[0-9]))(?=.*[\\!@#$%^&*()\\\\[\\]{}\\\\-_+=~`|:;\"'<>,./?])(?=.*[a-z])(?=(.*[A-Z]))(?=(.*)).{8,}"
        let passwordPredicate = NSPredicate(format: "SELF MATCHES %@", passwordPattern)
        return passwordPredicate.evaluate(with: password)
    }
}

// ViewModel
@MainActor
class ChangePasswordViewModel: ObservableObject {
    @Published var currentPassword = ""
    @Published var newPassword = ""
    @Published var confirmPassword = ""
    @Published var isLoading = false
    @Published var errorMessage: String?
    @Published var successMessage: String?
    
    private let passwordService: PasswordService
    
    init(passwordService: PasswordService) {
        self.passwordService = passwordService
    }
    
    var isFormValid: Bool {
        !currentPassword.isEmpty &&
        !newPassword.isEmpty &&
        newPassword == confirmPassword &&
        currentPassword != newPassword
    }
    
    func changePassword() async {
        guard isFormValid else { return }
        
        isLoading = true
        errorMessage = nil
        successMessage = nil
        
        do {
            try await passwordService.changePassword(
                current: currentPassword,
                new: newPassword
            )
            
            successMessage = "Password changed successfully"
            
            // Clear fields
            currentPassword = ""
            newPassword = ""
            confirmPassword = ""
            
        } catch let error as ChangePasswordError {
            errorMessage = error.localizedDescription
        } catch {
            errorMessage = "An unexpected error occurred"
        }
        
        isLoading = false
    }
    
    func passwordStrength(_ password: String) -> PasswordStrength {
        var strength = 0
        
        if password.count >= 8 { strength += 1 }
        if password.range(of: "[A-Z]", options: .regularExpression) != nil { strength += 1 }
        if password.range(of: "[a-z]", options: .regularExpression) != nil { strength += 1 }
        if password.range(of: "[0-9]", options: .regularExpression) != nil { strength += 1 }
        if password.range(of: "[!@#$%^&*()\\[\\]{}\\-_+=~`|:;\"'<>,./?]", options: .regularExpression) != nil { strength += 1 }
        
        switch strength {
        case 0...2: return .weak
        case 3...4: return .medium
        default: return .strong
        }
    }
}

enum PasswordStrength {
    case weak, medium, strong
    
    var color: Color {
        switch self {
        case .weak: return .red
        case .medium: return .orange
        case .strong: return .green
        }
    }
}

// SwiftUI View
struct ChangePasswordView: View {
    @StateObject private var viewModel: ChangePasswordViewModel
    
    var body: some View {
        Form {
            Section(header: Text("Current Password")) {
                SecureField("Current Password", text: $viewModel.currentPassword)
            }
            
            Section(header: Text("New Password")) {
                SecureField("New Password", text: $viewModel.newPassword)
                
                // Password strength indicator
                HStack {
                    Text("Strength:")
                    Rectangle()
                        .fill(viewModel.passwordStrength(viewModel.newPassword).color)
                        .frame(height: 4)
                }
                
                SecureField("Confirm New Password", text: $viewModel.confirmPassword)
                
                if !viewModel.confirmPassword.isEmpty &&
                   viewModel.confirmPassword != viewModel.newPassword {
                    Text("Passwords do not match")
                        .foregroundColor(.red)
                        .font(.caption)
                }
            }
            
            Section {
                Button(action: {
                    Task {
                        await viewModel.changePassword()
                    }
                }) {
                    if viewModel.isLoading {
                        ProgressView()
                    } else {
                        Text("Change Password")
                    }
                }
                .disabled(!viewModel.isFormValid || viewModel.isLoading)
            }
            
            if let error = viewModel.errorMessage {
                Section {
                    Text(error)
                        .foregroundColor(.red)
                }
            }
            
            if let success = viewModel.successMessage {
                Section {
                    Text(success)
                        .foregroundColor(.green)
                }
            }
        }
        .navigationTitle("Change Password")
    }
}
```

### Flutter (Dart) with Dio

```dart
// Models
class ChangePasswordRequest {
  final String currentPassword;
  final String newPassword;
  
  ChangePasswordRequest({
    required this.currentPassword,
    required this.newPassword,
  });
  
  Map<String, dynamic> toJson() => {
    'currentPassword': currentPassword,
    'newPassword': newPassword,
  };
}

class ApiError {
  final String code;
  final String description;
  
  ApiError({required this.code, required this.description});
  
  factory ApiError.fromJson(Map<String, dynamic> json) => ApiError(
    code: json['code'] as String,
    description: json['description'] as String,
  );
}

// Custom Exceptions
class IncorrectPasswordException implements Exception {
  final String message = 'Current password is incorrect';
}

class NoPasswordSetException implements Exception {
  final String message = 'Please set a password first. This account uses Google login.';
}

class SamePasswordException implements Exception {
  final String message = 'New password must be different from current password';
}

class WeakPasswordException implements Exception {
  final String message = 'Password must be at least 8 characters with uppercase, lowercase, number, and special character';
}

class UnauthorizedException implements Exception {
  final String message = 'Session expired. Please login again';
}

class RateLimitException implements Exception {
  final String message = 'Too many attempts. Please try again later';
}

// Service
class PasswordService {
  final Dio _dio;
  final TokenManager _tokenManager;
  
  PasswordService(this._dio, this._tokenManager);
  
  Future<void> changePassword({
    required String currentPassword,
    required String newPassword,
  }) async {
    // Validate locally
    if (!_isPasswordValid(newPassword)) {
      throw WeakPasswordException();
    }
    
    if (currentPassword == newPassword) {
      throw SamePasswordException();
    }
    
    try {
      final token = await _tokenManager.getAccessToken();
      
      final response = await _dio.put(
        '/me/change-password',
        data: ChangePasswordRequest(
          currentPassword: currentPassword,
          newPassword: newPassword,
        ).toJson(),
        options: Options(
          headers: {
            'Authorization': 'Bearer $token',
          },
        ),
      );
      
      if (response.statusCode == 204) {
        return;
      }
    } on DioException catch (e) {
      if (e.response?.statusCode == 400) {
        final error = e.response?.data;
        
        if (error is Map && error.containsKey('code')) {
          final apiError = ApiError.fromJson(error);
          
          switch (apiError.code) {
            case 'PasswordMismatch':
              throw IncorrectPasswordException();
            case 'User.NoPasswordSet':
              throw NoPasswordSetException();
            default:
              throw Exception(apiError.description);
          }
        } else if (error is Map && error.containsKey('errors')) {
          // Validation errors
          final errors = error['errors'] as Map<String, dynamic>;
          final errorList = errors['']  as List?;
          
          if (errorList?.first?.toString().contains('must be different') == true) {
            throw SamePasswordException();
          }
        }
      } else if (e.response?.statusCode == 401) {
        throw UnauthorizedException();
      } else if (e.response?.statusCode == 429) {
        throw RateLimitException();
      }
      
      throw Exception('Failed to change password: ${e.message}');
    }
  }
  
  bool _isPasswordValid(String password) {
    final pattern = RegExp(
      r'(?=(.*[0-9]))(?=.*[\!@#$%^&*()\\[\]{}\\-_+=~`|:;"\'<>,./?])(?=.*[a-z])(?=(.*[A-Z]))(?=(.*)).{8,}'
    );
    return pattern.hasMatch(password);
  }
  
  PasswordStrength getPasswordStrength(String password) {
    var strength = 0;
    
    if (password.length >= 8) strength++;
    if (RegExp(r'[A-Z]').hasMatch(password)) strength++;
    if (RegExp(r'[a-z]').hasMatch(password)) strength++;
    if (RegExp(r'[0-9]').hasMatch(password)) strength++;
    if (RegExp(r'[!@#$%^&*()[\]{}\_+=~`|:;"\'<>,./?-]').hasMatch(password)) strength++;
    
    if (strength <= 2) return PasswordStrength.weak;
    if (strength <= 4) return PasswordStrength.medium;
    return PasswordStrength.strong;
  }
}

enum PasswordStrength { weak, medium, strong }

// BLoC
class ChangePasswordCubit extends Cubit<ChangePasswordState> {
  final PasswordService _passwordService;
  
  ChangePasswordCubit(this._passwordService) : super(ChangePasswordInitial());
  
  Future<void> changePassword({
    required String currentPassword,
    required String newPassword,
  }) async {
    emit(ChangePasswordLoading());
    
    try {
      await _passwordService.changePassword(
        currentPassword: currentPassword,
        newPassword: newPassword,
      );
      
      emit(ChangePasswordSuccess());
    } on IncorrectPasswordException catch (e) {
      emit(ChangePasswordError(e.message));
    } on NoPasswordSetException catch (e) {
      emit(ChangePasswordNoPasswordSet(e.message));
    } on SamePasswordException catch (e) {
      emit(ChangePasswordError(e.message));
    } on WeakPasswordException catch (e) {
      emit(ChangePasswordError(e.message));
    } on UnauthorizedException catch (e) {
      emit(ChangePasswordUnauthorized(e.message));
    } on RateLimitException catch (e) {
      emit(ChangePasswordRateLimited(e.message));
    } catch (e) {
      emit(ChangePasswordError('An unexpected error occurred'));
    }
  }
}

// States
abstract class ChangePasswordState {}
class ChangePasswordInitial extends ChangePasswordState {}
class ChangePasswordLoading extends ChangePasswordState {}
class ChangePasswordSuccess extends ChangePasswordState {}
class ChangePasswordError extends ChangePasswordState {
  final String message;
  ChangePasswordError(this.message);
}
class ChangePasswordNoPasswordSet extends ChangePasswordState {
  final String message;
  ChangePasswordNoPasswordSet(this.message);
}
class ChangePasswordUnauthorized extends ChangePasswordState {
  final String message;
  ChangePasswordUnauthorized(this.message);
}
class ChangePasswordRateLimited extends ChangePasswordState {
  final String message;
  ChangePasswordRateLimited(this.message);
}

// UI Widget
class ChangePasswordScreen extends StatefulWidget {
  @override
  _ChangePasswordScreenState createState() => _ChangePasswordScreenState();
}

class _ChangePasswordScreenState extends State<ChangePasswordScreen> {
  final _formKey = GlobalKey<FormState>();
  final _currentPasswordController = TextEditingController();
  final _newPasswordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();
  
  bool _obscureCurrent = true;
  bool _obscureNew = true;
  bool _obscureConfirm = true;
  
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Change Password')),
      body: BlocListener<ChangePasswordCubit, ChangePasswordState>(
        listener: (context, state) {
          if (state is ChangePasswordSuccess) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text('Password changed successfully'),
                backgroundColor: Colors.green,
              ),
            );
            Navigator.pop(context);
          } else if (state is ChangePasswordError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: Colors.red,
              ),
            );
          } else if (state is ChangePasswordNoPasswordSet) {
            showDialog(
              context: context,
              builder: (context) => AlertDialog(
                title: Text('Set Password Required'),
                content: Text(state.message),
                actions: [
                  TextButton(
                    onPressed: () => Navigator.pop(context),
                    child: Text('Cancel'),
                  ),
                  TextButton(
                    onPressed: () {
                      Navigator.pop(context);
                      Navigator.pushNamed(context, '/set-password');
                    },
                    child: Text('Set Password'),
                  ),
                ],
              ),
            );
          } else if (state is ChangePasswordUnauthorized) {
            Navigator.pushNamedAndRemoveUntil(
              context,
              '/login',
              (route) => false,
            );
          } else if (state is ChangePasswordRateLimited) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                duration: Duration(seconds: 5),
                backgroundColor: Colors.orange,
              ),
            );
          }
        },
        child: Form(
          key: _formKey,
          child: ListView(
            padding: EdgeInsets.all(16),
            children: [
              // Current Password
              TextFormField(
                controller: _currentPasswordController,
                decoration: InputDecoration(
                  labelText: 'Current Password',
                  border: OutlineInputBorder(),
                  suffixIcon: IconButton(
                    icon: Icon(_obscureCurrent ? Icons.visibility : Icons.visibility_off),
                    onPressed: () => setState(() => _obscureCurrent = !_obscureCurrent),
                  ),
                ),
                obscureText: _obscureCurrent,
                validator: (value) {
                  if (value == null || value.isEmpty) {
                    return 'Current password is required';
                  }
                  return null;
                },
              ),
              
              SizedBox(height: 16),
              
              // New Password
              TextFormField(
                controller: _newPasswordController,
                decoration: InputDecoration(
                  labelText: 'New Password',
                  border: OutlineInputBorder(),
                  suffixIcon: IconButton(
                    icon: Icon(_obscureNew ? Icons.visibility : Icons.visibility_off),
                    onPressed: () => setState(() => _obscureNew = !_obscureNew),
                  ),
                ),
                obscureText: _obscureNew,
                onChanged: (value) => setState(() {}),
                validator: (value) {
                  if (value == null || value.isEmpty) {
                    return 'New password is required';
                  }
                  final service = context.read<PasswordService>();
                  if (!service._isPasswordValid(value)) {
                    return 'Password must meet requirements';
                  }
                  if (value == _currentPasswordController.text) {
                    return 'New password must be different';
                  }
                  return null;
                },
              ),
              
              // Password Strength Indicator
              if (_newPasswordController.text.isNotEmpty)
                Padding(
                  padding: EdgeInsets.only(top: 8),
                  child: _buildPasswordStrengthIndicator(),
                ),
              
              SizedBox(height: 16),
              
              // Confirm Password
              TextFormField(
                controller: _confirmPasswordController,
                decoration: InputDecoration(
                  labelText: 'Confirm New Password',
                  border: OutlineInputBorder(),
                  suffixIcon: IconButton(
                    icon: Icon(_obscureConfirm ? Icons.visibility : Icons.visibility_off),
                    onPressed: () => setState(() => _obscureConfirm = !_obscureConfirm),
                  ),
                ),
                obscureText: _obscureConfirm,
                validator: (value) {
                  if (value != _newPasswordController.text) {
                    return 'Passwords do not match';
                  }
                  return null;
                },
              ),
              
              SizedBox(height: 24),
              
              // Submit Button
              BlocBuilder<ChangePasswordCubit, ChangePasswordState>(
                builder: (context, state) {
                  final isLoading = state is ChangePasswordLoading;
                  
                  return ElevatedButton(
                    onPressed: isLoading ? null : _handleSubmit,
                    child: isLoading
                        ? CircularProgressIndicator()
                        : Text('Change Password'),
                  );
                },
              ),
            ],
          ),
        ),
      ),
    );
  }
  
  Widget _buildPasswordStrengthIndicator() {
    final service = context.read<PasswordService>();
    final strength = service.getPasswordStrength(_newPasswordController.text);
    
    Color color;
    String text;
    double value;
    
    switch (strength) {
      case PasswordStrength.weak:
        color = Colors.red;
        text = 'Weak';
        value = 0.33;
        break;
      case PasswordStrength.medium:
        color = Colors.orange;
        text = 'Medium';
        value = 0.66;
        break;
      case PasswordStrength.strong:
        color = Colors.green;
        text = 'Strong';
        value = 1.0;
        break;
    }
    
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Text('Password Strength: '),
            Text(text, style: TextStyle(color: color, fontWeight: FontWeight.bold)),
          ],
        ),
        SizedBox(height: 4),
        LinearProgressIndicator(
          value: value,
          backgroundColor: Colors.grey[300],
          valueColor: AlwaysStoppedAnimation(color),
        ),
      ],
    );
  }
  
  void _handleSubmit() {
    if (_formKey.currentState!.validate()) {
      context.read<ChangePasswordCubit>().changePassword(
        currentPassword: _currentPasswordController.text,
        newPassword: _newPasswordController.text,
      );
    }
  }
  
  @override
  void dispose() {
    _currentPasswordController.dispose();
    _newPasswordController.dispose();
    _confirmPasswordController.dispose();
    super.dispose();
  }
}
```

---

## UI/UX Best Practices

### 1. Form Design

#### Required Fields
- **Current Password:** Masked text input with show/hide toggle
- **New Password:** Masked text input with show/hide toggle
- **Confirm New Password:** Masked text input (client-side validation)

#### Field Ordering
1. Current Password (top)
2. New Password
3. Confirm New Password (bottom)

#### Visual Elements
- Clear labels for each field
- Eye icon to show/hide passwords
- Password strength indicator (live update)
- Validation checklist showing requirements
- Submit button (disabled until valid)

### 2. Password Strength Indicator

Display real-time feedback as user types new password:

```
Weak     [████░░░░░░] Red
Medium   [███████░░░] Orange
Strong   [██████████] Green
```

**Requirements Checklist:**
- ✓ At least 8 characters
- ✓ Contains uppercase letter
- ✓ Contains lowercase letter
- ✓ Contains number
- ✓ Contains special character
- ✓ Different from current password

### 3. Error Messages

#### User-Friendly Messages
```
✗ "Current password is incorrect. Please try again."
✗ "Your new password must be different from your current password."
✗ "Password must be at least 8 characters with uppercase, lowercase, number, and special character."
✗ "Session expired. Please log in again."
✗ "Too many attempts. Please wait a moment and try again."
```

#### Error Display
- Show errors below respective fields
- Use red color for errors
- Clear error when user starts typing
- Don't clear all fields on error

### 4. Success Feedback

```
✓ "Password Changed Successfully"
   "Your password has been updated. You're still logged in."
```

- Show success message prominently
- Option to navigate back or clear form
- Consider auto-navigation after 2 seconds

### 5. Loading State

- Disable submit button during API call
- Show loading spinner on button
- Disable all input fields while loading
- Prevent multiple submissions

### 6. Accessibility

- Proper labels for screen readers
- Tab order: Current → New → Confirm → Submit
- ARIA labels for password visibility toggles
- Announce errors to screen readers
- Color contrast for error messages

### 7. Mobile Considerations

- Large touch targets (min 44x44 points)
- Keyboard type: default (not email)
- Auto-capitalize: none
- Auto-correct: off
- Secure text entry: on
- Clear button in text fields

### 8. Special Cases Handling

#### Google Sign-In Users
```
Alert: "Set Password Required"
Message: "This account uses Google sign-in and doesn't have a password yet. 
          Would you like to set one now?"
Actions: [Cancel] [Set Password →]
```

#### Rate Limited
```
Message: "Too many password change attempts. 
          Please wait 5 minutes and try again."
Timer: "Retry available in 4:32"
```

### 9. Security Indicators

- Show "Secure Connection" icon
- Display when password was last changed
- Option to "Log out other devices" after change

---

## Testing

### Test Scenarios

#### 1. Happy Path Tests

**Test 1.1: Valid Password Change**
```
Input:
  Current: "OldPassword@123"
  New: "NewPassword@456"

Expected:
  Status: 204 No Content
  User remains logged in
  Success message displayed
```

**Test 1.2: Strong Password**
```
Input:
  Current: "Test@1234"
  New: "SuperSecure@Pass2024!"

Expected:
  Status: 204 No Content
  Password strength shows "Strong"
```

#### 2. Validation Error Tests

**Test 2.1: Same Password**
```
Input:
  Current: "Password@123"
  New: "Password@123"

Expected:
  Status: 400
  Error: "New password must be different from the current password."
```

**Test 2.2: Missing Uppercase**
```
Input:
  New: "password@123"

Expected:
  Status: 400
  Error: "Password must be at least 8 characters long..."
```

**Test 2.3: Missing Lowercase**
```
Input:
  New: "PASSWORD@123"

Expected:
  Status: 400
  Error: Password pattern error
```

**Test 2.4: Missing Digit**
```
Input:
  New: "Password@Test"

Expected:
  Status: 400
  Error: Password pattern error
```

**Test 2.5: Missing Special Character**
```
Input:
  New: "Password123"

Expected:
  Status: 400
  Error: Password pattern error
```

**Test 2.6: Too Short**
```
Input:
  New: "Pass@1"

Expected:
  Status: 400
  Error: Password pattern error
```

**Test 2.7: Empty Fields**
```
Input:
  Current: ""
  New: ""

Expected:
  Status: 400
  Errors for both fields
```

#### 3. Business Logic Tests

**Test 3.1: Incorrect Current Password**
```
Input:
  Current: "WrongPassword@123"
  New: "NewPassword@456"

Expected:
  Status: 400
  Code: "PasswordMismatch"
  Description: "Incorrect password."
```

**Test 3.2: Google User (No Password)**
```
Setup: User signed up via Google
Input:
  Current: (any)
  New: "NewPassword@456"

Expected:
  Status: 400
  Code: "User.NoPasswordSet"
  Description: "This account does not have a password..."
  Action: Redirect to Set Password screen
```

#### 4. Authentication Tests

**Test 4.1: No Token**
```
Setup: Request without Authorization header

Expected:
  Status: 401 Unauthorized
```

**Test 4.2: Invalid Token**
```
Setup: Authorization: Bearer invalid-token

Expected:
  Status: 401 Unauthorized
```

**Test 4.3: Expired Token**
```
Setup: Token expired 1 hour ago

Expected:
  Status: 401 Unauthorized
  Action: Attempt token refresh, then redirect to login
```

#### 5. Rate Limiting Tests

**Test 5.1: Too Many Requests**
```
Setup: Make 10+ requests in quick succession

Expected:
  Status: 429 Too Many Requests
  Headers: Retry-After: 60
```

#### 6. Edge Cases

**Test 6.1: Special Characters in Password**
```
Input:
  New: "P@ssw0rd!#$%"

Expected:
  Status: 204 No Content
```

**Test 6.2: Maximum Length Password**
```
Input:
  New: (Very long password, 100+ characters)

Expected:
  Should succeed if meets requirements
```

**Test 6.3: Unicode Characters**
```
Input:
  New: "Pāsswörd@123"

Expected:
  Test server behavior with Unicode
```

**Test 6.4: Whitespace in Password**
```
Input:
  New: " Password@123 "

Expected:
  Test if trimmed or rejected
```

### Test Data

#### Valid Passwords
```
Test@1234
SecurePass@2024
MyP@ssw0rd!
Strong#Pass123
P@ssw0rd2024!
```

#### Invalid Passwords
```
weak          (too short, no uppercase, no special char, no digit)
PASSWORD123   (no lowercase, no special char)
password123   (no uppercase, no special char)
Password123   (no special char)
Pass@word     (no digit)
Pass@1        (too short)
```

#### Test Accounts

Create test users with different scenarios:
1. **Email/Password User:** Standard account with password
2. **Google User:** Account created via OAuth, no password set
3. **Recently Changed:** User who changed password in last 24 hours
4. **Rate Limited User:** Account that hit rate limit

### Automation Testing

#### Example Test (Jest/JavaScript)
```javascript
describe('Change Password API', () => {
  it('should change password with valid credentials', async () => {
    const response = await request(app)
      .put('/me/change-password')
      .set('Authorization', `Bearer ${validToken}`)
      .send({
        currentPassword: 'OldPass@123',
        newPassword: 'NewPass@456'
      });
    
    expect(response.status).toBe(204);
  });
  
  it('should reject same password', async () => {
    const response = await request(app)
      .put('/me/change-password')
      .set('Authorization', `Bearer ${validToken}`)
      .send({
        currentPassword: 'Password@123',
        newPassword: 'Password@123'
      });
    
    expect(response.status).toBe(400);
    expect(response.body.errors['']).toContain('must be different');
  });
  
  it('should reject incorrect current password', async () => {
    const response = await request(app)
      .put('/me/change-password')
      .set('Authorization', `Bearer ${validToken}`)
      .send({
        currentPassword: 'WrongPass@123',
        newPassword: 'NewPass@456'
      });
    
    expect(response.status).toBe(400);
    expect(response.body.code).toBe('PasswordMismatch');
  });
});
```

---

## Frequently Asked Questions

### Q1: Do I need to log in again after changing password?
**A:** No, your current session remains active. However, you may want to offer users the option to log out other devices for security.

### Q2: What happens to refresh tokens after password change?
**A:** Current implementation keeps refresh tokens valid. Check with backend team if tokens are revoked on password change.

### Q3: Can I change password without knowing the current one?
**A:** No, this endpoint requires the current password. Use the Forgot Password flow instead (`/Auth/forget-password`).

### Q4: What if the user signed up with Google?
**A:** Google users don't have a password initially. They'll get a `User.NoPasswordSet` error and should use the Set Password endpoint (`/me/set-password`) first.

### Q5: Is there a password history check?
**A:** Current implementation prevents reuse of the immediate previous password. Additional history checks may be implemented server-side.

### Q6: How long until I can retry after rate limit?
**A:** Check the `Retry-After` header in the 429 response (typically 60 seconds).

### Q7: Should I hash passwords client-side?
**A:** No, send passwords as plain text over HTTPS. Server handles secure hashing.

### Q8: Can users set any password they want?
**A:** No, passwords must meet minimum requirements: 8+ characters, uppercase, lowercase, digit, and special character.

### Q9: What if token expires during password change?
**A:** You'll receive 401 Unauthorized. Try refreshing the token first, or redirect to login.

### Q10: Should I show password requirements?
**A:** Yes, always display password requirements to users. Use a visual checklist that updates in real-time.

---

## Related Documentation

- [Authentication API](./API_DOCUMENTATION.md#login)
- [Set Password API](./SET_PASSWORD_API_DOCUMENTATION.md)
- [Forgot Password API](./API_DOCUMENTATION.md#forgot-password)
- [Refresh Token API](./API_DOCUMENTATION.md#refresh-token)

---

## Support & Contact

### API Support
- **Email:** api-support@hireme.com
- **Response Time:** Within 24 hours

### Documentation
- **Full API Docs:** https://api.hireme.com/docs
- **Changelog:** https://api.hireme.com/changelog

### System Status
- **Status Page:** https://status.hireme.com
- **Uptime:** 99.9% SLA

### Security Issues
- **Security Email:** security@hireme.com
- **Response:** Immediate priority

---

## Changelog

### Version 1.0.0 (December 8, 2025)
- Initial documentation release
- Covers change password endpoint
- Includes mobile implementation examples
- Complete error handling guide
