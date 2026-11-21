# ClassroomQuiz shared data contract

This document is the shared reference for the Unity client, the .NET TCP server, and the PHP/MySQL API. Keep JSON property names in lower camel case. C# field/property casing may differ because Unity `JsonUtility` writes fields as declared while the server's `JsonSerializerOptions` applies `JsonNamingPolicy.CamelCase`.

## TCP framing and envelope

- TCP port: `7777` by default.
- Each message is UTF-8 JSON preceded by a four-byte little-endian signed payload length.
- `protocolVersion` is `1`.
- `type` identifies the message; `requestId` correlates a request and response when applicable.
- Optional envelope values are `roomCode`, `roomName`, `playerName`, `authToken`, `accountId`, `role`, `mode`, `lessonTitle`, `lessonId`, `questionIndex`, `answerIndex`, `durationSeconds`, `success`, `error`, `message`, `payload`, `data`, `serverAddress`, and `serverPort`.
- Omitted numeric values are treated as absent; Unity's sentinel values (`-1` for indexes and `0` for unset IDs/durations) must not be interpreted as valid answers or identifiers.

## Lesson payload

`payload` for room creation contains a JSON object with `schemaVersion`, `lessonTitle`, `summary`, and `questions`. A question uses `lessonId`, `lessonTitle`, `questionId`, `sequenceIndex`, `prompt`, `options` (exactly four strings), `correctIndex` (0–3), and `explanation`. The same property names are used by `LessonQuestionData`, the TCP server's `QuestionDefinition`, and PHP's lesson question endpoint. SQL column names remain snake case and are mapped to JSON names at the API boundary.

## PHP API envelope

Requests are JSON POST objects. Account operations use `action`, `email`, `password`, and, for registration, `displayName`. Session validation uses `action: "check_session"` and `token`; the TCP server's existing `verifyToken` action remains a compatibility alias. Responses use `success`, `error`, `accountId`, `email`, `displayName`, `avatarKey`, and `token` as applicable. Room and lesson records also use lower camel case at the HTTP boundary. Session tokens are bearer credentials and must not be logged.

## Compatibility rule

Changes to a wire field or its meaning require updating the Unity DTO, the .NET DTO/serializer, the PHP endpoint, and this contract together. Keep transport-specific storage naming out of the JSON contract.
