# API Documentation

Welcome to our comprehensive API documentation. This page demonstrates how technical documentation can be automatically generated from markdown files.

## Overview

Our API provides RESTful endpoints for managing markdown content and generating Razor pages dynamically.

## Authentication

All API endpoints require authentication using JWT tokens:

```csharp
// Add authentication header
client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", "your-jwt-token");
```

## Endpoints

### Markdown Processing

#### POST /api/markdown/convert

Converts markdown content to HTML using our MarkdownSection component.

**Request Body:**

```json
{
  "content": "# Hello World\nThis is **bold** text.",
  "enableSyntaxHighlighting": true,
  "includeLineNumbers": false
}
```

**Response:**

```json
{
  "html": "<h1>Hello World</h1><p>This is <strong>bold</strong> text.</p>",
  "success": true,
  "processingTimeMs": 15
}
```

#### GET /api/markdown/files

Lists all available markdown files in the system.

**Query Parameters:**

- `page` (int): Page number for pagination
- `pageSize` (int): Number of items per page
- `filter` (string): Optional filter by filename

**Response:**

```json
{
  "files": [
    {
      "name": "about.md",
      "route": "/about",
      "lastModified": "2025-08-22T10:30:00Z",
      "size": 1024
    }
  ],
  "totalCount": 15,
  "currentPage": 1
}
```

### Page Generation

#### POST /api/pages/generate

Triggers code generation for all markdown files.

```bash
curl -X POST https://localhost:7064/api/pages/generate \
  -H "Authorization: Bearer your-token" \
  -H "Content-Type: application/json"
```

**Response:**

```json
{
  "generatedPages": [
    {
      "sourceFile": "about.md",
      "generatedFile": "Pages/Generated/About.razor",
      "route": "/about",
      "success": true
    }
  ],
  "totalProcessed": 3,
  "errors": []
}
```

## SDK Examples

### C# SDK

```csharp
using MDFIleTORazor.Client;

var client = new MarkdownApiClient("https://localhost:7064");
client.SetAuthToken("your-jwt-token");

// Convert markdown to HTML
var result = await client.ConvertMarkdownAsync(new ConvertRequest
{
    Content = "# Hello API",
    EnableSyntaxHighlighting = true
});

Console.WriteLine(result.Html);
```

### JavaScript SDK

```javascript
import { MarkdownClient } from "@mdfiletorazor/js-sdk";

const client = new MarkdownClient({
  baseUrl: "https://localhost:7064",
  token: "your-jwt-token",
});

// Generate pages from markdown
const result = await client.generatePages();
console.log(`Generated ${result.totalProcessed} pages`);
```

### PowerShell

```powershell
# Convert markdown file to Razor page
$headers = @{
    "Authorization" = "Bearer your-jwt-token"
    "Content-Type" = "application/json"
}

$body = @{
    content = Get-Content "my-file.md" -Raw
    enableSyntaxHighlighting = $true
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7064/api/markdown/convert" `
                  -Method POST `
                  -Headers $headers `
                  -Body $body
```

## Error Handling

All endpoints return standard HTTP status codes:

| Status Code | Description                             |
| ----------- | --------------------------------------- |
| 200         | Success                                 |
| 400         | Bad Request - Invalid input             |
| 401         | Unauthorized - Missing or invalid token |
| 404         | Not Found - Resource doesn't exist      |
| 500         | Internal Server Error                   |

**Error Response Format:**

```json
{
  "error": {
    "code": "INVALID_MARKDOWN",
    "message": "The provided markdown content is invalid",
    "details": "Line 5: Unclosed code block",
    "timestamp": "2025-08-22T10:30:00Z"
  }
}
```

## Rate Limiting

API endpoints are rate limited:

- **Free Tier**: 100 requests/hour
- **Pro Tier**: 1000 requests/hour
- **Enterprise**: Unlimited

Rate limit headers are included in responses:

```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1629648000
```

## Webhooks

Configure webhooks to receive notifications when pages are generated:

```json
{
  "url": "https://your-app.com/webhook",
  "events": ["page.generated", "page.deleted"],
  "secret": "your-webhook-secret"
}
```

**Webhook Payload:**

```json
{
  "event": "page.generated",
  "data": {
    "sourceFile": "about.md",
    "generatedFile": "About.razor",
    "route": "/about"
  },
  "timestamp": "2025-08-22T10:30:00Z"
}
```

---

_This API documentation was generated from `api-docs.md` using our build-time code generation system._
