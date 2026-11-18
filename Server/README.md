# ClassroomQuiz TCP server and XAMPP API

The Unity clients connect to the .NET 8 TCP server on port `7777`. The TCP server uses the PHP API hosted by XAMPP for account validation and for saving each completed room run.

## Start XAMPP and create the database

1. Install XAMPP on the teacher's computer and start **Apache** and **MySQL** in the XAMPP Control Panel.
2. From the project root, run:

   ```bat
   install_xampp_api.bat
   ```

   Apache on this computer uses `D:\webserver` as its `DocumentRoot`, so the default destination is `D:\webserver\ClassroomQuizApi`. Pass a different web folder only if that setting changes:

   ```bat
   install_xampp_api.bat D:\webserver\ClassroomQuizApi
   ```

3. Open `http://127.0.0.1/ClassroomQuizApi/api.php` in a browser; it should show that the API is ready. On the first API request, PHP creates the `classromquiz` database and its tables automatically. `Server\XamppApi\schema.sql` can also be imported with phpMyAdmin.
4. On a computer running XAMPP, the login screen's default API URL is `http://127.0.0.1/ClassroomQuizApi/api.php`. On each student computer, enter the teacher computer's LAN IPv4 address instead, for example `http://192.168.1.20/ClassroomQuizApi/api.php`.

The schema contains `accounts` (email, display name, avatar key, and password hash), `account_sessions`, `lesson_questions`, `rooms`, and `detail_room`. On the first API request, `lesson_questions_seed.json` adds 30 editable quiz questions across six sample lessons. Passwords are hashed with PHP's `password_hash`; the game client sends the resulting session token to the TCP server, never the password. A room run is saved when it ends. Restarting the room runs and saves another result row.

The API assumes XAMPP's default MySQL account (`root` with no password). Set `CLASSROOMQUIZ_DB_HOST`, `CLASSROOMQUIZ_DB_USER`, and `CLASSROOMQUIZ_DB_PASSWORD` in Apache's environment if your MySQL credentials differ.

## Build and run the TCP server

Requirements: Windows 10/11, .NET 8 SDK, and TCP port `7777` available.

From the project root:

```bat
build_server.bat
run_server.bat
```

The server binds to `0.0.0.0:7777`. You can override the address and port with:

```bat
run_server.bat --ip 0.0.0.0 --port 7777
```

You can also set `CLASSROOM_QUIZ_BIND_IP`, `CLASSROOM_QUIZ_PORT`, `CLASSROOM_QUIZ_MAX_STUDENTS`, and `CLASSROOM_QUIZ_DB_API`. The student limit defaults to 16 (the 4×4 classroom seats) and does not include the teacher. The default API endpoint is `http://127.0.0.1/ClassroomQuizApi/api.php`.

The server and PHP API share `CLASSROOM_QUIZ_SERVICE_KEY` to authorize saving room records. They use the same development default, `ClassroomQuiz-local-change-this-key`; change it in both the server process environment and Apache's environment before exposing the API outside a trusted local network.

## LAN setup

1. Start Apache and MySQL, then run the TCP server on the teacher's host computer.
2. Find that computer's IPv4 address with Windows Network Settings or `ipconfig`.
3. On each client, set the TCP server address to the teacher's IPv4 address in ClassroomQuiz Settings. On the login screen, also set the API URL to `http://<teacher-ip>/ClassroomQuizApi/api.php`.
4. Allow inbound HTTP traffic for Apache and TCP traffic on port `7777` through Windows Firewall for the private network.
5. Sign in on the teacher's client, create a room, and have at least two signed-in student clients join with its six-character code. The teacher never enters the question screen; the teacher sees the live leaderboard dashboard.

`127.0.0.1` only connects to services on the same computer. Clients on other computers must use the teacher host's LAN IPv4 address.

## Game rules and results

- The server requires one teacher and at least two students before starting. It validates room options and question sets, generates unique room codes without O/0/I/1, accepts one answer per student per question, and uses a shared wall-clock timer.
- A lesson is one ordered sequence of 2–5 questions. Correct answers earn 1,000 base points plus up to 1,000 speed points; incorrect answers earn no points. After answering, the student sees an explanation until the shared timer ends.
- At question end, the teacher's live panel and each classroom seat show that question's points for three seconds before the next question starts.
- The teacher sees the live full-class leaderboard. At the ending screen, each student sees their own rank and the top student; only the teacher sees the full ranking. Only the teacher can restart the room, and each completed run is stored as another room history row.
- The room history menu lists the rooms an account hosted or joined. Opening a room shows its ranking, correct and incorrect counts, response time, and score. The API only returns full details to that room's teacher and returns the top student plus the requesting student to other participants.
- Study materials are a separate, untimed library with worked examples and practice prompts. They do not use the room question set.

## Protocol

Messages are UTF-8 JSON preceded by a four-byte little-endian payload length. Frames larger than 1 MiB are rejected. Protocol version 1 covers authentication, room lifecycle, countdowns, questions, answers, results, session end, and Ping/Pong messages. Account identity and display name come from the verified XAMPP session.
