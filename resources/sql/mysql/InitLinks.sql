CREATE TABLE IF NOT EXISTS links
(
    link           VARCHAR(12) PRIMARY KEY,
    user_id        BIGINT  NOT NULL,
    used           BIGINT  NOT NULL,
    max_usages     BIGINT,
    is_deactivated BOOLEAN NOT NULL,
    expires_at     TIMESTAMP(0),
    INDEX idx_user_id (user_id)
)
