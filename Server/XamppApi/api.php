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

    return $pdo;
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
        $insert = $pdo->prepare('INSERT INTO accounts (email, display_name, password_hash) VALUES (?, ?, ?)');
        try {
            $insert->execute([$email, $displayName, password_hash($password, PASSWORD_DEFAULT)]);
        } catch (PDOException $exception) {
            if ($exception->getCode() === '23000') {
                respond(['success' => false, 'error' => 'Email này đã có tài khoản.'], 409);
            }
            throw $exception;
        }
        $accountId = (int)$pdo->lastInsertId();
        respond(['success' => true, 'accountId' => $accountId, 'email' => $email, 'displayName' => $displayName, 'token' => issueSession($pdo, $accountId)]);
    }

    if ($action === 'login') {
        $email = strtolower(trim((string)($input['email'] ?? '')));
        $password = (string)($input['password'] ?? '');
        $query = $pdo->prepare('SELECT account_id, email, display_name, password_hash FROM accounts WHERE email = ? LIMIT 1');
        $query->execute([$email]);
        $account = $query->fetch();
        if (!$account || !password_verify($password, $account['password_hash'])) {
            respond(['success' => false, 'error' => 'Email hoặc mật khẩu không đúng.'], 401);
        }
        $update = $pdo->prepare('UPDATE accounts SET last_login_at = UTC_TIMESTAMP() WHERE account_id = ?');
        $update->execute([$account['account_id']]);
        respond(['success' => true, 'accountId' => (int)$account['account_id'], 'email' => $account['email'], 'displayName' => $account['display_name'], 'token' => issueSession($pdo, (int)$account['account_id'])]);
    }

    if ($action === 'verifyToken') {
        $token = (string)($input['token'] ?? '');
        $hash = hash('sha256', $token);
        $query = $pdo->prepare('SELECT a.account_id, a.email, a.display_name FROM account_sessions s JOIN accounts a ON a.account_id = s.account_id WHERE s.token_hash = ? AND s.expires_at > UTC_TIMESTAMP() LIMIT 1');
        $query->execute([$hash]);
        $account = $query->fetch();
        if (!$account) {
            respond(['success' => false, 'error' => 'Phiên đăng nhập đã hết hạn.'], 401);
        }
        respond(['success' => true, 'accountId' => (int)$account['account_id'], 'email' => $account['email'], 'displayName' => $account['display_name']]);
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
