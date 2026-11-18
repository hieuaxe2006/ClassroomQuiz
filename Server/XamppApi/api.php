<?php
declare(strict_types=1);

header('Content-Type: application/json; charset=utf-8');

function respond(array $data, int $status = 200): never
{
    http_response_code($status);
    echo json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

function readJson(): array
{
    $json = file_get_contents('php://input');
    $data = json_decode($json === false ? '' : $json, true);
    return is_array($data) ? $data : [];
}

function database(): PDO
{
    $host = getenv('CLASSROOMQUIZ_DB_HOST') ?: '127.0.0.1';
    $user = getenv('CLASSROOMQUIZ_DB_USER') ?: 'root';
    $password = getenv('CLASSROOMQUIZ_DB_PASSWORD') ?: '';
    $name = 'classromquiz';

    $bootstrap = new PDO("mysql:host={$host};charset=utf8mb4", $user, $password, [
        PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
        PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
        PDO::ATTR_EMULATE_PREPARES => false,
    ]);
    $bootstrap->exec("CREATE DATABASE IF NOT EXISTS `{$name}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci");
    $pdo = new PDO("mysql:host={$host};dbname={$name};charset=utf8mb4", $user, $password, [
        PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
        PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
        PDO::ATTR_EMULATE_PREPARES => false,
    ]);

    $schema = file_get_contents(__DIR__ . '/schema.sql');
    if ($schema === false) {
        throw new RuntimeException('schema.sql is missing.');
    }
    foreach (explode(';', $schema) as $statement) {
        $statement = trim($statement);
        if ($statement === '' || stripos($statement, 'CREATE DATABASE') === 0 || stripos($statement, 'USE ') === 0) {
            continue;
        }
        $pdo->exec($statement);
    }

    $avatarColumn = $pdo->query("SHOW COLUMNS FROM accounts LIKE 'avatar_key'")->fetch();
    if ($avatarColumn === false) {
        $pdo->exec("ALTER TABLE accounts ADD avatar_key CHAR(1) NOT NULL DEFAULT 'A' AFTER display_name");
    }

    seedLessonQuestions($pdo);

    return $pdo;
}

function seedLessonQuestions(PDO $pdo): void
{
    if ((int)$pdo->query('SELECT COUNT(*) FROM lesson_questions')->fetchColumn() > 0) {
        return;
    }
    $seedJson = file_get_contents(__DIR__ . '/lesson_questions_seed.json');
    $lessons = json_decode($seedJson === false ? '' : $seedJson, true);
    if (!is_array($lessons)) {
        throw new RuntimeException('Lesson question seed is missing or invalid.');
    }
    $insert = $pdo->prepare('INSERT INTO lesson_questions (lesson_id, lesson_title, lesson_summary, sequence_index, prompt, options_json, correct_index, explanation, difficulty) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)');
    foreach ($lessons as $lesson) {
        if (!is_array($lesson) || !is_array($lesson['questions'] ?? null)) {
            continue;
        }
        foreach ($lesson['questions'] as $index => $question) {
            if (!is_array($question) || count($question['options'] ?? []) !== 4) {
                continue;
            }
            $insert->execute([
                (int)($lesson['lessonId'] ?? 0),
                substr((string)($lesson['title'] ?? ''), 0, 160),
                substr((string)($lesson['summary'] ?? ''), 0, 500),
                $index + 1,
                (string)($question['prompt'] ?? ''),
                json_encode($question['options'], JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES),
                (int)($question['correctIndex'] ?? 0),
                (string)($question['explanation'] ?? ''),
                (string)($question['difficulty'] ?? 'Practice')
            ]);
        }
    }
}

function issueSession(PDO $pdo, int $accountId): string
{
    $token = bin2hex(random_bytes(32));
    $tokenHash = hash('sha256', $token);
    $insert = $pdo->prepare('INSERT INTO account_sessions (account_id, token_hash, expires_at) VALUES (?, ?, DATE_ADD(UTC_TIMESTAMP(), INTERVAL 30 DAY))');
    $insert->execute([$accountId, $tokenHash]);
    return $token;
}

function authenticatedAccount(PDO $pdo): array
{
    $header = $_SERVER['HTTP_AUTHORIZATION'] ?? '';
    if (!preg_match('/^Bearer\s+([a-f0-9]{64})$/i', $header, $matches)) {
        respond(['success' => false, 'error' => 'Vui lòng đăng nhập lại.'], 401);
    }
    $tokenHash = hash('sha256', $matches[1]);
    $query = $pdo->prepare('SELECT a.account_id, a.email, a.display_name FROM account_sessions s JOIN accounts a ON a.account_id = s.account_id WHERE s.token_hash = ? AND s.expires_at > UTC_TIMESTAMP() LIMIT 1');
    $query->execute([$tokenHash]);
    $account = $query->fetch();
    if (!$account) {
        respond(['success' => false, 'error' => 'Phiên đăng nhập đã hết hạn.'], 401);
    }
    return $account;
}

function requireServiceKey(): void
{
    $expected = getenv('CLASSROOM_QUIZ_SERVICE_KEY') ?: 'ClassroomQuiz-local-change-this-key';
    $received = $_SERVER['HTTP_X_CLASSROOMQUIZ_SERVICE_KEY'] ?? '';
    if (!hash_equals($expected, $received)) {
        respond(['success' => false, 'error' => 'Service authentication failed.'], 403);
    }
}

try {
    $input = readJson();
    $action = (string)($input['action'] ?? '');
    $pdo = database();

    if ($action === 'health' || ($_SERVER['REQUEST_METHOD'] === 'GET' && $action === '')) {
        respond(['success' => true, 'database' => 'classromquiz', 'message' => 'ClassroomQuiz XAMPP API is ready.']);
    }

    if ($action === 'register') {
        $email = strtolower(trim((string)($input['email'] ?? '')));
        $displayName = trim((string)($input['displayName'] ?? ''));
        $password = (string)($input['password'] ?? '');
        if (!filter_var($email, FILTER_VALIDATE_EMAIL) || strlen($email) > 254) {
            respond(['success' => false, 'error' => 'Gmail/email chưa đúng định dạng.'], 400);
        }
        if ($displayName === '' || mb_strlen($displayName) > 48) {
            respond(['success' => false, 'error' => 'Tên hiển thị cần từ 1 đến 48 ký tự.'], 400);
        }
        if (strlen($password) < 8 || strlen($password) > 128) {
            respond(['success' => false, 'error' => 'Mật khẩu cần từ 8 đến 128 ký tự.'], 400);
        }
        $insert = $pdo->prepare('INSERT INTO accounts (email, display_name, password_hash, avatar_key) VALUES (?, ?, ?, ?)');
        try {
            $insert->execute([$email, $displayName, password_hash($password, PASSWORD_DEFAULT), 'A']);
        } catch (PDOException $exception) {
            if ($exception->getCode() === '23000') {
                respond(['success' => false, 'error' => 'Email này đã có tài khoản.'], 409);
            }
            throw $exception;
        }
        $accountId = (int)$pdo->lastInsertId();
        respond(['success' => true, 'accountId' => $accountId, 'email' => $email, 'displayName' => $displayName, 'avatarKey' => 'A', 'token' => issueSession($pdo, $accountId)]);
    }

    if ($action === 'login') {
        $email = strtolower(trim((string)($input['email'] ?? '')));
        $password = (string)($input['password'] ?? '');
        $query = $pdo->prepare('SELECT account_id, email, display_name, avatar_key, password_hash FROM accounts WHERE email = ? LIMIT 1');
        $query->execute([$email]);
        $account = $query->fetch();
        if (!$account || !password_verify($password, $account['password_hash'])) {
            respond(['success' => false, 'error' => 'Email hoặc mật khẩu không đúng.'], 401);
        }
        $update = $pdo->prepare('UPDATE accounts SET last_login_at = UTC_TIMESTAMP() WHERE account_id = ?');
        $update->execute([$account['account_id']]);
        respond(['success' => true, 'accountId' => (int)$account['account_id'], 'email' => $account['email'], 'displayName' => $account['display_name'], 'avatarKey' => $account['avatar_key'], 'token' => issueSession($pdo, (int)$account['account_id'])]);
    }

    if ($action === 'verifyToken') {
        $token = (string)($input['token'] ?? '');
        $hash = hash('sha256', $token);
        $query = $pdo->prepare('SELECT a.account_id, a.email, a.display_name, a.avatar_key AS avatarKey FROM account_sessions s JOIN accounts a ON a.account_id = s.account_id WHERE s.token_hash = ? AND s.expires_at > UTC_TIMESTAMP() LIMIT 1');
        $query->execute([$hash]);
        $account = $query->fetch();
        if (!$account) {
            respond(['success' => false, 'error' => 'Phiên đăng nhập đã hết hạn.'], 401);
        }
        respond(['success' => true, 'accountId' => (int)$account['account_id'], 'email' => $account['email'], 'displayName' => $account['display_name'], 'avatarKey' => $account['avatarKey']]);
    }

    if ($action === 'updateAvatar') {
        $account = authenticatedAccount($pdo);
        $avatarKey = strtoupper(trim((string)($input['avatarKey'] ?? '')));
        if (!in_array($avatarKey, ['A', 'B', 'C', 'D'], true)) {
            respond(['success' => false, 'error' => 'Avatar phải là A, B, C hoặc D.'], 400);
        }
        $update = $pdo->prepare('UPDATE accounts SET avatar_key = ? WHERE account_id = ?');
        $update->execute([$avatarKey, $account['account_id']]);
        respond(['success' => true, 'avatarKey' => $avatarKey]);
    }

    if ($action === 'getLessonCatalog') {
        authenticatedAccount($pdo);
        $query = $pdo->query('SELECT lesson_id, lesson_title, lesson_summary, sequence_index, prompt, options_json, correct_index, explanation FROM lesson_questions WHERE is_active = 1 ORDER BY lesson_id, sequence_index');
        $lessons = [];
        foreach ($query->fetchAll() as $row) {
            $lessonId = (int)$row['lesson_id'];
            if (!isset($lessons[$lessonId])) {
                $lessons[$lessonId] = [
                    'lessonId' => $lessonId,
                    'title' => $row['lesson_title'],
                    'summary' => $row['lesson_summary'],
                    'questions' => []
                ];
            }
            $lessons[$lessonId]['questions'][] = [
                'lessonId' => $lessonId,
                'lessonTitle' => $row['lesson_title'],
                'questionId' => 'lesson-' . $lessonId . '-question-' . (int)$row['sequence_index'],
                'sequenceIndex' => (int)$row['sequence_index'],
                'prompt' => $row['prompt'],
                'options' => json_decode((string)$row['options_json'], true) ?: [],
                'correctIndex' => (int)$row['correct_index'],
                'explanation' => $row['explanation']
            ];
        }
        respond(['success' => true, 'lessons' => array_values($lessons)]);
    }

    if ($action === 'saveLessonQuestions') {
        authenticatedAccount($pdo);
        $lessonId = (int)($input['lessonId'] ?? 0);
        $title = trim((string)($input['title'] ?? ''));
        $summary = trim((string)($input['summary'] ?? ''));
        $questions = $input['questions'] ?? null;
        if ($lessonId < 1 || $lessonId > 42 || $title === '' || mb_strlen($title) > 160 || mb_strlen($summary) > 500
            || !is_array($questions) || count($questions) < 2 || count($questions) > 5) {
            respond(['success' => false, 'error' => 'Thông tin bài học cần tên hợp lệ và từ 2 đến 5 câu.'], 400);
        }
        foreach ($questions as $index => $question) {
            if (!is_array($question) || trim((string)($question['prompt'] ?? '')) === '' || mb_strlen((string)$question['prompt']) > 1200
                || !is_array($question['options'] ?? null) || count($question['options']) !== 4
                || (int)($question['correctIndex'] ?? -1) < 0 || (int)($question['correctIndex'] ?? -1) > 3
                || trim((string)($question['explanation'] ?? '')) === '') {
                respond(['success' => false, 'error' => 'Câu ' . ($index + 1) . ' chưa đủ nội dung, 4 đáp án, đáp án đúng hoặc lời giải.'], 400);
            }
            foreach ($question['options'] as $option) {
                if (trim((string)$option) === '') {
                    respond(['success' => false, 'error' => 'Cả 4 đáp án của mỗi câu đều phải có nội dung.'], 400);
                }
            }
        }
        $pdo->beginTransaction();
        $pdo->prepare('DELETE FROM lesson_questions WHERE lesson_id = ?')->execute([$lessonId]);
        $insert = $pdo->prepare('INSERT INTO lesson_questions (lesson_id, lesson_title, lesson_summary, sequence_index, prompt, options_json, correct_index, explanation, difficulty) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)');
        foreach ($questions as $index => $question) {
            $insert->execute([
                $lessonId,
                $title,
                $summary,
                $index + 1,
                trim((string)$question['prompt']),
                json_encode(array_map(static fn($value) => trim((string)$value), $question['options']), JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES),
                (int)$question['correctIndex'],
                trim((string)$question['explanation']),
                in_array($question['difficulty'] ?? '', ['Basic', 'Practice', 'Advanced'], true) ? $question['difficulty'] : 'Practice'
            ]);
        }
        $pdo->commit();
        respond(['success' => true, 'lessonId' => $lessonId, 'questionCount' => count($questions)]);
    }

    if ($action === 'listRooms') {
        $account = authenticatedAccount($pdo);
        $query = $pdo->prepare('SELECT r.room_id AS roomId, r.room_code AS roomCode, r.room_name AS roomName, r.lesson_title AS lessonTitle, r.mode, r.host_name AS hostName, DATE_FORMAT(r.created_at, "%Y-%m-%d %H:%i") AS createdAt, d.rank_position AS rankPosition FROM rooms r LEFT JOIN detail_room d ON d.room_id = r.room_id AND d.account_id = ? WHERE r.host_account_id = ? OR d.account_id = ? ORDER BY r.created_at DESC LIMIT 100');
        $query->execute([$account['account_id'], $account['account_id'], $account['account_id']]);
        respond(['success' => true, 'rooms' => $query->fetchAll()]);
    }

    if ($action === 'getRoomDetails') {
        $account = authenticatedAccount($pdo);
        $roomId = (int)($input['roomId'] ?? 0);
        $roomQuery = $pdo->prepare('SELECT room_id AS roomId, room_name AS roomName, room_code AS roomCode, lesson_title AS lessonTitle, host_name AS hostName, host_account_id AS hostAccountId FROM rooms WHERE room_id = ? LIMIT 1');
        $roomQuery->execute([$roomId]);
        $room = $roomQuery->fetch();
        if (!$room) {
            respond(['success' => false, 'error' => 'Không tìm thấy buổi học.'], 404);
        }
        $isHost = $room['hostAccountId'] !== null && (int)$room['hostAccountId'] === (int)$account['account_id'];
        if (!$isHost) {
            $membership = $pdo->prepare('SELECT 1 FROM detail_room WHERE room_id = ? AND account_id = ? LIMIT 1');
            $membership->execute([$roomId, $account['account_id']]);
            if (!$membership->fetchColumn()) {
                respond(['success' => false, 'error' => 'Bạn không tham gia buổi học này.'], 403);
            }
        }
        $sql = 'SELECT display_name AS displayName, rank_position AS rankPosition, correct_count AS correctCount, incorrect_count AS incorrectCount, unanswered_count AS unansweredCount, total_questions AS totalQuestions, score, response_time_ms AS responseTimeMs FROM detail_room WHERE room_id = ?';
        if (!$isHost) {
            $sql .= ' AND (rank_position = 1 OR account_id = ?)';
        }
        $sql .= ' ORDER BY rank_position ASC';
        $detailsQuery = $pdo->prepare($sql);
        $isHost ? $detailsQuery->execute([$roomId]) : $detailsQuery->execute([$roomId, $account['account_id']]);
        respond(['success' => true, 'isHost' => $isHost, 'room' => $room, 'details' => $detailsQuery->fetchAll()]);
    }

    if ($action === 'saveRoom') {
        requireServiceKey();
        $room = is_array($input['room'] ?? null) ? $input['room'] : [];
        $details = is_array($input['details'] ?? null) ? $input['details'] : [];
        $code = trim((string)($room['roomCode'] ?? ''));
        if ($code === '' || !isset($room['roomName'], $room['hostName'])) {
            respond(['success' => false, 'error' => 'Room record is incomplete.'], 400);
        }
        $pdo->beginTransaction();
        $insertRoom = $pdo->prepare('INSERT INTO rooms (room_code, room_name, lesson_title, mode, question_count, host_account_id, host_name, created_at) VALUES (?, ?, ?, ?, ?, ?, ?, UTC_TIMESTAMP())');
        $hostAccountId = (int)($room['hostAccountId'] ?? 0);
        $insertRoom->execute([$code, substr((string)$room['roomName'], 0, 120), substr((string)($room['lessonTitle'] ?? ''), 0, 160), substr((string)($room['mode'] ?? 'Study'), 0, 16), (int)($room['questionCount'] ?? 0), $hostAccountId > 0 ? $hostAccountId : null, substr((string)$room['hostName'], 0, 48)]);
        $roomId = (int)$pdo->lastInsertId();
        $insertDetail = $pdo->prepare('INSERT INTO detail_room (room_id, account_id, display_name, rank_position, correct_count, incorrect_count, unanswered_count, total_questions, score, response_time_ms) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)');
        foreach ($details as $detail) {
            if (!is_array($detail)) {
                continue;
            }
            $accountId = (int)($detail['accountId'] ?? 0);
            $insertDetail->execute([$roomId, $accountId > 0 ? $accountId : null, substr((string)($detail['displayName'] ?? 'Guest'), 0, 48), (int)($detail['rankPosition'] ?? 0), (int)($detail['correctCount'] ?? 0), (int)($detail['incorrectCount'] ?? 0), (int)($detail['unansweredCount'] ?? 0), (int)($detail['totalQuestions'] ?? 0), (int)($detail['score'] ?? 0), (int)($detail['responseTimeMs'] ?? 0)]);
        }
        $pdo->commit();
        respond(['success' => true, 'roomId' => $roomId]);
    }

    respond(['success' => false, 'error' => 'Unsupported action.'], 400);
} catch (Throwable $exception) {
    if (isset($pdo) && $pdo instanceof PDO && $pdo->inTransaction()) {
        $pdo->rollBack();
    }
    error_log('ClassroomQuiz API: ' . $exception->getMessage());
    respond(['success' => false, 'error' => 'Database service is unavailable.'], 500);
}
