# Live Transcription Feature Documentation

## Overview

The ProDialer system now includes live transcription capabilities using Azure OpenAI Whisper for extremely fast real-time transcriptions during agent calls. This feature provides automatic speech-to-text conversion that can help with call monitoring, compliance, and agent assistance.

## Features

- **Real-time Transcription**: Live transcription during active calls
- **Multiple Languages**: Automatic language detection or manual language specification
- **High Performance**: Uses Azure OpenAI Whisper "whisper-1" model for fast processing
- **Call Integration**: Seamlessly integrates with existing Azure Communication Services
- **Flexible Storage**: Transcriptions stored in CallLog with confidence scores

## API Endpoints

### Start Live Transcription
```http
POST /api/transcription/start
Content-Type: application/json

{
    "callId": "unique-call-id",
    "language": "en",
    "enableRealTime": true
}
```

**Response:**
```json
{
    "sessionId": "transcription-session-id",
    "callId": "unique-call-id",
    "isActive": true,
    "startedAt": "2024-01-01T12:00:00Z",
    "language": "en",
    "status": "Active"
}
```

### Stop Live Transcription
```http
POST /api/transcription/{sessionId}/stop
```

**Response:**
```json
{
    "sessionId": "transcription-session-id",
    "success": true,
    "text": "Full transcription text of the call...",
    "language": "en",
    "duration": 120.5,
    "confidence": 0.92,
    "processedAt": "2024-01-01T12:02:00Z"
}
```

### Transcribe Audio File
```http
POST /api/transcription/audio?language=en
Content-Type: audio/wav

[Binary audio data]
```

### Process Audio Chunk (Real-time)
```http
POST /api/transcription/{sessionId}/chunk
Content-Type: audio/wav

[Binary audio chunk data]
```

### Get Settings
```http
GET /api/transcription/settings
```

## Database Schema

The following fields have been added to the `CallLogs` table:

- `TranscriptionEnabled` (bit): Whether transcription was enabled for the call
- `TranscriptionText` (nvarchar(max)): Full transcription text
- `TranscriptionConfidence` (float): Confidence score (0-1)
- `TranscriptionLanguage` (nvarchar(10)): Detected/specified language
- `TranscriptionStatus` (nvarchar(20)): Processing status

## Configuration

Add the following configuration to your `local.settings.json` or Azure App Settings:

```json
{
    "TranscriptionService:AzureOpenAIEndpoint": "https://your-openai-resource.openai.azure.com/",
    "TranscriptionService:AzureOpenAIApiKey": "your-azure-openai-api-key",
    "TranscriptionService:WhisperModel": "whisper-1",
    "TranscriptionService:EnableByDefault": "true",
    "TranscriptionService:MaxAudioChunkSize": "1048576",
    "TranscriptionService:MinChunkDuration": "2.0",
    "TranscriptionService:MaxTranscriptionLength": "10000"
}
```

### Required Configuration

- **AzureOpenAIEndpoint**: Your Azure OpenAI service endpoint URL
- **WhisperModel**: Model to use (default: "whisper-1" for fastest processing)

### Optional Configuration

- **AzureOpenAIApiKey**: API key for authentication (optional if using managed identity)
- **EnableByDefault**: Enable transcription for all calls by default
- **MaxAudioChunkSize**: Maximum size of audio chunks in bytes
- **MinChunkDuration**: Minimum duration before processing chunks
- **MaxTranscriptionLength**: Maximum characters to store in database

## Usage Examples

### Starting Transcription in Communication Service

```csharp
var communicationService = serviceProvider.GetService<CommunicationService>();
var sessionId = await communicationService.StartTranscriptionAsync(callId, "en");

if (sessionId != null)
{
    // Transcription started successfully
    Console.WriteLine($"Transcription session {sessionId} started for call {callId}");
}
```

### Stopping Transcription

```csharp
var result = await communicationService.StopTranscriptionAsync(sessionId);

if (result?.Success == true)
{
    // Save transcription to database
    var callLog = await dbContext.CallLogs
        .FirstOrDefaultAsync(cl => cl.CallId == callId);
    
    if (callLog != null)
    {
        callLog.TranscriptionEnabled = true;
        callLog.TranscriptionText = result.Text;
        callLog.TranscriptionConfidence = result.Confidence;
        callLog.TranscriptionLanguage = result.Language;
        callLog.TranscriptionStatus = "Completed";
        
        await dbContext.SaveChangesAsync();
    }
}
```

## Integration with Call Flow

1. **Call Initiated**: When a call starts, optionally start transcription
2. **Real-time Processing**: Audio chunks are processed as they arrive
3. **Call Ended**: Transcription is stopped and final result saved to CallLog
4. **Agent Interface**: Transcription can be displayed to agents in real-time

## Error Handling

The transcription service includes comprehensive error handling:

- **API Configuration Missing**: Service degrades gracefully with warning logs
- **Network Issues**: Retry logic for transient failures
- **Audio Format Issues**: Clear error messages for unsupported formats
- **Rate Limiting**: Handles Azure OpenAI rate limits appropriately

## Performance Considerations

- **Latency**: Whisper-1 model provides sub-second transcription for short audio segments
- **Bandwidth**: Audio chunks are processed in small segments to minimize memory usage
- **Storage**: Large transcriptions are truncated to prevent database bloat
- **Costs**: Monitor Azure OpenAI usage for cost management

## Future Enhancements

- **Real-time Streaming**: WebSocket support for live transcription updates
- **Custom Models**: Support for fine-tuned Whisper models
- **Speaker Identification**: Multi-speaker transcription with speaker labels
- **Sentiment Analysis**: Integration with sentiment analysis for call quality
- **Search Integration**: Full-text search across transcriptions

## Troubleshooting

### Common Issues

1. **"Transcription service not available"**
   - Check Azure OpenAI endpoint and API key configuration
   - Verify network connectivity to Azure OpenAI service
   - Ensure proper managed identity permissions if not using API key

2. **"Audio format not supported"**
   - Ensure audio is in WAV, MP3, or other supported format
   - Check audio quality and sample rate

3. **Low confidence scores**
   - Check audio quality and background noise
   - Verify correct language setting
   - Consider audio preprocessing

### Debugging

Enable detailed logging to troubleshoot issues:

```json
{
    "Logging": {
        "LogLevel": {
            "ProDialer.Functions.Services.TranscriptionService": "Debug"
        }
    }
}
```