CREATE DATABASE IF NOT EXISTS classromquiz
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE classromquiz;

CREATE TABLE IF NOT EXISTS accounts (
  account_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  email VARCHAR(254) NOT NULL,
  display_name VARCHAR(48) NOT NULL,
  password_hash VARCHAR(255) NOT NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  last_login_at DATETIME NULL,
  PRIMARY KEY (account_id),
  UNIQUE KEY uq_accounts_email (email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS account_sessions (
  session_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  account_id BIGINT UNSIGNED NOT NULL,
  token_hash CHAR(64) NOT NULL,
  expires_at DATETIME NOT NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (session_id),
  UNIQUE KEY uq_account_sessions_token_hash (token_hash),
  KEY ix_account_sessions_account (account_id),
  CONSTRAINT fk_account_sessions_account FOREIGN KEY (account_id)
    REFERENCES accounts (account_id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS rooms (
  room_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  room_code VARCHAR(12) NOT NULL,
  room_name VARCHAR(120) NOT NULL,
  lesson_title VARCHAR(160) NOT NULL,
  mode VARCHAR(16) NOT NULL,
  question_count SMALLINT UNSIGNED NOT NULL,
  host_account_id BIGINT UNSIGNED NULL,
  host_name VARCHAR(48) NOT NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (room_id),
  KEY ix_rooms_host_date (host_account_id, created_at),
  KEY ix_rooms_code_date (room_code, created_at),
  CONSTRAINT fk_rooms_host FOREIGN KEY (host_account_id)
    REFERENCES accounts (account_id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS detail_room (
  detail_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  room_id BIGINT UNSIGNED NOT NULL,
  account_id BIGINT UNSIGNED NULL,
  display_name VARCHAR(48) NOT NULL,
  rank_position SMALLINT UNSIGNED NOT NULL,
  correct_count SMALLINT UNSIGNED NOT NULL DEFAULT 0,
  incorrect_count SMALLINT UNSIGNED NOT NULL DEFAULT 0,
  unanswered_count SMALLINT UNSIGNED NOT NULL DEFAULT 0,
  total_questions SMALLINT UNSIGNED NOT NULL DEFAULT 0,
  score INT UNSIGNED NOT NULL DEFAULT 0,
  response_time_ms BIGINT UNSIGNED NOT NULL DEFAULT 0,
  PRIMARY KEY (detail_id),
  UNIQUE KEY uq_detail_room_account (room_id, account_id),
  KEY ix_detail_room_rank (room_id, rank_position),
  KEY ix_detail_room_account (account_id, room_id),
  CONSTRAINT fk_detail_room_room FOREIGN KEY (room_id)
    REFERENCES rooms (room_id) ON DELETE CASCADE,
  CONSTRAINT fk_detail_room_account FOREIGN KEY (account_id)
    REFERENCES accounts (account_id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
