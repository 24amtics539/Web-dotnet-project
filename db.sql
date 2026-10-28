-- =============================================================================
-- CampusDeadline — Database Script (MySQL)
-- Clean and simple SQL script for presentation and viva
-- =============================================================================

-- =============================================================================
-- 1. CREATE DATABASE
-- =============================================================================
CREATE DATABASE IF NOT EXISTS campusdeadline_db;
USE campusdeadline_db;

-- =============================================================================
-- 2. CREATE TABLES
-- =============================================================================

-- Table 1: Users (Stores student accounts)
CREATE TABLE IF NOT EXISTS users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    role VARCHAR(20) NOT NULL DEFAULT 'student',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Table 2: Subjects (Stores student courses/subjects)
CREATE TABLE IF NOT EXISTS subjects (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    subject_name VARCHAR(150) NOT NULL,
    subject_code VARCHAR(50),
    semester INT NOT NULL DEFAULT 1,
    color VARCHAR(20) NOT NULL DEFAULT '#B98AFF',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Table 3: Tasks (Stores assignments, projects, exams, labs)
CREATE TABLE IF NOT EXISTS tasks (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    subject_id INT NOT NULL,
    title VARCHAR(200) NOT NULL,
    description TEXT,
    task_type VARCHAR(50) NOT NULL DEFAULT 'Assignment',
    priority VARCHAR(20) NOT NULL DEFAULT 'medium',
    due_date DATETIME NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'pending',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (subject_id) REFERENCES subjects(id) ON DELETE CASCADE
);

-- Table 4: Task Status History (Logs when task status changes)
CREATE TABLE IF NOT EXISTS task_status_history (
    id INT AUTO_INCREMENT PRIMARY KEY,
    task_id INT NOT NULL,
    old_status VARCHAR(20),
    new_status VARCHAR(20) NOT NULL,
    changed_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (task_id) REFERENCES tasks(id) ON DELETE CASCADE
);


-- =============================================================================
-- 3. COMMANDS TO REMOVE ALL DATA FROM ALL TABLES (TRUNCATE)
-- =============================================================================
-- Disables foreign key checks temporarily to safely wipe all tables and reset IDs to 1
SET FOREIGN_KEY_CHECKS = 0;
TRUNCATE TABLE task_status_history;
TRUNCATE TABLE tasks;
TRUNCATE TABLE subjects;
TRUNCATE TABLE users;
SET FOREIGN_KEY_CHECKS = 1;


-- =============================================================================
-- 4. HOW TO SHOW TABLES AND SCHEMA (Explain to Teacher)
-- =============================================================================

-- Command 1: Show list of all tables in database
SHOW TABLES;

-- Command 2: Show structure / columns of each table
DESCRIBE users;
DESCRIBE subjects;
DESCRIBE tasks;
DESCRIBE task_status_history;


-- =============================================================================
-- 5. HOW TO SHOW DATA IN ALL TABLES (Explain to Teacher)
-- =============================================================================

-- 1. View all Users data (Currently empty: 0 rows)
SELECT * FROM users;

-- 2. View all Subjects data (Currently empty: 0 rows)
SELECT * FROM subjects;

-- 3. View all Tasks data (Currently empty: 0 rows)
SELECT * FROM tasks;

-- 4. View all Task Status History data (Currently empty: 0 rows)
SELECT * FROM task_status_history;
