# Update Profile API Documentation

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

The Update Profile endpoint allows authenticated users to update their account profile information, specifically their first and last names. This endpoint requires authentication and only updates the profile data for the currently logged-in user.

**Key Features:**
- Requires authentication (JWT Bearer token)
- Updates first name and last name
- Real-time validation
- Cannot change email or username
- Rate-limited per authenticated user
- Returns no content on success

---

## Endpoint Details

### HTTP Method & URL
```
PUT /me
```

### Base URL
```
https://your-api-domain.com
```

### Full URL
```
https://your-api-domain.com/me
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
  "firstName": "string",
  "lastName": "string"
}
```

### Request Body Fields

| Field | Type | Required | Min Length | Max Length | Description |
|-------|------|----------|------------|------------|-------------|
| firstName | string | Yes | 3 | 50 | User's first name |
| lastName | string | Yes | 3 | 50 | User's last name |

### Validation Rules

#### firstName
- **Required:** Yes
- **Type:** String
- **Minimum length:** 3 characters
- **Maximum length:** 50 characters
- **Cannot be empty**
- **Error message:** "First name must be between 3 and 50 characters long."

#### lastName
- **Required:** Yes
- **Type:** String
- **Minimum length:** 3 characters
- **Maximum length:** 50 characters
- **Cannot be empty**
- **Error message:** "Last name must be between 3 and 50 characters long."

### Request Examples

#### Valid Request
```bash
curl -X PUT https://your-api-domain.com/me \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -d '{
    "firstName": "John",
    "lastName": "Doe"
  }'
```

#### Postman Example
```json
PUT /me HTTP/1.1
Host: your-api-domain.com
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "firstName": "Jane",
  "lastName": "Smith"
}
```

#### Example with Special Characters
```json
{
  "firstName": "María",
  "lastName": "O'Brien"
}
```

---

## Response Specification

### Success Response

**HTTP Status Code:** `204 No Content`

**Response Body:** Empty

**Description:** 
- Profile has been successfully updated
- No response body is returned
- User remains logged in
- Changes are immediately effective

**Headers:**
```
HTTP/1.1 204 No Content
Date: Sun, 08 Dec 2025 10:30:00 GMT
Server: Kestrel
```

### Get Updated Profile

To retrieve the updated profile, call the Get Profile endpoint:

```bash
GET /me
Authorization: Bearer {token}
```

**Response:**
```json
{
  "email": "john.doe@example.com",
  "userName": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "hasPassword": true,
  "imageProfileUrl": "https://your-api-domain.com/ImageProfile/profile.jpg"
}
```

---

## Error Handling

### Error Response Format

All error responses follow a consistent format:

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

**Scenario A: First name too short**

```json
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "FirstName": [
      "First name must be between 3 and 50 characters long."
    ]
  }
}
```

**Scenario B: Last name too long**

```json
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "LastName": [
      "Last name must be between 3 and 50 characters long."
    ]
  }
}
```

**Scenario C: Empty fields**

```json
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "FirstName": [
      "'First Name' must not be empty."
    ],
    "LastName": [
      "Last name is required."
    ]
  }
}
```

**Scenario D: Multiple validation errors**

```json
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "FirstName": [
      "First name must be between 3 and 50 characters long."
    ],
    "LastName": [
      "Last name must be between 3 and 50 characters long."
    ]
  }
}
```

**Possible Validation Errors:**
- `'First Name' must not be empty.`
- `First name must be between 3 and 50 characters long.`
- `Last name is required.`
- `Last name must be between 3 and 50 characters long.`

---

#### 2. Unauthorized (401)

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
```kotlin
if (response.statusCode == 401) {
    // Try refresh token
    if (canRefreshToken) {
        await refreshAccessToken()
        retry()
    } else {
        logout()
        navigateToLogin()
    }
}
```

---

#### 3. Rate Limit Exceeded (429)

**Scenario:** Too many update requests in short time

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

**Response Headers:**
```
Retry-After: 60
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1701172800
```

**User Action:** Wait before retrying

---

#### 4. Internal Server Error (500)

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

**User Action:** Show generic error, retry after delay

---

## Business Rules

### 1. Authentication Required
- User must be authenticated with valid JWT token
- User ID is extracted from token claims
- Users can only update their own profile

### 2. Updatable Fields
- **Can update:** firstName, lastName
- **Cannot update:** email, username, password, userId
- Email and username changes require separate endpoints

### 3. Name Requirements
- **First Name:**
  - Minimum 3 characters
  - Maximum 50 characters
  - Cannot be empty or whitespace only
  - Can contain letters, spaces, hyphens, apostrophes
  
- **Last Name:**
  - Minimum 3 characters
  - Maximum 50 characters
  - Cannot be empty or whitespace only
  - Can contain letters, spaces, hyphens, apostrophes

### 4. Character Support
- Supports Unicode characters (María, José, etc.)
- Supports hyphens (Mary-Jane)
- Supports apostrophes (O'Brien)
- Supports spaces (Van Der Berg)

### 5. Immediate Effect
- Changes take effect immediately
- No approval process required
- Reflected in all subsequent API calls

### 6. Audit Logging
- All profile updates are logged server-side
- Logs include: user ID, timestamp, old values, new values
- Used for compliance and security monitoring

### 7. Rate Limiting
- Applied per authenticated user
- Prevents excessive profile updates
- Configurable limits (check server config)

---

## Security Considerations

### 1. HTTPS Only
- All requests must use HTTPS
- Passwords/tokens transmitted over encrypted connection
- Implement certificate pinning in production

### 2. Token Security
- Store JWT tokens securely
- Never expose tokens in logs
- Clear tokens on logout

### 3. Input Sanitization
- Server-side validation is always performed
- Client-side validation for better UX
- Protection against XSS and injection attacks

### 4. Profile Visibility
- Users can only update their own profile
- User ID derived from authenticated token
- Cannot update other users' profiles

### 5. Data Privacy
- Names are considered personal data
- Comply with GDPR/privacy regulations
- Users can request data deletion separately

---

## Implementation Examples

### Android (Kotlin) with Retrofit

```kotlin
// API Service Interface
interface AccountApi {
    @PUT("me")
    suspend fun updateProfile(
        @Header("Authorization") token: String,
        @Body request: UpdateProfileRequest
    ): Response<Unit>
    
    @GET("me")
    suspend fun getProfile(
        @Header("Authorization") token: String
    ): Response<UserProfileResponse>
}

// Request Model
data class UpdateProfileRequest(
    val firstName: String,
    val lastName: String
)

// Response Model
data class UserProfileResponse(
    val email: String,
    val userName: String,
    val firstName: String,
    val lastName: String,
    val hasPassword: Boolean,
    val imageProfileUrl: String?
)

// Repository
class ProfileRepository(
    private val api: AccountApi,
    private val tokenManager: TokenManager
) {
    suspend fun updateProfile(
        firstName: String,
        lastName: String
    ): Result<Unit> {
        return try {
            // Validate locally first
            validateName(firstName, "First name")
            validateName(lastName, "Last name")
            
            val token = tokenManager.getAccessToken()
            val request = UpdateProfileRequest(firstName, lastName)
            
            val response = api.updateProfile("Bearer $token", request)
            
            when {
                response.isSuccessful -> {
                    // Optionally refresh cached profile
                    refreshProfile()
                    Result.success(Unit)
                }
                
                response.code() == 400 -> {
                    val errorBody = response.errorBody()?.string()
                    val error = parseValidationError(errorBody)
                    Result.failure(Exception(error))
                }
                
                response.code() == 401 -> {
                    Result.failure(UnauthorizedException("Session expired"))
                }
                
                response.code() == 429 -> {
                    Result.failure(RateLimitException("Too many requests"))
                }
                
                else -> Result.failure(Exception("Unknown error"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    private fun validateName(name: String, fieldName: String) {
        when {
            name.isBlank() -> throw ValidationException("$fieldName is required")
            name.length < 3 -> throw ValidationException("$fieldName must be at least 3 characters")
            name.length > 50 -> throw ValidationException("$fieldName must be at most 50 characters")
        }
    }
    
    private suspend fun refreshProfile() {
        try {
            val token = tokenManager.getAccessToken()
            val response = api.getProfile("Bearer $token")
            if (response.isSuccessful) {
                response.body()?.let { profile ->
                    // Update cached profile
                    saveProfileToCache(profile)
                }
            }
        } catch (e: Exception) {
            // Silently fail - not critical
        }
    }
    
    private fun parseValidationError(errorBody: String?): String {
        return try {
            val gson = Gson()
            val validationError = gson.fromJson(
                errorBody,
                ValidationErrorResponse::class.java
            )
            
            // Extract first error message
            validationError.errors.values.firstOrNull()?.firstOrNull()
                ?: "Validation error occurred"
        } catch (e: Exception) {
            "An error occurred"
        }
    }
}

data class ValidationErrorResponse(
    val type: String,
    val title: String,
    val status: Int,
    val errors: Map<String, List<String>>
)

// ViewModel
class UpdateProfileViewModel(
    private val repository: ProfileRepository
) : ViewModel() {
    
    private val _uiState = MutableStateFlow<UpdateProfileUiState>(UpdateProfileUiState.Idle)
    val uiState: StateFlow<UpdateProfileUiState> = _uiState.asStateFlow()
    
    private val _currentProfile = MutableStateFlow<UserProfileResponse?>(null)
    val currentProfile: StateFlow<UserProfileResponse?> = _currentProfile.asStateFlow()
    
    fun loadProfile() {
        viewModelScope.launch {
            // Load current profile to prefill form
            repository.getProfile()
                .onSuccess { profile ->
                    _currentProfile.value = profile
                }
        }
    }
    
    fun updateProfile(firstName: String, lastName: String) {
        viewModelScope.launch {
            _uiState.value = UpdateProfileUiState.Loading
            
            repository.updateProfile(firstName.trim(), lastName.trim())
                .onSuccess {
                    _uiState.value = UpdateProfileUiState.Success
                }
                .onFailure { error ->
                    _uiState.value = UpdateProfileUiState.Error(
                        error.message ?: "An error occurred"
                    )
                }
        }
    }
}

sealed class UpdateProfileUiState {
    object Idle : UpdateProfileUiState()
    object Loading : UpdateProfileUiState()
    object Success : UpdateProfileUiState()
    data class Error(val message: String) : UpdateProfileUiState()
}

// Compose UI
@Composable
fun UpdateProfileScreen(
    viewModel: UpdateProfileViewModel = viewModel()
) {
    val currentProfile by viewModel.currentProfile.collectAsState()
    val uiState by viewModel.uiState.collectAsState()
    
    var firstName by remember { mutableStateOf("") }
    var lastName by remember { mutableStateOf("") }
    
    // Load profile on launch
    LaunchedEffect(Unit) {
        viewModel.loadProfile()
    }
    
    // Update form when profile loads
    LaunchedEffect(currentProfile) {
        currentProfile?.let { profile ->
            firstName = profile.firstName
            lastName = profile.lastName
        }
    }
    
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp)
    ) {
        Text(
            text = "Update Profile",
            style = MaterialTheme.typography.headlineMedium,
            modifier = Modifier.padding(bottom = 24.dp)
        )
        
        // First Name
        OutlinedTextField(
            value = firstName,
            onValueChange = { firstName = it },
            label = { Text("First Name") },
            modifier = Modifier.fillMaxWidth(),
            enabled = uiState !is UpdateProfileUiState.Loading,
            isError = firstName.length < 3 && firstName.isNotEmpty(),
            supportingText = {
                Text("${firstName.length}/50 characters")
            }
        )
        
        Spacer(modifier = Modifier.height(16.dp))
        
        // Last Name
        OutlinedTextField(
            value = lastName,
            onValueChange = { lastName = it },
            label = { Text("Last Name") },
            modifier = Modifier.fillMaxWidth(),
            enabled = uiState !is UpdateProfileUiState.Loading,
            isError = lastName.length < 3 && lastName.isNotEmpty(),
            supportingText = {
                Text("${lastName.length}/50 characters")
            }
        )
        
        Spacer(modifier = Modifier.height(24.dp))
        
        // Submit Button
        Button(
            onClick = { viewModel.updateProfile(firstName, lastName) },
            enabled = uiState !is UpdateProfileUiState.Loading &&
                     firstName.length in 3..50 &&
                     lastName.length in 3..50,
            modifier = Modifier.fillMaxWidth()
        ) {
            if (uiState is UpdateProfileUiState.Loading) {
                CircularProgressIndicator(
                    modifier = Modifier.size(24.dp),
                    color = MaterialTheme.colorScheme.onPrimary
                )
            } else {
                Text("Update Profile")
            }
        }
        
        // Error/Success Messages
        when (val state = uiState) {
            is UpdateProfileUiState.Error -> {
                Text(
                    text = state.message,
                    color = MaterialTheme.colorScheme.error,
                    modifier = Modifier.padding(top = 16.dp)
                )
            }
            is UpdateProfileUiState.Success -> {
                Text(
                    text = "Profile updated successfully!",
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
struct UpdateProfileRequest: Codable {
    let firstName: String
    let lastName: String
}

struct UserProfileResponse: Codable {
    let email: String
    let userName: String
    let firstName: String
    let lastName: String
    let hasPassword: Bool
    let imageProfileUrl: String?
}

enum UpdateProfileError: Error {
    case validationError(String)
    case unauthorized
    case rateLimited
    case networkError(String)
    
    var localizedDescription: String {
        switch self {
        case .validationError(let message):
            return message
        case .unauthorized:
            return "Session expired. Please login again"
        case .rateLimited:
            return "Too many requests. Please try again later"
        case .networkError(let message):
            return message
        }
    }
}

// Service
class ProfileService {
    private let baseURL = "https://your-api-domain.com"
    private let tokenManager: TokenManager
    
    init(tokenManager: TokenManager) {
        self.tokenManager = tokenManager
    }
    
    func updateProfile(
        firstName: String,
        lastName: String
    ) async throws {
        // Validate locally
        try validateName(firstName, fieldName: "First name")
        try validateName(lastName, fieldName: "Last name")
        
        // Prepare request
        let url = URL(string: "\(baseURL)/me")!
        var request = URLRequest(url: url)
        request.httpMethod = "PUT"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        
        if let token = tokenManager.getAccessToken() {
            request.setValue("Bearer \(token)", forHTTPHeaderField: "Authorization")
        }
        
        let body = UpdateProfileRequest(
            firstName: firstName.trimmingCharacters(in: .whitespaces),
            lastName: lastName.trimmingCharacters(in: .whitespaces)
        )
        request.httpBody = try JSONEncoder().encode(body)
        
        // Make request
        let (data, response) = try await URLSession.shared.data(for: request)
        
        guard let httpResponse = response as? HTTPURLResponse else {
            throw UpdateProfileError.networkError("Invalid response")
        }
        
        // Handle response
        switch httpResponse.statusCode {
        case 204:
            // Success - profile updated
            return
            
        case 400:
            let errorMessage = parseValidationError(from: data)
            throw UpdateProfileError.validationError(errorMessage)
            
        case 401:
            throw UpdateProfileError.unauthorized
            
        case 429:
            throw UpdateProfileError.rateLimited
            
        default:
            throw UpdateProfileError.networkError("Error \(httpResponse.statusCode)")
        }
    }
    
    func getProfile() async throws -> UserProfileResponse {
        let url = URL(string: "\(baseURL)/me")!
        var request = URLRequest(url: url)
        request.httpMethod = "GET"
        
        if let token = tokenManager.getAccessToken() {
            request.setValue("Bearer \(token)", forHTTPHeaderField: "Authorization")
        }
        
        let (data, response) = try await URLSession.shared.data(for: request)
        
        guard let httpResponse = response as? HTTPURLResponse,
              httpResponse.statusCode == 200 else {
            throw UpdateProfileError.networkError("Failed to fetch profile")
        }
        
        return try JSONDecoder().decode(UserProfileResponse.self, from: data)
    }
    
    private func validateName(_ name: String, fieldName: String) throws {
        let trimmed = name.trimmingCharacters(in: .whitespaces)
        
        if trimmed.isEmpty {
            throw UpdateProfileError.validationError("\(fieldName) is required")
        }
        
        if trimmed.count < 3 {
            throw UpdateProfileError.validationError("\(fieldName) must be at least 3 characters")
        }
        
        if trimmed.count > 50 {
            throw UpdateProfileError.validationError("\(fieldName) must be at most 50 characters")
        }
    }
    
    private func parseValidationError(from data: Data) -> String {
        struct ValidationError: Codable {
            let errors: [String: [String]]
        }
        
        do {
            let error = try JSONDecoder().decode(ValidationError.self, from: data)
            return error.errors.values.first?.first ?? "Validation error"
        } catch {
            return "Validation error occurred"
        }
    }
}

// ViewModel
@MainActor
class UpdateProfileViewModel: ObservableObject {
    @Published var firstName = ""
    @Published var lastName = ""
    @Published var isLoading = false
    @Published var errorMessage: String?
    @Published var successMessage: String?
    
    private let profileService: ProfileService
    
    init(profileService: ProfileService) {
        self.profileService = profileService
    }
    
    var isFormValid: Bool {
        firstName.count >= 3 && firstName.count <= 50 &&
        lastName.count >= 3 && lastName.count <= 50
    }
    
    func loadProfile() async {
        do {
            let profile = try await profileService.getProfile()
            firstName = profile.firstName
            lastName = profile.lastName
        } catch {
            errorMessage = "Failed to load profile"
        }
    }
    
    func updateProfile() async {
        guard isFormValid else { return }
        
        isLoading = true
        errorMessage = nil
        successMessage = nil
        
        do {
            try await profileService.updateProfile(
                firstName: firstName,
                lastName: lastName
            )
            
            successMessage = "Profile updated successfully"
            
        } catch let error as UpdateProfileError {
            errorMessage = error.localizedDescription
        } catch {
            errorMessage = "An unexpected error occurred"
        }
        
        isLoading = false
    }
}

// SwiftUI View
struct UpdateProfileView: View {
    @StateObject private var viewModel: UpdateProfileViewModel
    
    var body: some View {
        Form {
            Section(header: Text("Profile Information")) {
                TextField("First Name", text: $viewModel.firstName)
                    .autocapitalization(.words)
                    .disabled(viewModel.isLoading)
                
                Text("\(viewModel.firstName.count)/50")
                    .font(.caption)
                    .foregroundColor(
                        viewModel.firstName.count < 3 ? .red : .secondary
                    )
                
                TextField("Last Name", text: $viewModel.lastName)
                    .autocapitalization(.words)
                    .disabled(viewModel.isLoading)
                
                Text("\(viewModel.lastName.count)/50")
                    .font(.caption)
                    .foregroundColor(
                        viewModel.lastName.count < 3 ? .red : .secondary
                    )
            }
            
            Section {
                Button(action: {
                    Task {
                        await viewModel.updateProfile()
                    }
                }) {
                    if viewModel.isLoading {
                        ProgressView()
                    } else {
                        Text("Update Profile")
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
        .navigationTitle("Update Profile")
        .task {
            await viewModel.loadProfile()
        }
    }
}
```

### Flutter (Dart) with Dio

```dart
// Models
class UpdateProfileRequest {
  final String firstName;
  final String lastName;
  
  UpdateProfileRequest({
    required this.firstName,
    required this.lastName,
  });
  
  Map<String, dynamic> toJson() => {
    'firstName': firstName,
    'lastName': lastName,
  };
}

class UserProfileResponse {
  final String email;
  final String userName;
  final String firstName;
  final String lastName;
  final bool hasPassword;
  final String? imageProfileUrl;
  
  UserProfileResponse({
    required this.email,
    required this.userName,
    required this.firstName,
    required this.lastName,
    required this.hasPassword,
    this.imageProfileUrl,
  });
  
  factory UserProfileResponse.fromJson(Map<String, dynamic> json) {
    return UserProfileResponse(
      email: json['email'],
      userName: json['userName'],
      firstName: json['firstName'],
      lastName: json['lastName'],
      hasPassword: json['hasPassword'],
      imageProfileUrl: json['imageProfileUrl'],
    );
  }
}

// Exceptions
class ValidationException implements Exception {
  final String message;
  ValidationException(this.message);
}

class UnauthorizedException implements Exception {
  final String message = 'Session expired. Please login again';
}

class RateLimitException implements Exception {
  final String message = 'Too many requests. Please try again later';
}

// Service
class ProfileService {
  final Dio _dio;
  final TokenManager _tokenManager;
  
  ProfileService(this._dio, this._tokenManager);
  
  Future<void> updateProfile({
    required String firstName,
    required String lastName,
  }) async {
    // Validate locally
    _validateName(firstName, 'First name');
    _validateName(lastName, 'Last name');
    
    try {
      final token = await _tokenManager.getAccessToken();
      
      await _dio.put(
        '/me',
        data: UpdateProfileRequest(
          firstName: firstName.trim(),
          lastName: lastName.trim(),
        ).toJson(),
        options: Options(
          headers: {'Authorization': 'Bearer $token'},
        ),
      );
      
      // Success - no content returned
    } on DioException catch (e) {
      if (e.response?.statusCode == 400) {
        final errorMessage = _parseValidationError(e.response?.data);
        throw ValidationException(errorMessage);
      } else if (e.response?.statusCode == 401) {
        throw UnauthorizedException();
      } else if (e.response?.statusCode == 429) {
        throw RateLimitException();
      }
      
      throw Exception('Failed to update profile: ${e.message}');
    }
  }
  
  Future<UserProfileResponse> getProfile() async {
    try {
      final token = await _tokenManager.getAccessToken();
      
      final response = await _dio.get(
        '/me',
        options: Options(
          headers: {'Authorization': 'Bearer $token'},
        ),
      );
      
      return UserProfileResponse.fromJson(response.data);
    } on DioException catch (e) {
      throw Exception('Failed to load profile: ${e.message}');
    }
  }
  
  void _validateName(String name, String fieldName) {
    final trimmed = name.trim();
    
    if (trimmed.isEmpty) {
      throw ValidationException('$fieldName is required');
    }
    
    if (trimmed.length < 3) {
      throw ValidationException('$fieldName must be at least 3 characters');
    }
    
    if (trimmed.length > 50) {
      throw ValidationException('$fieldName must be at most 50 characters');
    }
  }
  
  String _parseValidationError(dynamic errorData) {
    if (errorData is Map && errorData.containsKey('errors')) {
      final errors = errorData['errors'] as Map<String, dynamic>;
      final firstError = errors.values.first as List;
      return firstError.first.toString();
    }
    return 'Validation error occurred';
  }
}

// BLoC
class UpdateProfileCubit extends Cubit<UpdateProfileState> {
  final ProfileService _profileService;
  
  UpdateProfileCubit(this._profileService) : super(UpdateProfileInitial());
  
  Future<void> loadProfile() async {
    emit(UpdateProfileLoading());
    
    try {
      final profile = await _profileService.getProfile();
      emit(UpdateProfileLoaded(profile));
    } catch (e) {
      emit(UpdateProfileError('Failed to load profile'));
    }
  }
  
  Future<void> updateProfile({
    required String firstName,
    required String lastName,
  }) async {
    emit(UpdateProfileSaving());
    
    try {
      await _profileService.updateProfile(
        firstName: firstName,
        lastName: lastName,
      );
      
      emit(UpdateProfileSuccess());
      
      // Reload profile to get updated data
      await loadProfile();
    } on ValidationException catch (e) {
      emit(UpdateProfileError(e.message));
    } on UnauthorizedException catch (e) {
      emit(UpdateProfileUnauthorized(e.message));
    } on RateLimitException catch (e) {
      emit(UpdateProfileRateLimited(e.message));
    } catch (e) {
      emit(UpdateProfileError('An unexpected error occurred'));
    }
  }
}

// States
abstract class UpdateProfileState {}
class UpdateProfileInitial extends UpdateProfileState {}
class UpdateProfileLoading extends UpdateProfileState {}
class UpdateProfileLoaded extends UpdateProfileState {
  final UserProfileResponse profile;
  UpdateProfileLoaded(this.profile);
}
class UpdateProfileSaving extends UpdateProfileState {}
class UpdateProfileSuccess extends UpdateProfileState {}
class UpdateProfileError extends UpdateProfileState {
  final String message;
  UpdateProfileError(this.message);
}
class UpdateProfileUnauthorized extends UpdateProfileState {
  final String message;
  UpdateProfileUnauthorized(this.message);
}
class UpdateProfileRateLimited extends UpdateProfileState {
  final String message;
  UpdateProfileRateLimited(this.message);
}

// UI Widget
class UpdateProfileScreen extends StatefulWidget {
  @override
  _UpdateProfileScreenState createState() => _UpdateProfileScreenState();
}

class _UpdateProfileScreenState extends State<UpdateProfileScreen> {
  final _formKey = GlobalKey<FormState>();
  final _firstNameController = TextEditingController();
  final _lastNameController = TextEditingController();
  
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Update Profile')),
      body: BlocConsumer<UpdateProfileCubit, UpdateProfileState>(
        listener: (context, state) {
          if (state is UpdateProfileSuccess) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text('Profile updated successfully'),
                backgroundColor: Colors.green,
              ),
            );
          } else if (state is UpdateProfileError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: Colors.red,
              ),
            );
          } else if (state is UpdateProfileUnauthorized) {
            Navigator.pushNamedAndRemoveUntil(
              context,
              '/login',
              (route) => false,
            );
          } else if (state is UpdateProfileRateLimited) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                duration: Duration(seconds: 5),
                backgroundColor: Colors.orange,
              ),
            );
          } else if (state is UpdateProfileLoaded) {
            // Prefill form
            _firstNameController.text = state.profile.firstName;
            _lastNameController.text = state.profile.lastName;
          }
        },
        builder: (context, state) {
          if (state is UpdateProfileLoading) {
            return Center(child: CircularProgressIndicator());
          }
          
          return Form(
            key: _formKey,
            child: ListView(
              padding: EdgeInsets.all(16),
              children: [
                // First Name
                TextFormField(
                  controller: _firstNameController,
                  decoration: InputDecoration(
                    labelText: 'First Name',
                    border: OutlineInputBorder(),
                    counterText: '${_firstNameController.text.length}/50',
                  ),
                  textCapitalization: TextCapitalization.words,
                  enabled: state is! UpdateProfileSaving,
                  onChanged: (value) => setState(() {}),
                  validator: (value) {
                    if (value == null || value.trim().isEmpty) {
                      return 'First name is required';
                    }
                    if (value.trim().length < 3) {
                      return 'First name must be at least 3 characters';
                    }
                    if (value.trim().length > 50) {
                      return 'First name must be at most 50 characters';
                    }
                    return null;
                  },
                ),
                
                SizedBox(height: 16),
                
                // Last Name
                TextFormField(
                  controller: _lastNameController,
                  decoration: InputDecoration(
                    labelText: 'Last Name',
                    border: OutlineInputBorder(),
                    counterText: '${_lastNameController.text.length}/50',
                  ),
                  textCapitalization: TextCapitalization.words,
                  enabled: state is! UpdateProfileSaving,
                  onChanged: (value) => setState(() {}),
                  validator: (value) {
                    if (value == null || value.trim().isEmpty) {
                      return 'Last name is required';
                    }
                    if (value.trim().length < 3) {
                      return 'Last name must be at least 3 characters';
                    }
                    if (value.trim().length > 50) {
                      return 'Last name must be at most 50 characters';
                    }
                    return null;
                  },
                ),
                
                SizedBox(height: 24),
                
                // Submit Button
                ElevatedButton(
                  onPressed: state is UpdateProfileSaving
                      ? null
                      : _handleSubmit,
                  child: state is UpdateProfileSaving
                      ? CircularProgressIndicator()
                      : Text('Update Profile'),
                ),
              ],
            ),
          );
        },
      ),
    );
  }
  
  void _handleSubmit() {
    if (_formKey.currentState!.validate()) {
      context.read<UpdateProfileCubit>().updateProfile(
        firstName: _firstNameController.text,
        lastName: _lastNameController.text,
      );
    }
  }
  
  @override
  void initState() {
    super.initState();
    context.read<UpdateProfileCubit>().loadProfile();
  }
  
  @override
  void dispose() {
    _firstNameController.dispose();
    _lastNameController.dispose();
    super.dispose();
  }
}
```

---

## UI/UX Best Practices

### 1. Form Design

#### Required Fields
- **First Name:** Text input with character counter
- **Last Name:** Text input with character counter

#### Field Styling
- Use title case/capitalize words
- Show character count (X/50)
- Highlight field when focused
- Show error state with red border
- Disable fields during submission

#### Visual Feedback
- Real-time character counting
- Validation on blur or submit
- Clear error messages below fields
- Success message on update

### 2. Pre-filling Form

Always load current profile data to prefill the form:

```dart
// On screen load
await loadCurrentProfile();
firstNameField.text = currentProfile.firstName;
lastNameField.text = currentProfile.lastName;
```

### 3. Validation Feedback

#### Real-time Validation
```
First Name: [John___________] 4/50 ✓
Last Name:  [Do_____________] 2/50 ✗ (Too short)
```

#### Error Messages
```
✗ "First name must be between 3 and 50 characters long."
✗ "Last name is required."
✗ "Last name must be at least 3 characters."
```

### 4. Success Feedback

```
✓ "Profile Updated Successfully"
  "Your changes have been saved."
```

Options:
- Show toast/snackbar for 3 seconds
- Auto-dismiss and navigate back
- Display updated info in profile view

### 5. Loading State

- Disable input fields during save
- Show loading spinner on submit button
- Prevent multiple submissions
- Show "Saving..." text

### 6. Character Counter

```
First Name [John________] 4/50
           [Too short!___] (if < 3)
           [Perfect!_____] (if 3-50)
           [Too long!____] (if > 50)
```

### 7. Accessibility

- Proper labels for screen readers
- Tab order: First Name → Last Name → Submit
- Announce validation errors
- Announce success messages
- Sufficient color contrast

### 8. Mobile Considerations

- Auto-capitalize first letter of each word
- Auto-correct: on
- Keyboard type: default text
- Large touch targets (44x44 points)
- Tap outside to dismiss keyboard

---

## Testing

### Test Scenarios

#### 1. Happy Path Tests

**Test 1.1: Valid Profile Update**
```
Input:
  firstName: "John"
  lastName: "Doe"

Expected:
  Status: 204 No Content
  Profile updated successfully
```

**Test 1.2: Names with Special Characters**
```
Input:
  firstName: "María"
  lastName: "O'Brien"

Expected:
  Status: 204 No Content
```

**Test 1.3: Names with Hyphens**
```
Input:
  firstName: "Mary-Jane"
  lastName: "Parker"

Expected:
  Status: 204 No Content
```

**Test 1.4: Names with Spaces**
```
Input:
  firstName: "Jean"
  lastName: "Van Der Berg"

Expected:
  Status: 204 No Content
```

#### 2. Validation Error Tests

**Test 2.1: First Name Too Short**
```
Input:
  firstName: "Jo"
  lastName: "Doe"

Expected:
  Status: 400
  Error: "First name must be between 3 and 50 characters long."
```

**Test 2.2: Last Name Too Short**
```
Input:
  firstName: "John"
  lastName: "Do"

Expected:
  Status: 400
  Error: "Last name must be between 3 and 50 characters long."
```

**Test 2.3: First Name Too Long**
```
Input:
  firstName: "A" * 51
  lastName: "Doe"

Expected:
  Status: 400
  Error: "First name must be between 3 and 50 characters long."
```

**Test 2.4: Empty First Name**
```
Input:
  firstName: ""
  lastName: "Doe"

Expected:
  Status: 400
  Error: "'First Name' must not be empty."
```

**Test 2.5: Empty Last Name**
```
Input:
  firstName: "John"
  lastName: ""

Expected:
  Status: 400
  Error: "Last name is required."
```

**Test 2.6: Whitespace Only**
```
Input:
  firstName: "   "
  lastName: "   "

Expected:
  Status: 400
  Errors for both fields
```

**Test 2.7: Both Fields Invalid**
```
Input:
  firstName: "Jo"
  lastName: "Do"

Expected:
  Status: 400
  Errors for both fields
```

#### 3. Authentication Tests

**Test 3.1: No Token**
```
Setup: Request without Authorization header

Expected:
  Status: 401 Unauthorized
```

**Test 3.2: Invalid Token**
```
Setup: Authorization: Bearer invalid-token

Expected:
  Status: 401 Unauthorized
```

**Test 3.3: Expired Token**
```
Setup: Token expired

Expected:
  Status: 401 Unauthorized
  Action: Refresh token or redirect to login
```

#### 4. Rate Limiting Tests

**Test 4.1: Too Many Requests**
```
Setup: Make 100+ requests in quick succession

Expected:
  Status: 429 Too Many Requests
  Headers: Retry-After
```

#### 5. Edge Cases

**Test 5.1: Minimum Valid Length**
```
Input:
  firstName: "Joe"
  lastName: "Doe"

Expected:
  Status: 204 No Content
```

**Test 5.2: Maximum Valid Length**
```
Input:
  firstName: "A" * 50
  lastName: "B" * 50

Expected:
  Status: 204 No Content
```

**Test 5.3: Unicode Characters**
```
Input:
  firstName: "François"
  lastName: "Müller"

Expected:
  Status: 204 No Content
```

**Test 5.4: Numbers in Name**
```
Input:
  firstName: "John123"
  lastName: "Doe456"

Expected:
  Test server behavior (typically allowed)
```

**Test 5.5: Trimming Whitespace**
```
Input:
  firstName: "  John  "
  lastName: "  Doe  "

Expected:
  Status: 204 No Content
  Saved as: "John", "Doe" (trimmed)
```

### Test Data

#### Valid Names
```
firstName: "John", lastName: "Doe"
firstName: "María", lastName: "García"
firstName: "Jean", lastName: "O'Brien"
firstName: "Mary-Jane", lastName: "Parker"
firstName: "José", lastName: "Van Der Berg"
```

#### Invalid Names
```
firstName: "Jo", lastName: "Do" (too short)
firstName: "", lastName: "" (empty)
firstName: "A"*51, lastName: "B"*51 (too long)
firstName: "  ", lastName: "  " (whitespace)
```

### Automation Testing

```javascript
describe('Update Profile API', () => {
  it('should update profile with valid data', async () => {
    const response = await request(app)
      .put('/me')
      .set('Authorization', `Bearer ${validToken}`)
      .send({
        firstName: 'John',
        lastName: 'Doe'
      });
    
    expect(response.status).toBe(204);
  });
  
  it('should reject short first name', async () => {
    const response = await request(app)
      .put('/me')
      .set('Authorization', `Bearer ${validToken}`)
      .send({
        firstName: 'Jo',
        lastName: 'Doe'
      });
    
    expect(response.status).toBe(400);
    expect(response.body.errors.FirstName[0]).toContain('between 3 and 50');
  });
  
  it('should accept special characters', async () => {
    const response = await request(app)
      .put('/me')
      .set('Authorization', `Bearer ${validToken}`)
      .send({
        firstName: 'María',
        lastName: "O'Brien"
      });
    
    expect(response.status).toBe(204);
  });
});
```

---

## Frequently Asked Questions

### Q1: Can I update just one field?
**A:** No, both firstName and lastName are required in the request. Send the unchanged value if you only want to update one field.

### Q2: Can I change my email address?
**A:** No, this endpoint only updates first and last names. Email changes require a separate endpoint (if available).

### Q3: What characters are allowed in names?
**A:** Letters, spaces, hyphens, apostrophes, and Unicode characters (for international names) are typically allowed.

### Q4: Is there a limit on how often I can update my profile?
**A:** Yes, rate limiting is applied per user. Check the 429 response for retry timing.

### Q5: Do I need to log in again after updating?
**A:** No, your session remains active. Changes take effect immediately.

### Q6: What if I enter whitespace before/after names?
**A:** The server trims whitespace, so "  John  " becomes "John".

### Q7: Can I use emojis in my name?
**A:** Test with your specific API. Typically, Unicode characters are supported.

### Q8: How do I know if my update was successful?
**A:** You'll receive a 204 No Content response. Fetch the profile (`GET /me`) to confirm changes.

### Q9: What happens if validation fails?
**A:** You'll receive a 400 Bad Request with specific error messages for each invalid field.

### Q10: Can I update someone else's profile?
**A:** No, the user ID comes from your authentication token. You can only update your own profile.

---

## Related Documentation

- [Get Profile API](./GET_PROFILE_API_DOCUMENTATION.md)
- [Authentication API](./API_DOCUMENTATION.md#login)
- [Upload Profile Image API](./UPLOAD_PROFILE_IMAGE_API_DOCUMENTATION.md)
- [Change Password API](./CHANGE_PASSWORD_API_DOCUMENTATION.md)

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
- Covers update profile endpoint
- Includes mobile implementation examples
- Complete error handling guide
- Validation rules and test scenarios
